using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from "מספרי רישוי של כלי הרכב הציבוריים הפעילים" (taxis, shared taxis, buses).
/// CKAN inbound only.
/// Map to <see cref="VehicleRecord"/> via <see cref="VehicleRecord.FromPublic"/>.
/// </summary>
public class VehiclePublicDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("sug_rechev_cd")]
    public string? VehicleTypeCode { get; set; }

    [JsonPropertyName("sug_rechev_nm")]
    public string? VehicleTypeName { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ManufactureYear { get; set; }

    [JsonPropertyName("mishkal_kolel")]
    public string? TotalWeight { get; set; }

    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("tzeva_cd")]
    public string? ColorCode { get; set; }

    [JsonPropertyName("tzeva_rechev")]
    public string? Color { get; set; }

    [JsonPropertyName("degem_cd")]
    public string? ModelCode { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

    [JsonPropertyName("kinuy_mishari")]
    public string? CommercialName { get; set; }

    [JsonPropertyName("sug_rechev_EU_cd")]
    public string? EuVehicleTypeCode { get; set; }

    [JsonPropertyName("sug_rechev_EU_nm")]
    public string? EuVehicleTypeName { get; set; }

    /// <summary><c>0</c> = not cancelled (לא מבוטל).</summary>
    [JsonPropertyName("bitul_cd")]
    public string? CancellationCode { get; set; }

    [JsonPropertyName("bitul_nm")]
    public string? CancellationName { get; set; }

    [JsonPropertyName("bitul_dt")]
    public string? CancellationDate { get; set; }

    [JsonPropertyName("tokef_dt")]
    public string? TestValidUntil { get; set; }

    [JsonPropertyName("mispar_mekomot")]
    public string? SeatCount { get; set; }

    [JsonPropertyName("mispar_mekomot_leyd_nahag")]
    public string? SeatsBesideDriver { get; set; }
}
