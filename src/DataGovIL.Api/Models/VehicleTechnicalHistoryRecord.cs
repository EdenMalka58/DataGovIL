using System.Globalization;
using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// Public API shape for technical vehicle history.
/// <c>*Indicator</c> fields are bool (datastore 0/1).
/// <see cref="FirstRegistrationDate"/> is from <c>rishum_rishon_dt</c>.
/// </summary>
public class VehicleTechnicalHistoryRecord
{
    public string? Id { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? LastTestOdometer { get; set; }
    public bool? StructureChangeIndicator { get; set; }
    /// <summary>LPG (גפ״מ) system installed/changed — from <c>gapam_ind</c>. Not related to accidents.</summary>
    public bool? LpgChangeIndicator { get; set; }
    public bool? ColorChangeIndicator { get; set; }
    public bool? TireChangeIndicator { get; set; }
    public string? FirstRegistrationDate { get; set; }
    public string? OriginalityName { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? ExtensionData { get; set; }

    public static VehicleTechnicalHistoryRecord FromDatastore(VehicleTechnicalHistoryDatastoreRecord row) => new()
    {
        Id = row.Id,
        RegistrationNumber = row.RegistrationNumber,
        EngineNumber = row.EngineNumber,
        LastTestOdometer = row.LastTestOdometer,
        StructureChangeIndicator = ToBool(row.StructureChangeIndicator),
        LpgChangeIndicator = ToBool(row.LpgChangeIndicator),
        ColorChangeIndicator = ToBool(row.ColorChangeIndicator),
        TireChangeIndicator = ToBool(row.TireChangeIndicator),
        FirstRegistrationDate = row.FirstRegistrationDate,
        OriginalityName = row.OriginalityName,
        ExtensionData = row.ExtensionData
    };

    private static bool? ToBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var trimmed = value.Trim();
        if (trimmed is "1" or "true" or "True" or "TRUE") return true;
        if (trimmed is "0" or "false" or "False" or "FALSE") return false;
        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
            return n != 0;
        return null;
    }
}
