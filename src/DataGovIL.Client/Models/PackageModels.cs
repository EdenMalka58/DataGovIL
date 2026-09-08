using System.Text.Json.Serialization;

namespace DataGovIL.Client.Models;

/// <summary>Request parameters for GET /action/package_search.</summary>
public class PackageSearchQuery
{
    /// <summary>Solr query string, e.g. "vehicles". Empty/null = match all.</summary>
    public string? Q { get; set; }

    /// <summary>Solr filter query, e.g. "organization:moit".</summary>
    public string? Fq { get; set; }

    public string? Sort { get; set; }

    /// <summary>Max rows, clamped server-side to 1000.</summary>
    public int Rows { get; set; } = 20;

    public int Start { get; set; }

    public bool? IncludePrivate { get; set; }

    public DatastoreSearchQueryStringBuilder ToQueryString()
    {
        var qs = new DatastoreSearchQueryStringBuilder();
        if (!string.IsNullOrWhiteSpace(Q)) qs.Add("q", Q);
        if (!string.IsNullOrWhiteSpace(Fq)) qs.Add("fq", Fq);
        if (!string.IsNullOrWhiteSpace(Sort)) qs.Add("sort", Sort);
        qs.Add("rows", Rows.ToString());
        qs.Add("start", Start.ToString());
        if (IncludePrivate.HasValue) qs.Add("include_private", IncludePrivate.Value.ToString().ToLowerInvariant());
        return qs;
    }
}

/// <summary>Tiny helper to build a query string without pulling in extra dependencies.</summary>
public class DatastoreSearchQueryStringBuilder
{
    private readonly List<string> _parts = new();

    public void Add(string key, string? value)
    {
        if (value is null) return;
        _parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
    }

    public override string ToString() => string.Join("&", _parts);
}

public class PackageSearchResult
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("sort")]
    public string? Sort { get; set; }

    [JsonPropertyName("results")]
    public List<PackageInfo> Results { get; set; } = new();
}

/// <summary>
/// A dataset ("package") as returned by package_show / package_search.
/// Only the commonly used fields are modeled explicitly; anything else CKAN
/// includes (extras, custom schema fields, etc.) is preserved in <see cref="ExtensionData"/>.
/// </summary>
public class PackageInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("author_email")]
    public string? AuthorEmail { get; set; }

    [JsonPropertyName("maintainer")]
    public string? Maintainer { get; set; }

    [JsonPropertyName("license_id")]
    public string? LicenseId { get; set; }

    [JsonPropertyName("license_title")]
    public string? LicenseTitle { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("private")]
    public bool Private { get; set; }

    [JsonPropertyName("metadata_created")]
    public DateTimeOffset? MetadataCreated { get; set; }

    [JsonPropertyName("metadata_modified")]
    public DateTimeOffset? MetadataModified { get; set; }

    [JsonPropertyName("organization")]
    public OrganizationInfo? Organization { get; set; }

    [JsonPropertyName("resources")]
    public List<ResourceInfo> Resources { get; set; } = new();

    [JsonPropertyName("tags")]
    public List<TagInfo> Tags { get; set; } = new();

    [JsonPropertyName("extras")]
    public List<ExtraInfo> Extras { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}

public class TagInfo
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class ExtraInfo
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}

/// <summary>A distribution/resource attached to a dataset (e.g. the CSV loaded into the datastore).</summary>
public class ResourceInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("package_id")]
    public string? PackageId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("mimetype")]
    public string? MimeType { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("resource_type")]
    public string? ResourceType { get; set; }

    [JsonPropertyName("datastore_active")]
    public bool? DatastoreActive { get; set; }

    [JsonPropertyName("created")]
    public DateTimeOffset? Created { get; set; }

    [JsonPropertyName("last_modified")]
    public DateTimeOffset? LastModified { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}

public class OrganizationInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("package_count")]
    public int? PackageCount { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}

/// <summary>Request parameters for GET /action/resource_search.</summary>
public class ResourceSearchQuery
{
    /// <summary>Criteria of the form "{field}:{term}", e.g. "format:csv".</summary>
    public string Query { get; set; } = "format:csv";

    public string? OrderBy { get; set; }

    public int Offset { get; set; }

    public int Limit { get; set; }
}

public class ResourceSearchResult
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("results")]
    public List<ResourceInfo> Results { get; set; } = new();
}

public class LicenseInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

public class StatusInfo
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("site_title")]
    public string? SiteTitle { get; set; }

    [JsonPropertyName("site_url")]
    public string? SiteUrl { get; set; }

    [JsonPropertyName("extensions")]
    public List<string> Extensions { get; set; } = new();

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}
