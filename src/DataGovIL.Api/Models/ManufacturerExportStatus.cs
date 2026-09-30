namespace DataGovIL.Api.Models;

public static class ManufacturerExportStates
{
    public const string NotStarted = "notStarted";
    public const string Running = "running";
    public const string Completed = "completed";
    public const string Failed = "failed";
}

/// <summary>Snapshot of the in-process manufacturer CSV export job (single-instance deployment).</summary>
public sealed record ManufacturerExportStatus
{
    public string State { get; init; } = ManufacturerExportStates.NotStarted;

    public string Message { get; init; } = "not started";

    public int PagesFetched { get; init; }

    public int RecordsSeen { get; init; }

    public int UniqueRowCount { get; init; }

    /// <summary>
    /// CKAN's include_total figure from the first page. The WLTP resource currently returns
    /// an estimate (<c>total_was_estimated</c>), so this is progress context only — not a
    /// stop condition.
    /// </summary>
    public int? EstimatedTotalRawRows { get; init; }

    public DateTimeOffset? StartedAt { get; init; }

    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>
    /// Destination of the last successful sync (e.g. <c>postgresql:public.manufacturer_models</c>).
    /// Kept as <c>OutputFile</c> for API compatibility with the former CSV export.
    /// </summary>
    public string? OutputFile { get; init; }

    /// <summary>Totals across all synced tables; see <see cref="Tables"/> for the per-table breakdown.</summary>
    public int? Inserted { get; init; }

    public int? Updated { get; init; }

    public int? Deleted { get; init; }

    public int? Unchanged { get; init; }

    /// <summary>Delta per table, keyed by table name (e.g. <c>manufacturers</c>, <c>manufacturer_models</c>).</summary>
    public IReadOnlyDictionary<string, ManufacturerSyncDelta>? Tables { get; init; }

    public string? Error { get; init; }
}

/// <summary>Counts from a manufacturer catalog delta sync.</summary>
public readonly record struct ManufacturerSyncDelta(int Inserted, int Updated, int Deleted, int Unchanged);
