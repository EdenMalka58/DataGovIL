using DataGovIL.Api.Models;
using DataGovIL.Client;
using DataGovIL.Client.Models;
using Microsoft.Extensions.Options;

namespace DataGovIL.Api.Services;

public interface IVehicleService
{
    /// <summary>
    /// Looks up a vehicle by exact registration number. The source dataset is split into two
    /// datastore resources on the portal ("...continued"), so this checks the primary resource
    /// first and falls back to the continuation resource if nothing is found there.
    /// </summary>
    Task<VehicleRecord?> SearchByRegistrationNumberAsync(string registrationNumber, CancellationToken ct = default);

    /// <summary>Free-text/paged search across the primary vehicle resource (e.g. by manufacturer name).</summary>
    Task<PagedResult<VehicleRecord>> SearchAsync(string? freeText, int page, int pageSize, CancellationToken ct = default);
}

public class VehicleService : IVehicleService
{
    private readonly ICkanApiClient _client;
    private readonly VehicleDataResourceOptions _options;

    public VehicleService(ICkanApiClient client, IOptions<VehicleDataResourceOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task<VehicleRecord?> SearchByRegistrationNumberAsync(string registrationNumber, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(registrationNumber))
            throw new ArgumentException("Registration number is required.", nameof(registrationNumber));

        var normalized = registrationNumber.Trim();

        var found = await FindInResourceAsync(_options.PrivateAndCommercialVehiclesResourceId, normalized, ct);
        if (found is not null) return found;

        if (!string.IsNullOrWhiteSpace(_options.PrivateAndCommercialVehiclesContinuationResourceId))
        {
            found = await FindInResourceAsync(_options.PrivateAndCommercialVehiclesContinuationResourceId!, normalized, ct);
        }

        return found;
    }

    public async Task<PagedResult<VehicleRecord>> SearchAsync(string? freeText, int page, int pageSize, CancellationToken ct = default)
    {
        var query = new DatastoreSearchQuery
        {
            ResourceId = _options.PrivateAndCommercialVehiclesResourceId,
            Q = string.IsNullOrWhiteSpace(freeText) ? null : freeText,
            IncludeTotal = true
        }.WithPage(page, pageSize);

        var result = await _client.DatastoreSearchAsync<VehicleRecord>(query, ct);

        return new PagedResult<VehicleRecord>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = result.Total,
            Items = result.Records
        };
    }

    private async Task<VehicleRecord?> FindInResourceAsync(string resourceId, string registrationNumber, CancellationToken ct)
    {
        var query = new DatastoreSearchQuery
        {
            ResourceId = resourceId,
            Filters = new Dictionary<string, object>
            {
                ["mispar_rechev"] = registrationNumber
            },
            Limit = 1
        };

        var result = await _client.DatastoreSearchAsync<VehicleRecord>(query, ct);
        return result.Records.FirstOrDefault();
    }
}
