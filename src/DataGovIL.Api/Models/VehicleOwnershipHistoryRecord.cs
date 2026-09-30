using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// Public API shape for one ownership-history row.
/// <see cref="OwnershipYearMonth"/> is from <c>baalut_dt</c> (typically <c>yyyyMM</c>).
/// </summary>
public class VehicleOwnershipHistoryRecord
{
    public string? Id { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? OwnershipYearMonth { get; set; }
    public string? OwnershipType { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? ExtensionData { get; set; }

    public static VehicleOwnershipHistoryRecord FromDatastore(VehicleOwnershipHistoryDatastoreRecord row) => new()
    {
        Id = row.Id,
        RegistrationNumber = row.RegistrationNumber,
        OwnershipYearMonth = row.OwnershipYearMonth,
        OwnershipType = row.OwnershipType,
        ExtensionData = row.ExtensionData
    };
}
