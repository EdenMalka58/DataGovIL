using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from "מספרי רישוי של כלי רכב לא פעילים וחסרי קוד דגם".
/// CKAN inbound only.
/// Map to <see cref="VehicleRecord"/> via <see cref="VehicleRecord.FromInactiveWithoutModelCode"/>.
/// </summary>
public class VehicleInactiveWithoutModelCodeDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    /// <summary>Chassis number (<c>mispar_shilda</c>).</summary>
    [JsonPropertyName("mispar_shilda")]
    public string? ChassisNumber { get; set; }

    [JsonPropertyName("tkina_EU")]
    public string? EuTypeApproval { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ManufactureYear { get; set; }

    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("tozeret_eretz_nm")]
    public string? ManufacturerCountryName { get; set; }

    [JsonPropertyName("sug_delek_cd")]
    public string? FuelCode { get; set; }

    [JsonPropertyName("sug_delek_nm")]
    public string? FuelType { get; set; }

    [JsonPropertyName("mishkal_kolel")]
    public string? TotalWeight { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

    [JsonPropertyName("mishkal_azmi")]
    public string? CurbWeight { get; set; }

    [JsonPropertyName("nefach_manoa")]
    public string? EngineDisplacement { get; set; }

    [JsonPropertyName("degem_manoa")]
    public string? EngineModel { get; set; }

    [JsonPropertyName("hanaa_cd")]
    public string? DriveCode { get; set; }

    [JsonPropertyName("hanaa_nm")]
    public string? DriveName { get; set; }

    [JsonPropertyName("mishkal_mitan_harama")]
    public string? LiftingLoadWeight { get; set; }

    [JsonPropertyName("horaat_rishum")]
    public string? RegistrationOrder { get; set; }
}
