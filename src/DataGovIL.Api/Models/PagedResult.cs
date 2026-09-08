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

    public string WltpMakeModelResourceId { get; set; } = string.Empty;
}
