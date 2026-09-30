using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// One row from "כלי רכב שלא ביצעו ריקול (קריאת שירות)".
/// CKAN inbound only. Column ids are uppercase on this resource.
/// Map to <see cref="VehicleRecallRecord"/> via <see cref="VehicleRecallRecord.FromDatastore"/>.
/// </summary>
public class VehicleRecallDatastoreRecord
{
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [JsonPropertyName("MISPAR_RECHEV")]
    public string? RegistrationNumber { get; set; }

    [JsonPropertyName("RECALL_ID")]
    public string? RecallId { get; set; }

    [JsonPropertyName("SUG_RECALL")]
    public string? RecallType { get; set; }

    [JsonPropertyName("SUG_TAKALA")]
    public string? FaultType { get; set; }

    [JsonPropertyName("TEUR_TAKALA")]
    public string? FaultDescription { get; set; }

    [JsonPropertyName("TAARICH_PTICHA")]
    public string? OpenedDate { get; set; }
}

/// <summary>ריקול שלא בוצע</summary>
public class VehicleRecallRecord
{
    /// <summary>מזהה</summary>
    public string? Id { get; set; }

    /// <summary>מזהה ריקול</summary>
    public string? RecallId { get; set; }

    /// <summary>סוג ריקול</summary>
    public string? RecallType { get; set; }

    /// <summary>סוג תקלה</summary>
    public string? FaultType { get; set; }

    /// <summary>תיאור תקלה</summary>
    public string? FaultDescription { get; set; }

    /// <summary>תאריך פתיחה</summary>
    public string? OpenedDate { get; set; }

    public static VehicleRecallRecord FromDatastore(VehicleRecallDatastoreRecord row) => new()
    {
        Id = row.Id,
        RecallId = row.RecallId,
        RecallType = row.RecallType,
        FaultType = row.FaultType,
        FaultDescription = row.FaultDescription,
        OpenedDate = row.OpenedDate
    };
}
