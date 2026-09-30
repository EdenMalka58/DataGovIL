using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// Vehicle value adjustment (ירידת ערך) from kilometers, owner count, and ownership type.
/// Percents are signed: negative lowers the value, positive raises it.
/// </summary>
public class VehicleDepreciationRecord
{
    /// <summary>סה"כ אחוז שינוי ערך</summary>
    public decimal DepreciationPercent { get; set; }

    /// <summary>סה"כ שינוי ערך בש"ח. Null when no price-list price was found.</summary>
    public decimal? DepreciationValue { get; set; }

    /// <summary>מחיר מחירון</summary>
    public decimal? ListPrice { get; set; }

    /// <summary>ערך משוערך (מחירון + שינוי ערך)</summary>
    public decimal? EstimatedValue { get; set; }

    /// <summary>גיל הרכב בשנים, from the road entry date to today.</summary>
    public double CarAge { get; set; }

    /// <summary>קילומטראז' בטסט האחרון</summary>
    public long? Kilometers { get; set; }

    /// <summary>מספר בעלים (count of ownership-history rows)</summary>
    public int OwnerCount { get; set; }

    /// <summary>מקוריות (<c>mkoriut_nm</c>)</summary>
    public string? Originality { get; set; }

    /// <summary>סוג הבעלות שנבחר לחישוב (the highest-loss type across owners and originality)</summary>
    public string? OwnershipType { get; set; }

    public DepreciationVehicleCategory VehicleCategory { get; set; }

    public DepreciationOwnerCategory OwnerCategory { get; set; }

    public List<VehicleDepreciationLine> Lines { get; set; } = new();
}

public class VehicleDepreciationLine
{
    public DepreciationFactor Factor { get; set; }

    public string Description { get; set; } = string.Empty;

    public decimal Percent { get; set; }

    /// <summary>Null when no price-list price was found.</summary>
    public decimal? Value { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DepreciationFactor
{
    Kilometers,
    OwnerCount,
    OwnershipType
}

/// <summary>Matches the Delphi <c>ct*</c> constants; the value is the row index in the loss table.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DepreciationVehicleCategory
{
    Private = 0,
    Commercial4t = 1,
    Commercial = 2,
    Taxi = 3,
    Minibus = 4
}

/// <summary>Matches the Delphi <c>ot*</c> constants; the value is the column index in the loss table.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DepreciationOwnerCategory
{
    Private = 0,
    Commercial = 1,
    Public = 2,
    Rental = 3,
    Kibbutz = 4,
    Government = 5,
    Taxi = 6,
    DriveTech = 7
}
