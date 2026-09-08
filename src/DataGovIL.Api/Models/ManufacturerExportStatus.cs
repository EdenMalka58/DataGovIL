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

    public string? OutputFile { get; init; }

    public string? Error { get; init; }
}
