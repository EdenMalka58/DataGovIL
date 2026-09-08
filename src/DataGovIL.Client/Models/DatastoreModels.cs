using System.Text.Json.Serialization;

namespace DataGovIL.Client.Models;

/// <summary>
/// Enum mirroring the "records_format" parameter of datastore_search.
/// </summary>
public enum DatastoreRecordsFormat
{
    Objects,
    Lists,
    Csv,
    Tsv
}

/// <summary>
/// Request parameters for POST /action/datastore_search.
/// Build one of these per call; it is serialized straight to JSON.
/// </summary>
public class DatastoreSearchQuery
{
    /// <summary>Id or alias of the resource to search against. Required.</summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// Exact-match filters, e.g. { "mispar_rechev": "1234567" }.
    /// Sent as a JSON object, which only works reliably via POST (this client always POSTs).
    /// </summary>
    public Dictionary<string, object>? Filters { get; set; }

    /// <summary>Free text query across all fields (or per-field if you need that, see FieldQuery).</summary>
    public string? Q { get; set; }

    /// <summary>Per-field full text query, alternative to Q. If set, takes precedence over Q.</summary>
    public Dictionary<string, string>? FieldQuery { get; set; }

    /// <summary>Return only distinct rows.</summary>
    public bool? Distinct { get; set; }

    /// <summary>Treat Q as plain text (CKAN default is true).</summary>
    public bool? Plain { get; set; }

    /// <summary>Maximum rows to return.</summary>
    public int? Limit { get; set; }

    /// <summary>Offset for pagination.</summary>
    public int? Offset { get; set; }

    /// <summary>Columns to return. Null/empty = all columns.</summary>
    public List<string>? Fields { get; set; }

    /// <summary>Comma separated sort spec, e.g. "shnat_yitzur desc".</summary>
    public string? Sort { get; set; }

    /// <summary>Whether to compute and return the total row count. Costs performance on large tables.</summary>
    public bool? IncludeTotal { get; set; }

    public DatastoreRecordsFormat? RecordsFormat { get; set; }

    /// <summary>
    /// Convenience helper: sets Limit/Offset from a 1-based page number and page size.
    /// </summary>
    public DatastoreSearchQuery WithPage(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        Limit = pageSize;
        Offset = (page - 1) * pageSize;
        return this;
    }

    internal object ToRequestBody()
    {
        var body = new Dictionary<string, object?>
        {
            ["resource_id"] = ResourceId
        };

        if (Filters is { Count: > 0 }) body["filters"] = Filters;
        if (FieldQuery is { Count: > 0 }) body["q"] = FieldQuery;
        else if (!string.IsNullOrWhiteSpace(Q)) body["q"] = Q;
        if (Distinct.HasValue) body["distinct"] = Distinct.Value;
        if (Plain.HasValue) body["plain"] = Plain.Value;
        if (Limit.HasValue) body["limit"] = Limit.Value;
        if (Offset.HasValue) body["offset"] = Offset.Value;
        if (Fields is { Count: > 0 }) body["fields"] = Fields;
        if (!string.IsNullOrWhiteSpace(Sort)) body["sort"] = Sort;
        if (IncludeTotal.HasValue) body["include_total"] = IncludeTotal.Value;
        if (RecordsFormat.HasValue) body["records_format"] = RecordsFormat.Value.ToString().ToLowerInvariant();

        return body;
    }
}

/// <summary>Describes one column as returned in the "fields" array of a datastore_search result.</summary>
public class DatastoreField
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Result payload of datastore_search, generic over the record shape so callers can deserialize
/// rows into a strongly typed DTO or into a loose Dictionary&lt;string, JsonElement&gt;.
/// </summary>
/// <typeparam name="TRecord">Row type. Use a POCO with [JsonExtensionData] to be resilient to
/// unknown/renamed columns, or Dictionary&lt;string, object&gt; if you want maximum flexibility.</typeparam>
public class DatastoreSearchResult<TRecord>
{
    [JsonPropertyName("resource_id")]
    public string ResourceId { get; set; } = string.Empty;

    [JsonPropertyName("fields")]
    public List<DatastoreField> Fields { get; set; } = new();

    [JsonPropertyName("records")]
    public List<TRecord> Records { get; set; } = new();

    [JsonPropertyName("total")]
    public int? Total { get; set; }

    [JsonPropertyName("include_total")]
    public bool? IncludeTotal { get; set; }

    [JsonPropertyName("_links")]
    public Dictionary<string, string>? Links { get; set; }
}
