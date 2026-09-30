using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the vehicle ownership-history datastore
/// (<c>bb2355dc-9ec7-4f06-9c3f-3344672171da</c>). CKAN inbound only.
/// </summary>
public class VehicleOwnershipHistoryDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("baalut_dt")]
    public string? OwnershipYearMonth { get; set; }

    [JsonPropertyName("baalut")]
    public string? OwnershipType { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}
