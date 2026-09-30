using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from "כלי רכב ביבוא אישי".
/// CKAN inbound only.
/// Map to <see cref="VehicleRecord"/> via <see cref="VehicleRecord.FromPersonalImport"/>.
/// </summary>
public class VehiclePersonalImportDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    /// <summary>Chassis number (<c>shilda</c>).</summary>
    [JsonPropertyName("shilda")]
    public string? ChassisNumber { get; set; }

    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("sug_rechev_cd")]
    public string? VehicleTypeCode { get; set; }

    [JsonPropertyName("sug_rechev_nm")]
    public string? VehicleTypeName { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

    [JsonPropertyName("mishkal_kolel")]
    public string? TotalWeight { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ManufactureYear { get; set; }

    [JsonPropertyName("nefach_manoa")]
    public string? EngineDisplacement { get; set; }

    [JsonPropertyName("tozeret_eretz_nm")]
    public string? ManufacturerCountryName { get; set; }

    [JsonPropertyName("degem_manoa")]
    public string? EngineModel { get; set; }

    [JsonPropertyName("mivchan_acharon_dt")]
    public string? LastTestDate { get; set; }

    [JsonPropertyName("tokef_dt")]
    public string? TestValidUntil { get; set; }

    /// <summary>Import type (<c>sug_yevu</c>).</summary>
    [JsonPropertyName("sug_yevu")]
    public string? ImportType { get; set; }

    [JsonPropertyName("moed_aliya_lakvish")]
    public string? RoadEntryDate { get; set; }

    [JsonPropertyName("sug_delek_nm")]
    public string? FuelType { get; set; }
}
