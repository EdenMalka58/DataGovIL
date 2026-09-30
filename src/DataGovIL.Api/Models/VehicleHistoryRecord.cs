namespace DataGovIL.Api.Models;

/// <summary>
/// Combined vehicle history from the technical and ownership datastore resources,
/// keyed by registration number (<c>mispar_rechev</c>).
/// </summary>
public class VehicleHistoryRecord
{
    public string RegistrationNumber { get; set; } = string.Empty;

    /// <summary>Technical / structural history row, when present.</summary>
    public VehicleTechnicalHistoryRecord? Technical { get; set; }

    /// <summary>Ownership timeline rows (typically multiple), newest first when sortable.</summary>
    public List<VehicleOwnershipHistoryRecord> OwnershipHistory { get; set; } = new();
}
