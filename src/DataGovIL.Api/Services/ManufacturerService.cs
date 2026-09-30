using System.Globalization;
using DataGovIL.Api.Models;
using DataGovIL.Client;
using DataGovIL.Client.Models;
using Microsoft.Extensions.Options;

namespace DataGovIL.Api.Services;

public interface IManufacturerService
{
    /// <summary>
    /// Returns a page of (manufacturer, model) rows from the WLTP makes/models resource.
    /// Optionally filter to a manufacturer name and/or search by commercial model name.
    /// </summary>
    Task<PagedResult<ManufacturerModelRecord>> GetManufacturersAndModelsAsync(
        string? manufacturerName, string? modelName, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Looks up a single WLTP make/model row by exact manufacturer code (<c>tozeret_cd</c>)
    /// and model code (<c>degem_cd</c>).
    /// </summary>
    Task<ManufacturerModelRecord?> GetByCodesAsync(
        string manufacturerCode, string modelCode, CancellationToken ct = default);
}

public class ManufacturerService : IManufacturerService
{
    private readonly ICkanApiClient _client;
    private readonly VehicleDataResourceOptions _options;

    public ManufacturerService(ICkanApiClient client, IOptions<VehicleDataResourceOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<PagedResult<ManufacturerModelRecord>> GetManufacturersAndModelsAsync(
        string? manufacturerName, string? modelName, int page, int pageSize, CancellationToken ct = default)
    {
        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.WltpMakeModelResourceId,
            IncludeTotal = true,
            Sort = "tozeret_nm asc"
        }.WithPage(page, pageSize);

        if (!string.IsNullOrWhiteSpace(manufacturerName))
        {
            query.Filters = new Dictionary<string, object>
            {
                ["tozeret_nm"] = manufacturerName.Trim()
            };
        }

        if (!string.IsNullOrWhiteSpace(modelName))
        {
            // Full-text on commercial name so "COROLLA" also matches "COROLLA CROSS" / "COROLLA RUNX".
            query.FieldQuery = new Dictionary<string, string>
            {
                ["kinuy_mishari"] = modelName.Trim()
            };
        }

        var result = await _client.DatastoreSearchAsync<ManufacturerModelDatastoreRecord>(query, ct);

        return new PagedResult<ManufacturerModelRecord>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = result.Total,
            Items = result.Records.Select(ManufacturerModelRecord.FromDatastore).ToList()
        };
    }

    public async Task<ManufacturerModelRecord?> GetByCodesAsync(
        string manufacturerCode, string modelCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(manufacturerCode))
            throw new ArgumentException("Manufacturer code is required.", nameof(manufacturerCode));
        if (string.IsNullOrWhiteSpace(modelCode))
            throw new ArgumentException("Model code is required.", nameof(modelCode));

        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.WltpMakeModelResourceId,
            Filters = new Dictionary<string, object>
            {
                ["tozeret_cd"] = ToNumericFilterValue(manufacturerCode.Trim()),
                ["degem_cd"] = ToNumericFilterValue(modelCode.Trim())
            },
            Limit = 1
        };

        var result = await _client.DatastoreSearchAsync<ManufacturerModelDatastoreRecord>(query, ct);
        var row = result.Records.FirstOrDefault();
        return row is null ? null : ManufacturerModelRecord.FromDatastore(row);
    }

    /// <summary>
    /// <c>tozeret_cd</c> / <c>degem_cd</c> are numeric in the datastore; send a JSON number when the
    /// path segment parses as one so CKAN exact-match filters succeed.
    /// </summary>
    private static object ToNumericFilterValue(string value)
        => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : value;
}
