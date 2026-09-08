using System.Text.Json.Serialization;

namespace DataGovIL.Client.Models;

/// <summary>
/// Every CKAN action API response is wrapped in this envelope:
/// { "help": "...", "success": true/false, "result": {...} } or
/// { "help": "...", "success": false, "error": {...} }.
/// </summary>
/// <typeparam name="TResult">Shape of the "result" payload for the specific action.</typeparam>
public class CkanEnvelope<TResult>
{
    [JsonPropertyName("help")]
    public string? Help { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("result")]
    public TResult? Result { get; set; }

    [JsonPropertyName("error")]
    public CkanError? Error { get; set; }
}

/// <summary>
/// Error payload returned by CKAN when success == false.
/// </summary>
public class CkanError
{
    [JsonPropertyName("__type")]
    public string? Type { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// CKAN sometimes returns per-field validation errors as extra
    /// properties on the error object (e.g. "resource_id": ["Not found"]).
    /// They land here since we can't know the field names up front.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? FieldErrors { get; set; }
}
