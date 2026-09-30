namespace DataGovIL.Api.Models;

/// <summary>Bound from the "SystemOptions" configuration section.</summary>
public class SystemOptions
{
    /// <summary>
    /// When true, a registration lookup loads the WLTP make/model row from Postgres.
    /// When false, that row is read from the data.gov.il datastore.
    /// </summary>
    public bool UseDB { get; set; }
}
