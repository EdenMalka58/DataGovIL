using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from "רכבים זכאים להנחה לאחר התקנת מערכות בטיחות".
/// CKAN inbound only. The resource stores a registration number and an update timestamp.
/// Map to <see cref="VehicleRecord"/> via <see cref="VehicleRecord.FromSafetyDiscount"/>.
/// </summary>
public class VehicleSafetyDiscountDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("mispar_rechev")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("updated_dt")]
    public string? UpdatedDate { get; set; }
}
