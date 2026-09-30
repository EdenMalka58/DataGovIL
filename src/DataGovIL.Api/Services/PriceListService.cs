using System.Globalization;
using DataGovIL.Api.Models;
using DataGovIL.Client;
using DataGovIL.Client.Models;
using Microsoft.Extensions.Options;

namespace DataGovIL.Api.Services;

public interface IPriceListService
{
    /// <summary>
    /// Looks up new-vehicle price-list rows by manufacturer code (<c>tozeret_cd</c>),
    /// model code (<c>degem_cd</c>), and manufacture year (<c>shnat_yitzur</c>).
    /// </summary>
    Task<IReadOnlyList<VehiclePriceListRecord>> GetByCodesAndYearAsync(
        string manufacturerCode,
        string modelCode,
        string manufactureYear,
        CancellationToken ct = default);
}

public class PriceListService : IPriceListService
{
    private readonly ICkanApiClient _client;
    private readonly VehicleDataResourceOptions _options;

    public PriceListService(ICkanApiClient client, IOptions<VehicleDataResourceOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<VehiclePriceListRecord>> GetByCodesAndYearAsync(
        string manufacturerCode,
        string modelCode,
        string manufactureYear,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(manufacturerCode))
            throw new ArgumentException("Manufacturer code is required.", nameof(manufacturerCode));
        if (string.IsNullOrWhiteSpace(modelCode))
            throw new ArgumentException("Model code is required.", nameof(modelCode));
        if (string.IsNullOrWhiteSpace(manufactureYear))
            throw new ArgumentException("Manufacture year is required.", nameof(manufactureYear));

        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.NewVehiclePriceListResourceId,
            Filters = new Dictionary<string, object>
            {
                ["tozeret_cd"] = ToNumericFilterValue(manufacturerCode.Trim()),
                ["degem_cd"] = ToNumericFilterValue(modelCode.Trim()),
                ["shnat_yitzur"] = ToNumericFilterValue(manufactureYear.Trim())
            },
            Limit = 100
        };

        var result = await _client.DatastoreSearchAsync<VehiclePriceListDatastoreRecord>(query, ct);
        return result.Records.Select(VehiclePriceListRecord.FromDatastore).ToList();
    }

    private static object ToNumericFilterValue(string value)
        => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : value;
}
