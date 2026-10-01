using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from "כלי רכב מעל 3 וחצי טון וכלי רכב חסרי קוד דגם"
/// (supported trailers, tractors, work vehicles, trucks, buses).
/// CKAN inbound only.
/// Map to <see cref="VehicleRecord"/> via <see cref="VehicleRecord.FromHeavy"/>.
/// </summary>
public class VehicleHeavyDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

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

    [JsonPropertyName("mishkal_azmi")]
    public string? CurbWeight { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

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

    [JsonPropertyName("moed_aliya_lakvish")]
    public string? RoadEntryDate { get; set; }

    [JsonPropertyName("horaat_rishum")]
    public string? RegistrationOrder { get; set; }

    [JsonPropertyName("mispar_mekomot_leyd_nahag")]
    public string? SeatsBesideDriver { get; set; }

    [JsonPropertyName("mispar_mekomot")]
    public string? SeatCount { get; set; }

    /// <summary>Vehicle type group: טרקטור, פרטי, מסחרי, גרור נתמך.</summary>
    [JsonPropertyName("kvutzat_sug_rechev")]
    public string? VehicleTypeGroup { get; set; }

    /// <summary>Tow hitch: יש / אין / קבוע וו גרירה.</summary>
    [JsonPropertyName("grira_nm")]
    public string? TowHitch { get; set; }

    [JsonPropertyName("zmig_kidmi")]
    public string? FrontTire { get; set; }

    [JsonPropertyName("zmig_ahori")]
    public string? RearTire { get; set; }

    [JsonPropertyName("mispar_manoa")]
    public string? EngineNumber { get; set; }

    [JsonPropertyName("sranim")]
    public string? Axles { get; set; }
}
