namespace DataGovIL.Api.Models;

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }

    /// <summary>Total number of matching rows, when known (datastore include_total).</summary>
    public int? TotalCount { get; set; }

    public List<T> Items { get; set; } = new();
}

/// <summary>Resource ids for the specific data.gov.il datasets this Api project exposes.</summary>
public class VehicleDataResourceOptions
{
    public string PrivateAndCommercialVehiclesResourceId { get; set; } = string.Empty;

    /// <summary>The dataset is split across two resources on the portal; this is the second part.</summary>
    public string? PrivateAndCommercialVehiclesContinuationResourceId { get; set; }

    /// <summary>Inactive registrations that still have a model code (לא פעילים עם קוד דגם).</summary>
    public string InactiveVehiclesWithModelCodeResourceId { get; set; } = string.Empty;

    /// <summary>Inactive registrations with no model code (לא פעילים וחסרי קוד דגם).</summary>
    public string InactiveVehiclesWithoutModelCodeResourceId { get; set; } = string.Empty;

    /// <summary>Vehicles eligible for a discount after safety-system installation (רכבים זכאים להנחה לאחר התקנת מערכות בטיחות).</summary>
    public string SafetyDiscountVehiclesResourceId { get; set; } = string.Empty;

    /// <summary>Open recalls not yet performed (כלי רכב שלא ביצעו ריקול).</summary>
    public string VehicleRecallsResourceId { get; set; } = string.Empty;

    /// <summary>Vehicles imported for personal use (כלי רכב ביבוא אישי).</summary>
    public string PersonalImportVehiclesResourceId { get; set; } = string.Empty;

    public string WltpMakeModelResourceId { get; set; } = string.Empty;

    /// <summary>Technical / structural vehicle history (km, color change, originality, etc.).</summary>
    public string VehicleTechnicalHistoryResourceId { get; set; } = string.Empty;

    /// <summary>Ownership-history rows (baalut + baalut_dt).</summary>
    public string VehicleOwnershipHistoryResourceId { get; set; } = string.Empty;

    /// <summary>Vehicles permanently cancelled / removed from the road (current).</summary>
    public string PermanentlyCancelledVehiclesResourceId { get; set; } = string.Empty;

    /// <summary>Permanently cancelled vehicles, model years 2010–2016.</summary>
    public string PermanentlyCancelledVehicles2010To2016ResourceId { get; set; } = string.Empty;

    /// <summary>Permanently cancelled vehicles, model years 2000–2009.</summary>
    public string PermanentlyCancelledVehicles2000To2009ResourceId { get; set; } = string.Empty;

    /// <summary>Importers and new-vehicle price lists (יבואנים ומחירוני רכב חדש).</summary>
    public string NewVehiclePriceListResourceId { get; set; } = string.Empty;

    /// <summary>Active / inactive vehicle counts by manufacturer, model and year (מאגר כמויות כלי רכב לפי תוצר, דגם ושנת יצור).</summary>
    public string VehicleCountsByModelResourceId { get; set; } = string.Empty;

    /// <summary>Manufacturers list: code, name, brand, country.</summary>
    public string ManufacturersResourceId { get; set; } = string.Empty;
}
