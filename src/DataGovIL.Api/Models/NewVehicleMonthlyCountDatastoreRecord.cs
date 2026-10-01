using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from "כמות סגירות חודשי עבור רכבים חדשים בעלי קוד דגם".
/// Used only to deserialize CKAN JSON. Numeric columns stay strings so
/// <c>FlexibleStringConverter</c> accepts either a JSON number or a string.
/// </summary>
public class NewVehicleMonthlyCountDatastoreRecord
{
    /// <summary>חודש ושנת סגירה, <c>yyyyMM</c>.</summary>
    [JsonPropertyName("sgira_month")]
    public string? ClosureMonth { get; set; }

    /// <summary>כמות רכבים חדשים.</summary>
    [JsonPropertyName("car_num")]
    public string? VehicleCount { get; set; }
}
