using DataGovIL.Client.Models;

namespace DataGovIL.Client;

/// <summary>
/// Generic client for the data.gov.il CKAN Action API. Covers the endpoints documented in the
/// provided OpenAPI spec: status, licenses, packages (datasets), organizations, resources and
/// the datastore (row-level data inside a resource).
/// </summary>
public interface ICkanApiClient
{
    Task<StatusInfo> GetStatusAsync(CancellationToken ct = default);

    Task<List<LicenseInfo>> GetLicenseListAsync(CancellationToken ct = default);

    /// <summary>Ids (== names) of every dataset on the portal.</summary>
    Task<List<string>> GetPackageListAsync(CancellationToken ct = default);

    Task<PackageSearchResult> SearchPackagesAsync(PackageSearchQuery query, CancellationToken ct = default);

    Task<PackageInfo> GetPackageAsync(string idOrName, CancellationToken ct = default);

    Task<OrganizationInfo> GetOrganizationAsync(string idOrName, CancellationToken ct = default);

    /// <summary>Short names of every organization on the portal.</summary>
    Task<List<string>> GetOrganizationListAsync(CancellationToken ct = default);

    Task<ResourceSearchResult> SearchResourcesAsync(ResourceSearchQuery query, CancellationToken ct = default);

    Task<ResourceInfo> GetResourceAsync(string resourceId, bool includeTracking = false, CancellationToken ct = default);

    /// <summary>
    /// Queries the row-level data of a resource (datastore_search). This is the endpoint used
    /// to actually read records such as vehicle registrations or model catalogs.
    /// </summary>
    /// <typeparam name="TRecord">
    /// Shape to deserialize each row into. Use a DTO decorated with [JsonExtensionData] (see the
    /// records in the Api project) so unexpected/renamed columns are never silently dropped.
    /// </typeparam>
    Task<DatastoreSearchResult<TRecord>> DatastoreSearchAsync<TRecord>(DatastoreSearchQuery query, CancellationToken ct = default);

    Task<object> GetDatasetSchemaAsync(string type = "dataset", CancellationToken ct = default);
}
