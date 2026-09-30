using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DataGovIL.Api.Data;
using DataGovIL.Api.Models;
using DataGovIL.Client;
using DataGovIL.Client.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DataGovIL.Api.Services;

public interface IManufacturerCsvExportService
{
    /// <summary>
    /// Syncs the manufacturers list (<c>manufacturers</c>) and every WLTP make/model/year row
    /// joined with active/inactive vehicle counts (<c>manufacturer_models</c>). Each table gets a
    /// delta sync (insert new, update changed, delete removed); both commit in one transaction.
    /// Must be invoked from a host-lifetime worker, not an HTTP request.
    /// </summary>
    Task ExportAsync(CancellationToken ct = default);
}

public class ManufacturerCsvExportService : IManufacturerCsvExportService
{
    private const string DbDestination = "postgresql:public.manufacturers,public.manufacturer_models";
    private const string LockTimeout = "30s";
    private const string IdleInTransactionTimeout = "2min";

    private readonly ICkanApiClient _client;
    private readonly VehicleDataResourceOptions _resources;
    private readonly ExportOptions _exportOptions;
    private readonly AppDbContext _db;
    private readonly IManufacturerExportStatusStore _status;
    private readonly ILogger<ManufacturerCsvExportService> _logger;

    public ManufacturerCsvExportService(
        ICkanApiClient client,
        IOptions<VehicleDataResourceOptions> resources,
        IOptions<ExportOptions> exportOptions,
        AppDbContext db,
        IManufacturerExportStatusStore status,
        ILogger<ManufacturerCsvExportService> logger)
    {
        _client = client;
        _resources = resources.Value;
        _exportOptions = exportOptions.Value;
        _db = db;
        _status = status;
        _logger = logger;
    }

    public async Task ExportAsync(CancellationToken ct = default)
    {
        try
        {
            var pagesFetched = 0;

            _logger.LogInformation(
                "Starting manufacturer catalog sync from manufacturers resource {ManufacturersResourceId}, " +
                "WLTP resource {WltpResourceId} + counts resource {CountsResourceId} -> {Destination}",
                _resources.ManufacturersResourceId,
                _resources.WltpMakeModelResourceId,
                _resources.VehicleCountsByModelResourceId,
                DbDestination);

            var manufacturers = new Dictionary<int, ManufacturerEntity>();
            var manufacturersSkipped = 0;
            await foreach (var page in FetchAllPagesAsync<ManufacturerDatastoreRecord>(
                               _resources.ManufacturersResourceId, ct).ConfigureAwait(false))
            {
                foreach (var record in page.Records)
                {
                    if (ManufacturerEntity.FromDatastore(record) is { } manufacturer)
                        manufacturers[manufacturer.ManufacturerCode] = manufacturer;
                    else
                        manufacturersSkipped++;
                }

                pagesFetched++;
                _status.UpdateProgress(pagesFetched, 0, 0, null);
            }

            if (manufacturersSkipped > 0)
                _logger.LogWarning("Skipped {Skipped} manufacturer rows with a missing or non-integer code", manufacturersSkipped);
            _logger.LogInformation("Loaded {Count} manufacturers", manufacturers.Count);

            var counts = new Dictionary<ModelKey, VehicleModelCountDatastoreRecord>();
            var countsSeen = 0;
            await foreach (var page in FetchAllPagesAsync<VehicleModelCountDatastoreRecord>(
                               _resources.VehicleCountsByModelResourceId, ct).ConfigureAwait(false))
            {
                foreach (var record in page.Records)
                {
                    countsSeen++;
                    if (ManufacturerModelEntity.TryParseKey(
                            record.ManufacturerCode, record.ModelCode, record.ModelYear, record.ModelType, out var key))
                    {
                        counts[key] = record;
                    }
                }

                pagesFetched++;
                _status.UpdateProgress(pagesFetched, 0, 0, null);
            }

            _logger.LogInformation("Loaded {Count} vehicle-count rows ({Unique} unique keys)", countsSeen, counts.Count);

            var existingTexts = await _db.Texts.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);
            var texts = new TextCatalog(existingTexts, _logger);

            var rows = new Dictionary<ModelKey, ManufacturerModelEntity>();
            var recordsSeen = 0;
            var skipped = 0;
            var withCounts = 0;
            int? estimatedTotal = null;

            await foreach (var page in FetchAllPagesAsync<ManufacturerModelDatastoreRecord>(
                               _resources.WltpMakeModelResourceId, ct).ConfigureAwait(false))
            {
                estimatedTotal ??= page.Total;

                foreach (var record in page.Records)
                {
                    recordsSeen++;
                    if (!ManufacturerModelEntity.TryParseKey(
                            record.ManufacturerCode, record.ModelCode, record.ModelYear, record.ModelType, out var key))
                    {
                        skipped++;
                        continue;
                    }

                    counts.TryGetValue(key, out var count);
                    if (count is not null)
                        withCounts++;

                    if (rows.ContainsKey(key))
                        _logger.LogWarning("Duplicate WLTP key {Key}; keeping the last row", key);

                    rows[key] = ManufacturerModelEntity.FromDatastore(key, record, count, texts);
                }

                pagesFetched++;
                _status.UpdateProgress(pagesFetched, recordsSeen, rows.Count, estimatedTotal);
            }

            if (skipped > 0)
                _logger.LogWarning("Skipped {Skipped} WLTP rows with a missing or non-integer key", skipped);

            // Plan both deltas before opening the transaction so no locks are held while reading.
            var existingManufacturers = await _db.Manufacturers
                .AsNoTracking()
                .Select(e => new { e.ManufacturerCode, e.ContentHash })
                .ToDictionaryAsync(e => e.ManufacturerCode, e => e.ContentHash, ct)
                .ConfigureAwait(false);
            var manufacturersPlan = PlanDelta(
                manufacturers,
                existingManufacturers,
                code => new ManufacturerEntity { ManufacturerCode = code });

            var existingModels = await LoadExistingModelHashesAsync(ct).ConfigureAwait(false);
            var modelsPlan = PlanDelta(
                rows,
                existingModels,
                key => new ManufacturerModelEntity
                {
                    ManufacturerCode = key.ManufacturerCode,
                    ModelCode = key.ModelCode,
                    ModelYear = key.ModelYear,
                    ModelType = key.ModelType
                });

            var textsPlan = texts.Plan();

            await using (var tx = await _db.Database.BeginTransactionAsync(ct).ConfigureAwait(false))
            {
                // If this process dies mid-write, the server ends the session instead of
                // keeping row locks forever; a blocked write fails fast instead of hanging.
                await _db.Database.ExecuteSqlRawAsync(
                        $"set local lock_timeout = '{LockTimeout}'; set local idle_in_transaction_session_timeout = '{IdleInTransactionTimeout}'",
                        ct)
                    .ConfigureAwait(false);

                var batchSize = Math.Clamp(_exportOptions.DbBatchSize, 100, 10_000);
                await SaveInBatchesAsync(textsPlan.ToInsert, batchSize, _db.Texts.AddRange, "insert", ct).ConfigureAwait(false);
                await SaveInBatchesAsync(textsPlan.ToUpdate, batchSize, _db.Texts.UpdateRange, "update", ct).ConfigureAwait(false);

                await ApplyDeltaAsync(manufacturersPlan, ct).ConfigureAwait(false);
                await ApplyDeltaAsync(modelsPlan, ct).ConfigureAwait(false);
                await tx.CommitAsync(ct).ConfigureAwait(false);
            }

            var textsDelta = new ManufacturerSyncDelta(textsPlan.ToInsert.Count, textsPlan.ToUpdate.Count, 0, textsPlan.Unchanged);
            var manufacturersDelta = manufacturersPlan.ToDelta();
            var modelsDelta = modelsPlan.ToDelta();

            LogDelta("texts", textsPlan.ToInsert.Count + textsPlan.ToUpdate.Count + textsPlan.Unchanged, textsDelta);
            LogDelta("manufacturers", manufacturers.Count, manufacturersDelta);
            LogDelta("manufacturer_models", rows.Count, modelsDelta);
            _logger.LogInformation(
                "Manufacturer catalog sync finished: {Unique} model rows ({WithCounts} with vehicle counts) from {Raw} raw records -> {Destination}",
                rows.Count,
                withCounts,
                recordsSeen,
                DbDestination);

            _status.MarkCompleted(rows.Count, DbDestination, new Dictionary<string, ManufacturerSyncDelta>
            {
                ["texts"] = textsDelta,
                ["manufacturers"] = manufacturersDelta,
                ["manufacturer_models"] = modelsDelta
            });
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _status.MarkFailed("export cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manufacturer catalog sync failed");
            _status.MarkFailed(ex.Message);
            throw;
        }
    }

    private async Task<Dictionary<ModelKey, string?>> LoadExistingModelHashesAsync(CancellationToken ct)
    {
        var pageSize = Math.Clamp(_exportOptions.DbReadPageSize, 1_000, 100_000);
        var hashes = new Dictionary<ModelKey, string?>();

        for (var offset = 0; ; offset += pageSize)
        {
            var page = await _db.ManufacturerModels
                .AsNoTracking()
                .OrderBy(e => e.ManufacturerCode)
                .ThenBy(e => e.ModelCode)
                .ThenBy(e => e.ModelYear)
                .ThenBy(e => e.ModelType)
                .Skip(offset)
                .Take(pageSize)
                .Select(e => new { e.ManufacturerCode, e.ModelCode, e.ModelYear, e.ModelType, e.ContentHash })
                .ToListAsync(ct)
                .ConfigureAwait(false);

            foreach (var e in page)
                hashes[new ModelKey(e.ManufacturerCode, e.ModelCode, e.ModelYear, e.ModelType)] = e.ContentHash;

            if (page.Count < pageSize)
                return hashes;
        }
    }

    private void LogDelta(string table, int rowCount, ManufacturerSyncDelta delta) =>
        _logger.LogInformation(
            "{Table}: {Rows} source rows (inserted={Inserted}, updated={Updated}, deleted={Deleted}, unchanged={Unchanged})",
            table,
            rowCount,
            delta.Inserted,
            delta.Updated,
            delta.Deleted,
            delta.Unchanged);

    /// <summary>
    /// Works out the delta for one table by comparing content hashes: insert new keys, update keys
    /// whose hash changed, delete keys missing from the source. Unchanged rows are left alone.
    /// </summary>
    private TablePlan<TEntity> PlanDelta<TEntity, TKey>(
        IReadOnlyDictionary<TKey, TEntity> incoming,
        IReadOnlyDictionary<TKey, string?> existingHashes,
        Func<TKey, TEntity> keyOnlyEntity)
        where TEntity : class, ISyncedEntity
        where TKey : notnull
    {
        var now = DateTimeOffset.UtcNow;
        var contentProperties = GetContentProperties<TEntity>();
        var plan = new TablePlan<TEntity>();

        foreach (var (key, row) in incoming)
        {
            row.ContentHash = ComputeContentHash(row, contentProperties);
            row.UpdatedAt = now;

            if (!existingHashes.TryGetValue(key, out var existingHash))
                plan.ToInsert.Add(row);
            else if (string.Equals(existingHash, row.ContentHash, StringComparison.Ordinal))
                plan.Unchanged++;
            else
                plan.ToUpdate.Add(row);
        }

        foreach (var key in existingHashes.Keys)
        {
            if (!incoming.ContainsKey(key))
                plan.ToDelete.Add(keyOnlyEntity(key));
        }

        return plan;
    }

    private async Task ApplyDeltaAsync<TEntity>(TablePlan<TEntity> plan, CancellationToken ct)
        where TEntity : class, ISyncedEntity
    {
        var set = _db.Set<TEntity>();
        var batchSize = Math.Clamp(_exportOptions.DbBatchSize, 100, 10_000);

        await SaveInBatchesAsync(plan.ToDelete, batchSize, set.RemoveRange, "delete", ct).ConfigureAwait(false);
        await SaveInBatchesAsync(plan.ToInsert, batchSize, set.AddRange, "insert", ct).ConfigureAwait(false);
        await SaveInBatchesAsync(plan.ToUpdate, batchSize, set.UpdateRange, "update", ct).ConfigureAwait(false);
    }

    private sealed class TablePlan<TEntity>
    {
        public List<TEntity> ToInsert { get; } = new();
        public List<TEntity> ToUpdate { get; } = new();
        public List<TEntity> ToDelete { get; } = new();
        public int Unchanged { get; set; }

        public ManufacturerSyncDelta ToDelta() => new(ToInsert.Count, ToUpdate.Count, ToDelete.Count, Unchanged);
    }

    private async Task SaveInBatchesAsync<TEntity>(
        List<TEntity> rows,
        int batchSize,
        Action<IEnumerable<TEntity>> stage,
        string operation,
        CancellationToken ct)
    {
        for (var i = 0; i < rows.Count; i += batchSize)
        {
            ct.ThrowIfCancellationRequested();
            var count = Math.Min(batchSize, rows.Count - i);
            stage(rows.GetRange(i, count));
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);
            _db.ChangeTracker.Clear();

            _logger.LogInformation(
                "{Table} {Operation}: {Done}/{Total} rows written",
                typeof(TEntity).Name,
                operation,
                i + count,
                rows.Count);
        }
    }

    private PropertyInfo[] GetContentProperties<TEntity>() =>
        _db.Model.FindEntityType(typeof(TEntity))!
            .GetProperties()
            .Where(p => !p.IsPrimaryKey()
                        && p.Name != nameof(ISyncedEntity.UpdatedAt)
                        && p.Name != nameof(ISyncedEntity.ContentHash))
            .Select(p => p.PropertyInfo!)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToArray();

    /// <summary>
    /// SHA-256 (first 16 bytes, hex) of the content columns. Decimals are normalized so
    /// <c>327.0000</c> and <c>327</c> hash the same.
    /// </summary>
    private static string ComputeContentHash(object entity, PropertyInfo[] properties)
    {
        var builder = new StringBuilder();
        foreach (var property in properties)
        {
            builder.Append(property.GetValue(entity) switch
            {
                null => "\u2400",
                decimal d => d.ToString("0.############################", CultureInfo.InvariantCulture),
                IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
                var v => v.ToString()
            });
            builder.Append('\u001F');
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()));
        return Convert.ToHexString(hash, 0, 16);
    }

    private async IAsyncEnumerable<DatastoreSearchResult<T>> FetchAllPagesAsync<T>(
        string resourceId,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var pageSize = Math.Clamp(_exportOptions.PageSize, 1, 32_000);
        var offset = 0;

        while (true)
        {
            ct.ThrowIfCancellationRequested();

            var page = await FetchPageWithRetryAsync<T>(resourceId, offset, pageSize, includeTotal: offset == 0, ct)
                .ConfigureAwait(false);
            yield return page;

            // Do not stop on offset >= total: these resources return total_was_estimated=true,
            // so a short page (or an empty page) is the only reliable end signal.
            if (page.Records.Count < pageSize)
                yield break;

            offset += pageSize;
        }
    }

    private async Task<DatastoreSearchResult<T>> FetchPageWithRetryAsync<T>(
        string resourceId, int offset, int pageSize, bool includeTotal, CancellationToken ct)
    {
        var maxAttempts = Math.Max(1, _exportOptions.MaxRetries);
        Exception? last = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var query = new DatastoreSearchQuery
                {
                    ResourceId = resourceId,
                    Limit = pageSize,
                    Offset = offset,
                    IncludeTotal = includeTotal,
                    Sort = "_id asc"
                };

                return await _client.DatastoreSearchAsync<T>(query, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                last = ex;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                _logger.LogWarning(
                    ex,
                    "datastore_search on {ResourceId} failed at offset {Offset} (attempt {Attempt}/{Max}); retrying in {DelaySeconds}s",
                    resourceId,
                    offset,
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);
                await Task.Delay(delay, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                last = ex;
            }
        }

        _logger.LogError(last, "Giving up on {ResourceId} at offset {Offset} after {Attempts} attempts", resourceId, offset, maxAttempts);
        throw last ?? new InvalidOperationException($"datastore_search on {resourceId} failed at offset {offset}.");
    }
}
