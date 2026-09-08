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

        var result = await _client.DatastoreSearchAsync<ManufacturerModelRecord>(query, ct);

        return new PagedResult<ManufacturerModelRecord>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = result.Total,
            Items = result.Records
        };
    }
}
