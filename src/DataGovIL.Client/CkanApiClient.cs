using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DataGovIL.Client.Json;
using DataGovIL.Client.Models;
using Microsoft.Extensions.Options;

namespace DataGovIL.Client;

/// <summary>
/// Default implementation of <see cref="ICkanApiClient"/>. Register via
/// <c>services.AddDataGovIlClient(...)</c> (see ServiceCollectionExtensions) so the
/// HttpClient lifetime is managed correctly by IHttpClientFactory.
/// </summary>
public class CkanApiClient : ICkanApiClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new FlexibleStringConverter() }
    };

    public CkanApiClient(HttpClient httpClient, IOptions<CkanClientOptions> options)
    {
        _http = httpClient;
        var opts = options.Value;

        if (_http.BaseAddress is null)
        {
            _http.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
        }
        _http.Timeout = opts.Timeout;

        if (!string.IsNullOrWhiteSpace(opts.ApiKey))
        {
            _http.DefaultRequestHeaders.Remove("Authorization");
            _http.DefaultRequestHeaders.Add("Authorization", opts.ApiKey);
        }

        _jsonOptions = DefaultJsonOptions;
    }

    public Task<StatusInfo> GetStatusAsync(CancellationToken ct = default)
        => GetAsync<StatusInfo>("action/status_show", ct);

    public async Task<List<LicenseInfo>> GetLicenseListAsync(CancellationToken ct = default)
        => await GetAsync<List<LicenseInfo>>("action/license_list", ct) ?? new List<LicenseInfo>();

    public async Task<List<string>> GetPackageListAsync(CancellationToken ct = default)
        => await GetAsync<List<string>>("action/package_list", ct) ?? new List<string>();

    public Task<PackageSearchResult> SearchPackagesAsync(PackageSearchQuery query, CancellationToken ct = default)
    {
        var qs = query.ToQueryString().ToString();
        var path = string.IsNullOrEmpty(qs) ? "action/package_search" : $"action/package_search?{qs}";
        return GetAsync<PackageSearchResult>(path, ct)!;
    }

    public Task<PackageInfo> GetPackageAsync(string idOrName, CancellationToken ct = default)
    {
        var path = $"action/package_show?id={Uri.EscapeDataString(idOrName)}";
        return GetAsync<PackageInfo>(path, ct)!;
    }

    public Task<OrganizationInfo> GetOrganizationAsync(string idOrName, CancellationToken ct = default)
    {
        var path = $"action/organization_show?id={Uri.EscapeDataString(idOrName)}";
        return GetAsync<OrganizationInfo>(path, ct)!;
    }

    public async Task<List<string>> GetOrganizationListAsync(CancellationToken ct = default)
        => await GetAsync<List<string>>("action/organization_list", ct) ?? new List<string>();

    public Task<ResourceSearchResult> SearchResourcesAsync(ResourceSearchQuery query, CancellationToken ct = default)
    {
        var qsb = new DatastoreSearchQueryStringBuilder();
        qsb.Add("query", query.Query);
        if (!string.IsNullOrWhiteSpace(query.OrderBy)) qsb.Add("order_by", query.OrderBy);
        if (query.Offset > 0) qsb.Add("offset", query.Offset.ToString());
        if (query.Limit > 0) qsb.Add("limit", query.Limit.ToString());
        return GetAsync<ResourceSearchResult>($"action/resource_search?{qsb}", ct)!;
    }

    public Task<ResourceInfo> GetResourceAsync(string resourceId, bool includeTracking = false, CancellationToken ct = default)
    {
        var path = $"action/resource_show?id={Uri.EscapeDataString(resourceId)}&include_tracking={includeTracking.ToString().ToLowerInvariant()}";
        return GetAsync<ResourceInfo>(path, ct)!;
    }

    public Task<DatastoreSearchResult<TRecord>> DatastoreSearchAsync<TRecord>(DatastoreSearchQuery query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query.ResourceId))
            throw new ArgumentException("ResourceId is required.", nameof(query));

        return PostAsync<DatastoreSearchResult<TRecord>>("action/datastore_search", query.ToRequestBody(), ct)!;
    }

    public async Task<object> GetDatasetSchemaAsync(string type = "dataset", CancellationToken ct = default)
    {
        var path = $"action/scheming_dataset_schema_show?type={Uri.EscapeDataString(type)}";
        var result = await GetAsync<JsonElement>(path, ct);
        return result;
    }

    // ---- internal helpers -------------------------------------------------

    private async Task<TResult?> GetAsync<TResult>(string relativePath, CancellationToken ct)
    {
        using var response = await _http.GetAsync(relativePath, ct).ConfigureAwait(false);
        return await UnwrapAsync<TResult>(response, ct).ConfigureAwait(false);
    }

    private async Task<TResult?> PostAsync<TResult>(string relativePath, object body, CancellationToken ct)
    {
        using var response = await _http.PostAsJsonAsync(relativePath, body, _jsonOptions, ct).ConfigureAwait(false);
        return await UnwrapAsync<TResult>(response, ct).ConfigureAwait(false);
    }

    private async Task<TResult?> UnwrapAsync<TResult>(HttpResponseMessage response, CancellationToken ct)
    {
        CkanEnvelope<TResult>? envelope;
        try
        {
            envelope = await response.Content.ReadFromJsonAsync<CkanEnvelope<TResult>>(_jsonOptions, ct)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            var raw = await SafeReadAsStringAsync(response, ct).ConfigureAwait(false);
            // Note: this is usually NOT malformed JSON — CKAN's envelope is almost always well
            // formed. The far more common cause is a shape mismatch, e.g. a datastore column
            // coming back as a JSON number/bool where the target TResult expects a string.
            // ex.Message from System.Text.Json normally names the offending path/token.
            throw new CkanApiException(
                $"CKAN response for '{response.RequestMessage?.RequestUri}' could not be deserialized into " +
                $"'{typeof(TResult).Name}': {ex.Message} Raw body: {Truncate(raw)}",
                response.StatusCode,
                inner: ex);
        }

        if (envelope is null)
        {
            throw new CkanApiException(
                $"CKAN response for '{response.RequestMessage?.RequestUri}' was empty.",
                response.StatusCode);
        }

        if (!response.IsSuccessStatusCode || !envelope.Success)
        {
            var message = envelope.Error?.Message
                ?? $"CKAN request to '{response.RequestMessage?.RequestUri}' failed with status {(int)response.StatusCode}.";
            throw new CkanApiException(message, response.StatusCode, envelope.Error);
        }

        return envelope.Result;
    }

    private static async Task<string> SafeReadAsStringAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try { return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false); }
        catch { return "<unreadable>"; }
    }

    private static string Truncate(string s, int max = 500) => s.Length <= max ? s : s[..max] + "...";
}
