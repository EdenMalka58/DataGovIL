namespace DataGovIL.Api.Models;

/// <summary>
/// Paging / retry knobs for the manufacturer/model catalog sync into Postgres.
/// Bound from the "ExportOptions" configuration section.
/// </summary>
public class ExportOptions
{
    /// <summary>Rows per datastore_search page. The WLTP resource accepts at least 1000.</summary>
    public int PageSize { get; set; } = 1000;

    /// <summary>Rows per EF Core SaveChanges batch when writing to Postgres.</summary>
    public int DbBatchSize { get; set; } = 1000;

    /// <summary>
    /// Rows per query when reading existing keys + hashes, so each statement stays well under
    /// Supabase's statement_timeout even on a slow link.
    /// </summary>
    public int DbReadPageSize { get; set; } = 10_000;

    /// <summary>EF Core command timeout for Postgres (seconds). Supabase also enforces its own statement_timeout.</summary>
    public int DbCommandTimeoutSeconds { get; set; } = 120;

    public int MaxRetries { get; set; } = 3;

    public int ProgressLogEveryPages { get; set; } = 5;
}
