namespace DataGovIL.Client;

/// <summary>
/// Configuration for <see cref="CkanApiClient"/>. Bind this to configuration section
/// "DataGovIL" (see appsettings.json in the Api project) or set it up manually.
/// </summary>
public class CkanClientOptions
{
    /// <summary>
    /// Base URL of the CKAN action API, e.g. "https://data.gov.il/api/3".
    /// No trailing slash.
    /// </summary>
    public string BaseUrl { get; set; } = "https://data.gov.il/api/3";

    /// <summary>
    /// Optional CKAN API key, sent as the "Authorization" header.
    /// Not required for read-only endpoints on data.gov.il, but supported
    /// in case you point this client at a private CKAN instance.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Request timeout for the underlying HttpClient.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
}
