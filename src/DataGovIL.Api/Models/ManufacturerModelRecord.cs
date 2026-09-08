using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from the "תוצרים ודגמים של כלי רכב WLTP" (vehicle makes/models, WLTP) datastore
/// resource. Same caveat as <see cref="VehicleRecord"/>: verify real column ids against
/// datastore_search's "fields" metadata before depending on this in production; unmapped
/// columns still arrive via <see cref="ExtensionData"/>.
/// </summary>
public class ManufacturerModelRecord
{
    [JsonPropertyName("tozeret_cd")]
    public string? ManufacturerCode { get; set; }

    [JsonPropertyName("tozeret_nm")]
    public string? ManufacturerName { get; set; }

    [JsonPropertyName("tozar")]
    public string? Tozar { get; set; }

    [JsonPropertyName("degem_cd")]
    public string? ModelCode { get; set; }

    [JsonPropertyName("degem_nm")]
    public string? ModelName { get; set; }

    [JsonPropertyName("kinuy_mishari")]
    public string? CommercialName { get; set; }

    [JsonPropertyName("shnat_yitzur")]
    public string? ModelYear { get; set; }

    [JsonPropertyName("ramat_gimur")]
    public string? TrimLevel { get; set; }

    [JsonPropertyName("kvutzat_zihum")]
    public string? PollutionGroup { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object>? ExtensionData { get; set; }
}

/// <summary>Distinct (manufacturer, model) pair projected out of the raw WLTP rows for the list endpoint.</summary>
public class ManufacturerModelSummary
{
    public string? ManufacturerName { get; set; }
    public string? ModelName { get; set; }
    public string? CommercialName { get; set; }
}
