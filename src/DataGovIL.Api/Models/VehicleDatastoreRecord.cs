using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the "מספרי רישוי של כלי רכב פרטיים ומסחריים" datastore resources.
/// Used only to deserialize CKAN JSON, whose keys are the live datastore field ids.
/// Types from data.gov.il (int / numeric / text) are stored as <see cref="string"/> so
/// CKAN's inconsistent JSON typing still deserializes via <c>FlexibleStringConverter</c>.
/// Unmapped columns are captured in <see cref="ExtensionData"/>.
/// Map to <see cref="VehicleRecord"/> before returning from the API.
/// </summary>
public class VehicleDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("sug_degem")]
    public string? ModelType { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("degem_cd")]
    public string? ModelCode { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

    [JsonPropertyName("ramat_gimur")]
    public string? TrimLevel { get; set; }

    [JsonPropertyName("ramat_eivzur_betihuty")]
    public string? SafetyEquipmentLevel { get; set; }

    [JsonPropertyName("kvutzat_zihum")]
    public string? PollutionGroup { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ManufactureYear { get; set; }

    [JsonPropertyName("degem_manoa")]
    public string? EngineModel { get; set; }

    [JsonPropertyName("mivchan_acharon_dt")]
    public string? LastTestDate { get; set; }

    [JsonPropertyName("tokef_dt")]
    public string? TestValidUntil { get; set; }

    [JsonPropertyName("baalut")]
    public string? OwnershipType { get; set; }

    [JsonPropertyName("misgeret")]
    public string? ChassisNumber { get; set; }

    [JsonPropertyName("tzeva_cd")]
    public string? ColorCode { get; set; }

    [JsonPropertyName("tzeva_rechev")]
    public string? Color { get; set; }

    [JsonPropertyName("zmig_kidmi")]
    public string? FrontTire { get; set; }

    [JsonPropertyName("zmig_ahori")]
    public string? RearTire { get; set; }

    [JsonPropertyName("sug_delek_nm")]
    public string? FuelType { get; set; }

    [JsonPropertyName("horaat_rishum")]
    public string? RegistrationOrder { get; set; }

    [JsonPropertyName("moed_aliya_lakvish")]
    public string? RoadEntryDate { get; set; }

    [JsonPropertyName("kinuy_mishari")]
    public string? CommercialName { get; set; }

    /// <summary>Any column not mapped above is preserved here, keyed by the raw datastore field id.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}
