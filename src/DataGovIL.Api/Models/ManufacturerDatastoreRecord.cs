using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the manufacturers datastore resource (tozeret_cd / tozeret_nm / tozar / tozeret_eretz_nm).
/// Used only to deserialize CKAN JSON.
/// </summary>
public class ManufacturerDatastoreRecord
{
    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("tozar")]
    public string? Tozar { get; set; }

    [JsonPropertyName("tozeret_eretz_nm")]
    public string? ManufacturerCountryName { get; set; }
}
