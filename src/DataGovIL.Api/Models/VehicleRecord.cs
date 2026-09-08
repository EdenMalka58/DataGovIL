using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the "מספרי רישוי של כלי רכב פרטיים ומסחריים" (private/commercial vehicle
/// registration) datastore resources.
///
/// IMPORTANT: data.gov.il does not publish a fixed schema contract for CSV-backed datastore
/// resources, and this sandbox could not reach data.gov.il to confirm the live column names
/// (robots.txt blocks automated fetches). The property names below are the commonly used ones
/// for this dataset, but before relying on this in production, confirm the real column ids by
/// calling GET {baseUrl}/action/datastore_search?resource_id=...&amp;limit=0 once and reading the
/// returned "fields" array, then adjust the [JsonPropertyName] attributes if needed.
/// Nothing is lost either way: unmapped columns are captured in <see cref="ExtensionData"/>.
/// </summary>
public class VehicleRecord
{
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

    [JsonPropertyName("kinuy_mishari")]
    public string? CommercialName { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ManufactureYear { get; set; }

    [JsonPropertyName("ramat_gimur")]
    public string? TrimLevel { get; set; }

    [JsonPropertyName("tzeva_rechev")]
    public string? Color { get; set; }

    [JsonPropertyName("sug_delek_nm")]
    public string? FuelType { get; set; }

    [JsonPropertyName("baalut")]
    public string? OwnershipType { get; set; }

    [JsonPropertyName("misgeret")]
    public string? ChassisNumber { get; set; }

    [JsonPropertyName("moed_aliya_lakvish")]
    public string? RoadEntryDate { get; set; }

    [JsonPropertyName("tokef_dt")]
    public string? TestValidUntil { get; set; }

    /// <summary>Any column not mapped above is preserved here, keyed by the raw datastore field id.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}
