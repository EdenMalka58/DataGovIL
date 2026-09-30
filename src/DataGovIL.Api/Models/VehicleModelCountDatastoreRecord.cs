using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the "מאגר כמויות כלי רכב לפי תוצר, דגם ושנת יצור" datastore resource.
/// Used only to deserialize CKAN JSON; only the join key and the two count columns are mapped.
/// </summary>
public class VehicleModelCountDatastoreRecord
{
    [JsonPropertyName("sug_degem")]
    public string? ModelType { get; set; }

    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("degem_cd")]
    public string? ModelCode { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ModelYear { get; set; }

    [JsonPropertyName("mispar_rechavim_pailim")]
    public string? ActiveVehicleCount { get; set; }

    [JsonPropertyName("mispar_rechavim_le_pailim")]
    public string? InactiveVehicleCount { get; set; }
}
