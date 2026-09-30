using System.Globalization;
using DataGovIL.Api.Data;
using DataGovIL.Api.Models;
using DataGovIL.Client;
using DataGovIL.Client.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DataGovIL.Api.Services;

public interface IVehicleService
{
    /// <summary>
    /// Looks up a vehicle by exact registration number. Checks the active private/commercial
    /// resources first, then permanently-cancelled ("ביטול סופי") resources, then inactive
    /// ("לא פעיל") resources, then personal-import vehicles. A found vehicle is also checked against the safety-systems
    /// discount list and the open-recall list, and gets a depreciation calculation from its history and price list
    /// plus an estimated monthly energy cost at the default mileage and an estimated annual license fee.
    /// </summary>
    Task<VehicleRecord?> SearchByRegistrationNumberAsync(string registrationNumber, CancellationToken ct = default);

    /// <summary>Paged/free-text search across the primary vehicle resource (e.g. by manufacturer name).</summary>
    Task<PagedResult<VehicleRecord>> SearchAsync(string? freeText, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Loads technical + ownership history for a registration number from the two history
    /// datastore resources. Returns null when neither resource has a matching row.
    /// </summary>
    Task<VehicleHistoryRecord?> GetHistoryByRegistrationNumberAsync(
        string registrationNumber, CancellationToken ct = default);
}

public class VehicleService : IVehicleService
{
    private readonly ICkanApiClient _client;
    private readonly VehicleDataResourceOptions _options;
    private readonly SystemOptions _systemOptions;
    private readonly AppDbContext _db;
    private readonly IPriceListService _priceListService;
    private readonly IVehicleEnergyCostService _energyCostService;

    public VehicleService(
        ICkanApiClient client,
        IOptions<VehicleDataResourceOptions> options,
        IOptions<SystemOptions> systemOptions,
        AppDbContext db,
        IPriceListService priceListService,
        IVehicleEnergyCostService energyCostService)
    {
        _client = client;
        _options = options.Value;
        _systemOptions = systemOptions.Value;
        _db = db;
        _priceListService = priceListService;
        _energyCostService = energyCostService;
    }

    public async Task<VehicleRecord?> SearchByRegistrationNumberAsync(string registrationNumber, CancellationToken ct = default)
    {
        var normalized = NormalizeRegistrationNumber(registrationNumber);

        var found = await FindActiveInResourceAsync(_options.PrivateAndCommercialVehiclesResourceId, normalized, ct)
            ?? await FindContinuationAsync(normalized, ct)
            ?? await FindCancelledInResourceAsync(
                _options.PermanentlyCancelledVehiclesResourceId, normalized, preferNumericFilter: true, ct)
            ?? await FindCancelledInResourceAsync(
                _options.PermanentlyCancelledVehicles2010To2016ResourceId, normalized, preferNumericFilter: false, ct)
            ?? await FindCancelledInResourceAsync(
                _options.PermanentlyCancelledVehicles2000To2009ResourceId, normalized, preferNumericFilter: false, ct)
            ?? await FindInactiveWithModelCodeAsync(normalized, ct)
            ?? await FindInactiveWithoutModelCodeAsync(normalized, ct)
            ?? await FindPersonalImportAsync(normalized, ct);

        if (found is not null)
        {
            var discount = await FindSafetyDiscountAsync(normalized, ct);
            if (discount is not null)
            {
                found.IsSafetyDiscountEligible = true;
                found.UpdatedDate = discount.UpdatedDate;
            }

            var recalls = await FindRecallsAsync(normalized, ct);
            if (recalls.Count > 0)
                found.Recalls = recalls;

            var catalog = _systemOptions.UseDB
                ? await FindManufacturerModelFromDbAsync(found, ct)
                : await FindManufacturerModelFromApiAsync(found, ct);
            if (catalog is not null)
                found.ManufacturerModel = catalog;

            if (found.ManufacturerModel is not null)
                found.EnergyCost = _energyCostService.CalculateCost(found.ManufacturerModel);

            await AttachHistoryAndDepreciationAsync(found, normalized, ct);
        }

        return found;
    }

    private async Task AttachHistoryAndDepreciationAsync(
        VehicleRecord vehicle, string registrationNumber, CancellationToken ct)
    {
        var plateFilter = ToNumericFilterValue(registrationNumber);
        var technicalTask = FetchTechnicalHistoryAsync(plateFilter, ct);
        var ownershipTask = FetchOwnershipHistoryAsync(plateFilter, ct);
        var priceListTask = FindPriceListAsync(vehicle, ct);
        await Task.WhenAll(technicalTask, ownershipTask, priceListTask).ConfigureAwait(false);

        var history = new VehicleHistoryRecord
        {
            RegistrationNumber = registrationNumber,
            Technical = await technicalTask.ConfigureAwait(false),
            OwnershipHistory = await ownershipTask.ConfigureAwait(false)
        };
        if (history.Technical is not null || history.OwnershipHistory.Count > 0)
            vehicle.History = history;

        vehicle.Depreciation = VehicleDepreciationCalculator.Calculate(
            vehicle, history, await priceListTask.ConfigureAwait(false), DateTime.Today);

        if (vehicle.ManufacturerModel is { } model)
            model.EstimatedLicenseFee = EstimateLicenseFee(model, vehicle, history, DateTime.Today.Year);
    }

    /// <summary>
    /// Uses the first registration date (<c>rishum_rishon_dt</c>), falling back to the road entry date
    /// (<c>moed_aliya_lakvish</c>). The manufacture year is not a substitute; without a date the fee is null.
    /// </summary>
    private static decimal? EstimateLicenseFee(
        ManufacturerModelRecord model, VehicleRecord vehicle, VehicleHistoryRecord history, int feeYear)
    {
        if (!TryParseInt(model.FeeGroupCode, out var feeGroup))
            return null;

        var firstRegistration = VehicleDepreciationCalculator.ParseYearMonth(history.Technical?.FirstRegistrationDate)
            ?? VehicleDepreciationCalculator.ParseYearMonth(vehicle.RoadEntryDate);
        return firstRegistration is DateTime date
            ? LicenseFeeTable.GetLicenseFee(feeGroup, date, feeYear)
            : null;
    }

    /// <summary>
    /// Price-list rows for the vehicle's manufacturer, model, and year. Prefers the same model type,
    /// then the same commercial name, among rows that have a price.
    /// </summary>
    private async Task<VehiclePriceListRecord?> FindPriceListAsync(VehicleRecord vehicle, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.NewVehiclePriceListResourceId)
            || string.IsNullOrWhiteSpace(vehicle.ManufacturerCode)
            || string.IsNullOrWhiteSpace(vehicle.ModelCode)
            || string.IsNullOrWhiteSpace(vehicle.ManufactureYear))
        {
            return null;
        }

        var rows = await _priceListService.GetByCodesAndYearAsync(
            vehicle.ManufacturerCode, vehicle.ModelCode, vehicle.ManufactureYear, ct);
        var priced = rows.Where(r => !string.IsNullOrWhiteSpace(r.Price)).ToList();
        if (priced.Count == 0)
            return null;

        var modelType = vehicle.ModelType?.Trim();
        if (!string.IsNullOrEmpty(modelType))
        {
            var byType = priced
                .Where(r => string.Equals(r.ModelType?.Trim(), modelType, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (byType.Count > 0)
                priced = byType;
        }

        return PickCatalogRow(priced, vehicle, _ => null, r => r.CommercialName);
    }

    private async Task<VehicleRecord?> FindContinuationAsync(string registrationNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.PrivateAndCommercialVehiclesContinuationResourceId))
            return null;

        return await FindActiveInResourceAsync(
            _options.PrivateAndCommercialVehiclesContinuationResourceId, registrationNumber, ct);
    }

    public async Task<PagedResult<VehicleRecord>> SearchAsync(string? freeText, int page, int pageSize, CancellationToken ct = default)
    {
        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.PrivateAndCommercialVehiclesResourceId,
            Q = string.IsNullOrWhiteSpace(freeText) ? null : freeText,
            IncludeTotal = true
        }.WithPage(page, pageSize);

        var result = await _client.DatastoreSearchAsync<VehicleDatastoreRecord>(query, ct);

        return new PagedResult<VehicleRecord>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = result.Total,
            Items = result.Records.Select(VehicleRecord.FromDatastore).ToList()
        };
    }

    public async Task<VehicleHistoryRecord?> GetHistoryByRegistrationNumberAsync(
        string registrationNumber, CancellationToken ct = default)
    {
        var normalized = NormalizeRegistrationNumber(registrationNumber);
        var plateFilter = ToNumericFilterValue(normalized);

        var technicalTask = FetchTechnicalHistoryAsync(plateFilter, ct);
        var ownershipTask = FetchOwnershipHistoryAsync(plateFilter, ct);
        await Task.WhenAll(technicalTask, ownershipTask).ConfigureAwait(false);

        var technical = await technicalTask.ConfigureAwait(false);
        var ownership = await ownershipTask.ConfigureAwait(false);

        if (technical is null && ownership.Count == 0)
            return null;

        return new VehicleHistoryRecord
        {
            RegistrationNumber = normalized,
            Technical = technical,
            OwnershipHistory = ownership
        };
    }

    private async Task<VehicleTechnicalHistoryRecord?> FetchTechnicalHistoryAsync(
        object plateFilter, CancellationToken ct)
    {
        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.VehicleTechnicalHistoryResourceId,
            Filters = new Dictionary<string, object>
            {
                ["mispar_rechev"] = plateFilter
            },
            Limit = 1
        };

        var result = await _client.DatastoreSearchAsync<VehicleTechnicalHistoryDatastoreRecord>(query, ct);
        var row = result.Records.FirstOrDefault();
        return row is null ? null : VehicleTechnicalHistoryRecord.FromDatastore(row);
    }

    private async Task<List<VehicleOwnershipHistoryRecord>> FetchOwnershipHistoryAsync(
        object plateFilter, CancellationToken ct)
    {
        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.VehicleOwnershipHistoryResourceId,
            Filters = new Dictionary<string, object>
            {
                ["mispar_rechev"] = plateFilter
            },
            Sort = "baalut_dt desc",
            Limit = 100
        };

        var result = await _client.DatastoreSearchAsync<VehicleOwnershipHistoryDatastoreRecord>(query, ct);
        return result.Records.Select(VehicleOwnershipHistoryRecord.FromDatastore).ToList();
    }

    private async Task<VehicleRecord?> FindActiveInResourceAsync(string resourceId, string registrationNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resourceId)) return null;

        var query = new DatastoreSearchQuery
        {
            ResourceId = resourceId,
            Filters = new Dictionary<string, object>
            {
                ["mispar_rechev"] = ToNumericFilterValue(registrationNumber)
            },
            Limit = 1
        };

        var result = await _client.DatastoreSearchAsync<VehicleDatastoreRecord>(query, ct);
        var row = result.Records.FirstOrDefault();
        return row is null ? null : VehicleRecord.FromDatastore(row);
    }

    private async Task<VehicleRecord?> FindCancelledInResourceAsync(
        string resourceId, string registrationNumber, bool preferNumericFilter, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(resourceId)) return null;

        var filterValue = preferNumericFilter
            ? ToNumericFilterValue(registrationNumber)
            : registrationNumber;

        var query = new DatastoreSearchQuery
        {
            ResourceId = resourceId,
            Filters = new Dictionary<string, object>
            {
                ["mispar_rechev"] = filterValue
            },
            Limit = 1
        };

        var result = await _client.DatastoreSearchAsync<VehicleCancelledDatastoreRecord>(query, ct);
        var row = result.Records.FirstOrDefault();
        return row is null ? null : VehicleRecord.FromCancelled(row);
    }

    /// <summary>
    /// Inactive registrations with a model code use the same columns as the active registry.
    /// </summary>
    private async Task<VehicleRecord?> FindInactiveWithModelCodeAsync(string registrationNumber, CancellationToken ct)
    {
        var found = await FindActiveInResourceAsync(_options.InactiveVehiclesWithModelCodeResourceId, registrationNumber, ct);
        if (found is not null)
            found.IsInactive = true;
        return found;
    }

    private async Task<VehicleRecord?> FindInactiveWithoutModelCodeAsync(string registrationNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.InactiveVehiclesWithoutModelCodeResourceId))
            return null;

        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.InactiveVehiclesWithoutModelCodeResourceId,
            Filters = new Dictionary<string, object>
            {
                ["mispar_rechev"] = ToNumericFilterValue(registrationNumber)
            },
            Limit = 1
        };

        var result = await _client.DatastoreSearchAsync<VehicleInactiveWithoutModelCodeDatastoreRecord>(query, ct);
        var row = result.Records.FirstOrDefault();
        return row is null ? null : VehicleRecord.FromInactiveWithoutModelCode(row);
    }

    private async Task<VehicleRecord?> FindPersonalImportAsync(string registrationNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.PersonalImportVehiclesResourceId))
            return null;

        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.PersonalImportVehiclesResourceId,
            Filters = new Dictionary<string, object>
            {
                ["mispar_rechev"] = ToNumericFilterValue(registrationNumber)
            },
            Limit = 1
        };

        var result = await _client.DatastoreSearchAsync<VehiclePersonalImportDatastoreRecord>(query, ct);
        var row = result.Records.FirstOrDefault();
        return row is null ? null : VehicleRecord.FromPersonalImport(row);
    }

    private async Task<VehicleRecord?> FindSafetyDiscountAsync(string registrationNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.SafetyDiscountVehiclesResourceId))
            return null;

        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.SafetyDiscountVehiclesResourceId,
            Filters = new Dictionary<string, object>
            {
                ["mispar_rechev"] = ToNumericFilterValue(registrationNumber)
            },
            Limit = 1
        };

        var result = await _client.DatastoreSearchAsync<VehicleSafetyDiscountDatastoreRecord>(query, ct);
        var row = result.Records.FirstOrDefault();
        return row is null ? null : VehicleRecord.FromSafetyDiscount(row);
    }

    private async Task<List<VehicleRecallRecord>> FindRecallsAsync(string registrationNumber, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.VehicleRecallsResourceId))
            return [];

        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.VehicleRecallsResourceId,
            Filters = new Dictionary<string, object>
            {
                ["MISPAR_RECHEV"] = ToNumericFilterValue(registrationNumber)
            },
            Sort = "TAARICH_PTICHA desc",
            Limit = 100
        };

        var result = await _client.DatastoreSearchAsync<VehicleRecallDatastoreRecord>(query, ct);
        return result.Records.Select(VehicleRecallRecord.FromDatastore).ToList();
    }

    /// <summary>
    /// Loads the WLTP catalog row from data.gov.il. Active registrations match
    /// manufacturer, model, year, and model type. Cancelled rows have no model type,
    /// so they match the other three and prefer the same trim or commercial name.
    /// </summary>
    private async Task<ManufacturerModelRecord?> FindManufacturerModelFromApiAsync(
        VehicleRecord vehicle, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(vehicle.ManufacturerCode)
            || string.IsNullOrWhiteSpace(vehicle.ModelCode)
            || string.IsNullOrWhiteSpace(vehicle.ManufactureYear))
        {
            return null;
        }

        var filters = new Dictionary<string, object>
        {
            ["tozeret_cd"] = ToNumericFilterValue(vehicle.ManufacturerCode.Trim()),
            ["degem_cd"] = ToNumericFilterValue(vehicle.ModelCode.Trim()),
            ["shnat_yitzur"] = ToNumericFilterValue(vehicle.ManufactureYear.Trim())
        };

        var modelType = vehicle.ModelType?.Trim();
        if (!string.IsNullOrEmpty(modelType))
            filters["sug_degem"] = modelType;

        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.WltpMakeModelResourceId,
            Filters = filters,
            Limit = string.IsNullOrEmpty(modelType) ? 20 : 1,
            Sort = "sug_degem asc"
        };

        var result = await _client.DatastoreSearchAsync<ManufacturerModelDatastoreRecord>(query, ct);
        var row = PickCatalogRow(result.Records, vehicle, r => r.TrimLevel, r => r.CommercialName);
        return row is null ? null : ManufacturerModelRecord.FromDatastore(row);
    }

    /// <summary>
    /// Loads the WLTP catalog row from Postgres. Match rules match
    /// <see cref="FindManufacturerModelFromApiAsync"/>.
    /// </summary>
    private async Task<ManufacturerModelRecord?> FindManufacturerModelFromDbAsync(VehicleRecord vehicle, CancellationToken ct)
    {
        if (!TryParseInt(vehicle.ManufacturerCode, out var manufacturerCode)
            || !TryParseInt(vehicle.ModelCode, out var modelCode)
            || !TryParseInt(vehicle.ManufactureYear, out var modelYear))
        {
            return null;
        }

        var query = _db.ManufacturerModels.AsNoTracking()
            .Where(m => m.ManufacturerCode == manufacturerCode
                        && m.ModelCode == modelCode
                        && m.ModelYear == modelYear);

        var modelType = vehicle.ModelType?.Trim();
        if (!string.IsNullOrEmpty(modelType))
            query = query.Where(m => m.ModelType == modelType);

        var rows = await query.OrderBy(m => m.ModelType).Take(20).ToListAsync(ct);
        var row = PickCatalogRow(rows, vehicle, r => r.TrimLevel, r => r.CommercialName);
        if (row is null)
            return null;

        var manufacturer = await _db.Manufacturers.AsNoTracking()
            .FirstOrDefaultAsync(m => m.ManufacturerCode == manufacturerCode, ct);
        var texts = await LoadCatalogTextsAsync(row, ct);
        return ManufacturerModelRecord.FromEntity(row, manufacturer, texts);
    }

    private static T? PickCatalogRow<T>(
        IReadOnlyList<T> rows,
        VehicleRecord vehicle,
        Func<T, string?> trimLevel,
        Func<T, string?> commercialName) where T : class
    {
        if (rows.Count == 0)
            return null;
        if (rows.Count == 1)
            return rows[0];

        var candidates = rows;
        var trim = vehicle.TrimLevel?.Trim();
        if (!string.IsNullOrEmpty(trim))
        {
            var byTrim = candidates
                .Where(r => string.Equals(trimLevel(r)?.Trim(), trim, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (byTrim.Count == 1)
                return byTrim[0];
            if (byTrim.Count > 1)
                candidates = byTrim;
        }

        var commercial = vehicle.CommercialName?.Trim();
        if (!string.IsNullOrEmpty(commercial))
        {
            var byName = candidates.FirstOrDefault(r =>
                string.Equals(commercialName(r)?.Trim(), commercial, StringComparison.OrdinalIgnoreCase));
            if (byName is not null)
                return byName;
        }

        return candidates[0];
    }

    private async Task<Dictionary<(int TableId, int Id), string>> LoadCatalogTextsAsync(
        ManufacturerModelEntity row, CancellationToken ct)
    {
        var keys = new List<(int TableId, int Id)>();
        void Add(TextTable table, int? id)
        {
            if (id is int value)
                keys.Add(((int)table, value));
        }

        Add(TextTable.Drive, row.DriveCode);
        Add(TextTable.Fuel, row.FuelCode);
        Add(TextTable.HomologationType, row.HomologationTypeCode);
        Add(TextTable.ConverterType, row.ConverterTypeCode);
        Add(TextTable.DriveTechnology, row.DriveTechnologyCode);
        Add(TextTable.BodyType, row.BodyType);

        if (keys.Count == 0)
            return new Dictionary<(int TableId, int Id), string>();

        // List.Contains stays List<int>.Contains. int[].Contains is compiled as
        // MemoryExtensions.Contains(ReadOnlySpan<int>, int), which EF cannot evaluate.
        var tableIds = keys.Select(k => k.TableId).Distinct().ToList();
        var ids = keys.Select(k => k.Id).Distinct().ToList();
        var texts = await _db.Texts.AsNoTracking()
            .Where(t => tableIds.Contains(t.TableId) && ids.Contains(t.Id))
            .ToListAsync(ct);

        return texts.ToDictionary(t => (t.TableId, t.Id), t => t.Text);
    }

    private static bool TryParseInt(string? raw, out int value)
    {
        value = 0;
        if (!decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            || parsed != decimal.Truncate(parsed)
            || parsed is < int.MinValue or > int.MaxValue)
        {
            return false;
        }

        value = (int)parsed;
        return true;
    }

    private static object ToNumericFilterValue(string value)
        => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : value;

    /// <summary>Keeps only digit characters (e.g. <c>12-345-67</c> → <c>1234567</c>).</summary>
    private static string NormalizeRegistrationNumber(string registrationNumber)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new ArgumentException("Registration number is required.", nameof(registrationNumber));

        var digits = new string(registrationNumber.Where(char.IsAsciiDigit).ToArray());
        if (digits.Length == 0)
            throw new ArgumentException("Registration number must contain digits.", nameof(registrationNumber));

        return digits;
    }
}
