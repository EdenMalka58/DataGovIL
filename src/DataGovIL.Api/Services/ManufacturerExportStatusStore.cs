using DataGovIL.Api.Models;

namespace DataGovIL.Api.Services;

public interface IManufacturerExportStatusStore
{
    ManufacturerExportStatus Snapshot { get; }

    /// <summary>Atomically claims the single export slot and marks the job running.</summary>
    bool TryBegin();

    void UpdateProgress(int pagesFetched, int recordsSeen, int uniqueRows, int? estimatedTotalRawRows);

    void MarkCompleted(int uniqueRows, string outputFile);

    void MarkFailed(string error);
}

/// <summary>
/// Process-wide status for the manufacturer CSV export. Enough for a single-instance
/// deployment; a durable job store is not required yet.
/// </summary>
public sealed class ManufacturerExportStatusStore : IManufacturerExportStatusStore
{
    private readonly object _gate = new();
    private ManufacturerExportStatus _status = new();
    private int _busy;

    public ManufacturerExportStatus Snapshot
    {
        get
        {
            lock (_gate)
            {
                return _status;
            }
        }
    }

    public bool TryBegin()
    {
        if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
            return false;

        lock (_gate)
        {
            _status = new ManufacturerExportStatus
            {
                State = ManufacturerExportStates.Running,
                Message = "running, 0 pages fetched",
                StartedAt = DateTimeOffset.UtcNow
            };
        }

        return true;
    }

    public void UpdateProgress(int pagesFetched, int recordsSeen, int uniqueRows, int? estimatedTotalRawRows)
    {
        lock (_gate)
        {
            _status = _status with
            {
                State = ManufacturerExportStates.Running,
                Message = $"running, {pagesFetched} pages fetched",
                PagesFetched = pagesFetched,
                RecordsSeen = recordsSeen,
                UniqueRowCount = uniqueRows,
                EstimatedTotalRawRows = estimatedTotalRawRows ?? _status.EstimatedTotalRawRows
            };
        }
    }

    public void MarkCompleted(int uniqueRows, string outputFile)
    {
        lock (_gate)
        {
            if (_status.State != ManufacturerExportStates.Running)
                return;

            var completedAt = DateTimeOffset.UtcNow;
            _status = _status with
            {
                State = ManufacturerExportStates.Completed,
                Message = $"completed at {completedAt:O}, {uniqueRows} rows",
                UniqueRowCount = uniqueRows,
                CompletedAt = completedAt,
                OutputFile = outputFile,
                Error = null
            };
            Interlocked.Exchange(ref _busy, 0);
        }
    }

    public void MarkFailed(string error)
    {
        lock (_gate)
        {
            if (_status.State != ManufacturerExportStates.Running)
                return;

            _status = _status with
            {
                State = ManufacturerExportStates.Failed,
                Message = $"failed: {error}",
                CompletedAt = DateTimeOffset.UtcNow,
                Error = error
            };
            Interlocked.Exchange(ref _busy, 0);
        }
    }
}
