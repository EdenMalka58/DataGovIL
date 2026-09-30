using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the "כלי רכב שירדו מהכביש ובסטטוס ביטול סופי" datastore resources
/// (current + historical 2010–2016 / 2000–2009). CKAN inbound only; column types differ
/// across resources (numeric vs text) but are all read as <see cref="string"/>.
/// Map to <see cref="VehicleRecord"/> via <see cref="VehicleRecord.FromCancelled"/>.
/// </summary>
public class VehicleCancelledDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("degem_cd")]
    public string? ModelCode { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

    [JsonPropertyName("sug_rechev_cd")]
    public string? VehicleTypeCode { get; set; }

    [JsonPropertyName("sug_rechev_nm")]
    public string? VehicleTypeName { get; set; }

    [JsonPropertyName("moed_aliya_lakvish")]
    public string? RoadEntryDate { get; set; }

    [JsonPropertyName("bitul_dt")]
    public string? CancellationDate { get; set; }

    [JsonPropertyName("misgeret")]
    public string? ChassisNumber { get; set; }

    [JsonPropertyName("tozar_manoa")]
    public string? EngineManufacturer { get; set; }

    [JsonPropertyName("degem_manoa")]
    public string? EngineModel { get; set; }

    [JsonPropertyName("mispar_manoa")]
    public string? EngineNumber { get; set; }

    [JsonPropertyName("mishkal_kolel")]
    public string? TotalWeight { get; set; }

    [JsonPropertyName("ramat_gimur")]
    public string? TrimLevel { get; set; }

    [JsonPropertyName("ramat_eivzur_betihuty")]
    public string? SafetyEquipmentLevel { get; set; }

    [JsonPropertyName("kvutzat_zihum")]
    public string? PollutionGroup { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ManufactureYear { get; set; }

    [JsonPropertyName("baalut")]
    public string? OwnershipType { get; set; }

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

    [JsonPropertyName("kinuy_mishari")]
    public string? CommercialName { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}
