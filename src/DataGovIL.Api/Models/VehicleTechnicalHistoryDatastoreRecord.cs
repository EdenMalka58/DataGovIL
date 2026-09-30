using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the vehicle technical/history datastore
/// (<c>56063a99-8a3e-4ff4-912e-5966c0279bad</c>). CKAN inbound only.
/// </summary>
public class VehicleTechnicalHistoryDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("mispar_manoa")]
    public string? EngineNumber { get; set; }

    [JsonPropertyName("kilometer_test_aharon")]
    public string? LastTestOdometer { get; set; }

    [JsonPropertyName("shinui_mivne_ind")]
    public string? StructureChangeIndicator { get; set; }

    /// <summary>LPG (גפ״מ) system installed/changed indicator. Not related to accidents.</summary>
    [JsonPropertyName("gapam_ind")]
    public string? LpgChangeIndicator { get; set; }

    [JsonPropertyName("shnui_zeva_ind")]
    public string? ColorChangeIndicator { get; set; }

    [JsonPropertyName("shinui_zmig_ind")]
    public string? TireChangeIndicator { get; set; }

    [JsonPropertyName("rishum_rishon_dt")]
    public string? FirstRegistrationDate { get; set; }

    [JsonPropertyName("mkoriut_nm")]
    public string? OriginalityName { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}
