using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from "מספרי רישוי של כלי רכב דו גלגליים" (motorcycles and scooters).
/// CKAN inbound only.
/// Map to <see cref="VehicleRecord"/> via <see cref="VehicleRecord.FromTwoWheeled"/>.
/// </summary>
public class VehicleTwoWheeledDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("tozeret_eretz_nm")]
    public string? ManufacturerCountryName { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ManufactureYear { get; set; }

    [JsonPropertyName("sug_delek_cd")]
    public string? FuelCode { get; set; }

    [JsonPropertyName("sug_delek_nm")]
    public string? FuelType { get; set; }

    [JsonPropertyName("mishkal_kolel")]
    public string? TotalWeight { get; set; }

    [JsonPropertyName("mida_zmig_kidmi")]
    public string? FrontTireSize { get; set; }

    [JsonPropertyName("mida_zmig_ahori")]
    public string? RearTireSize { get; set; }

    [JsonPropertyName("kod_omes_zmig_kidmi")]
    public string? FrontTireLoadIndex { get; set; }

    [JsonPropertyName("kod_omes_zmig_ahori")]
    public string? RearTireLoadIndex { get; set; }

    [JsonPropertyName("kod_mehirut_zmig_kidmi")]
    public string? FrontTireSpeedRating { get; set; }

    [JsonPropertyName("kod_mehirut_zmig_ahori")]
    public string? RearTireSpeedRating { get; set; }

    [JsonPropertyName("nefach_manoa")]
    public string? EngineDisplacement { get; set; }

    /// <summary>Engine power in kW.</summary>
    [JsonPropertyName("hespek")]
    public string? EnginePowerKw { get; set; }

    /// <summary>Frame number (<c>misgeret</c>), the motorcycle VIN.</summary>
    [JsonPropertyName("misgeret")]
    public string? ChassisNumber { get; set; }

    [JsonPropertyName("moed_aliya_lakvish")]
    public string? RoadEntryDate { get; set; }

    [JsonPropertyName("sug_rechev_EU_cd")]
    public string? EuVehicleTypeCode { get; set; }

    [JsonPropertyName("sug_rechev_cd")]
    public string? VehicleTypeCode { get; set; }

    [JsonPropertyName("sug_rechev_nm")]
    public string? VehicleTypeName { get; set; }

    [JsonPropertyName("mispar_manoa")]
    public string? EngineNumber { get; set; }

    [JsonPropertyName("horaat_rishum")]
    public string? RegistrationOrder { get; set; }

    [JsonPropertyName("baalut")]
    public string? OwnershipType { get; set; }

    [JsonPropertyName("mkoriut_nm")]
    public string? Originality { get; set; }

    [JsonPropertyName("mispar_mekomot_leyd_nahag")]
    public string? SeatsBesideDriver { get; set; }

    [JsonPropertyName("mispar_mekomot")]
    public string? SeatCount { get; set; }
}
