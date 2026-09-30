using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// Public API shape for a new-vehicle price-list row (יבואנים ומחירוני רכב חדש).
/// Serialized with ASP.NET camelCase names.
/// </summary>
public class VehiclePriceListRecord
{
    public string? Id { get; set; }
    public string? ImporterCode { get; set; }
    public string? ImporterName { get; set; }
    public string? ModelType { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? ManufacturerName { get; set; }
    public string? ModelCode { get; set; }
    public string? ModelName { get; set; }
    public string? ManufactureYear { get; set; }
    public string? Price { get; set; }
    public string? CommercialName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? ExtensionData { get; set; }

    public static VehiclePriceListRecord FromDatastore(VehiclePriceListDatastoreRecord row) => new()
    {
        Id = row.Id,
        ImporterCode = row.ImporterCode,
        ImporterName = row.ImporterName,
        ModelType = row.ModelType,
        ManufacturerCode = row.ManufacturerCode,
        ManufacturerName = row.ManufacturerName,
        ModelCode = row.ModelCode,
        ModelName = row.ModelName,
        ManufactureYear = row.ManufactureYear,
        Price = row.Price,
        CommercialName = row.CommercialName,
        ExtensionData = row.ExtensionData
    };
}
