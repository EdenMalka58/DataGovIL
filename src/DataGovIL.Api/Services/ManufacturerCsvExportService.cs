using System.Globalization;
using System.Text;
using CsvHelper;
using DataGovIL.Api.Models;
using DataGovIL.Client;
using DataGovIL.Client.Models;
using Microsoft.Extensions.Options;

namespace DataGovIL.Api.Services;

public interface IManufacturerCsvExportService
{
    /// <summary>
    /// Fetches every WLTP make/model row, collapses to one row per (tozeret_cd, degem_cd),
    /// and writes UTF-8 BOM CSV. Must be invoked from a host-lifetime worker, not an HTTP request.
    /// </summary>
    Task ExportAsync(CancellationToken ct = default);
}

public class ManufacturerCsvExportService : IManufacturerCsvExportService
{
    private static readonly string[] ExportFields =
    [
        "tozeret_cd",
        "tozeret_nm",
        "tozar",
        "degem_cd",
        "degem_nm",
        "kinuy_mishari",
        "shnat_yitzur",
        "ramat_gimur"
    ];

    private readonly ICkanApiClient _client;
    private readonly VehicleDataResourceOptions _resources;
    private readonly ExportOptions _exportOptions;
    private readonly IWebHostEnvironment _environment;
    private readonly IManufacturerExportStatusStore _status;
    private readonly ILogger<ManufacturerCsvExportService> _logger;

    public ManufacturerCsvExportService(
        ICkanApiClient client,
        IOptions<VehicleDataResourceOptions> resources,
        IOptions<ExportOptions> exportOptions,
        IWebHostEnvironment environment,
        IManufacturerExportStatusStore status,
        ILogger<ManufacturerCsvExportService> logger)
    {
        _client = client;
        _resources = resources.Value;
        _exportOptions = exportOptions.Value;
        _environment = environment;
        _status = status;
        _logger = logger;
    }

    public async Task ExportAsync(CancellationToken ct = default)
    {
        try
        {
            var pageSize = Math.Clamp(_exportOptions.PageSize, 1, 32_000);
            var groups = new Dictionary<(string ManufacturerCode, string ModelCode), GroupAccumulator>();

            var offset = 0;
            var pagesFetched = 0;
            var recordsSeen = 0;
            int? estimatedTotal = null;

            _logger.LogInformation(
                "Starting manufacturer CSV export from resource {ResourceId} (page size {PageSize})",
                _resources.WltpMakeModelResourceId,
                pageSize);

            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var isFirstPage = pagesFetched == 0;
                var page = await FetchPageWithRetryAsync(offset, pageSize, includeTotal: isFirstPage, ct)
                    .ConfigureAwait(false);

                if (isFirstPage && page.Total is int total)
                {
                    estimatedTotal = total;
                    _logger.LogInformation(
                        "WLTP datastore reported total={Total} (treated as an estimate; stopping on a short page)",
                        total);
                }

                foreach (var record in page.Records)
                    Fold(groups, record);

                pagesFetched++;
                recordsSeen += page.Records.Count;
                _status.UpdateProgress(pagesFetched, recordsSeen, groups.Count, estimatedTotal);

                if (pagesFetched == 1 || pagesFetched % Math.Max(1, _exportOptions.ProgressLogEveryPages) == 0)
                {
                    _logger.LogInformation(
                        "Manufacturer export progress: page {Page}, offset {Offset}, raw rows {RecordsSeen}, unique pairs {Unique}{TotalSuffix}",
                        pagesFetched,
                        offset,
                        recordsSeen,
                        groups.Count,
                        estimatedTotal is int t ? $", estimated total {t}" : string.Empty);
                }

                // Do not stop on offset >= total: this resource returns total_was_estimated=true,
                // so a short page (or an empty page) is the only reliable end signal.
                if (page.Records.Count < pageSize)
                    break;

                offset += pageSize;
            }

            var rows = groups.Values
                .Select(g => g.ToCsvRow())
                .OrderBy(r => r.ManufacturerCode, StringComparer.Ordinal)
                .ThenBy(r => r.ModelCode, StringComparer.Ordinal)
                .ToList();

            var outputPath = await WriteCsvAsync(rows, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Manufacturer CSV export finished: {Unique} unique (tozeret_cd, degem_cd) rows from {Raw} raw records -> {Path}",
                rows.Count,
                recordsSeen,
                outputPath);

            _status.MarkCompleted(rows.Count, outputPath);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _status.MarkFailed("export cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manufacturer CSV export failed");
            _status.MarkFailed(ex.Message);
            throw;
        }
    }

    private async Task<DatastoreSearchResult<ManufacturerModelRecord>> FetchPageWithRetryAsync(
        int offset, int pageSize, bool includeTotal, CancellationToken ct)
    {
        var maxAttempts = Math.Max(1, _exportOptions.MaxRetries);
        Exception? last = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var query = new DatastoreSearchQuery
                {
                    ResourceId = _resources.WltpMakeModelResourceId,
                    Limit = pageSize,
                    Offset = offset,
                    IncludeTotal = includeTotal,
                    Fields = [.. ExportFields],
                    Sort = "_id asc"
                };

                return await _client.DatastoreSearchAsync<ManufacturerModelRecord>(query, ct)
                    .ConfigureAwait(false);
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
                    "datastore_search failed at offset {Offset} (attempt {Attempt}/{Max}); retrying in {DelaySeconds}s",
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

        _logger.LogError(last, "Giving up manufacturer export page at offset {Offset} after {Attempts} attempts", offset, maxAttempts);
        throw last ?? new InvalidOperationException($"datastore_search failed at offset {offset}.");
    }

    private void Fold(
        Dictionary<(string ManufacturerCode, string ModelCode), GroupAccumulator> groups,
        ManufacturerModelRecord record)
    {
        var key = (
            ManufacturerCode: record.ManufacturerCode ?? string.Empty,
            ModelCode: record.ModelCode ?? string.Empty);

        if (!groups.TryGetValue(key, out var group))
        {
            groups[key] = new GroupAccumulator(record, _logger);
            return;
        }

        group.Merge(record);
    }

    private async Task<string> WriteCsvAsync(IReadOnlyCollection<ManufacturerCsvRow> rows, CancellationToken ct)
    {
        var directory = _exportOptions.OutputDirectory;
        if (!Path.IsPathRooted(directory))
            directory = Path.Combine(_environment.ContentRootPath, directory);

        Directory.CreateDirectory(directory);

        var fileName = string.IsNullOrWhiteSpace(_exportOptions.FileName) ? "data.csv" : _exportOptions.FileName;
        var outputPath = Path.Combine(directory, fileName);
        var tempPath = outputPath + ".tmp";

        var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
        await using (var writer = new StreamWriter(stream, utf8Bom))
        await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            await csv.WriteRecordsAsync(rows, ct).ConfigureAwait(false);
            await csv.FlushAsync().ConfigureAwait(false);
        }

        File.Move(tempPath, outputPath, overwrite: true);
        return outputPath;
    }

    internal static string FormatYears(IReadOnlyCollection<int> years)
    {
        if (years.Count == 0)
            return string.Empty;

        var fromYear = years.Min();
        if (years.Count == 1)
            return $"{fromYear}-";

        return $"{fromYear}-{years.Max()}";
    }

    private sealed class GroupAccumulator
    {
        private readonly ILogger _logger;
        private readonly ManufacturerModelRecord _representative;
        private readonly HashSet<int> _years = new();

        public GroupAccumulator(ManufacturerModelRecord first, ILogger logger)
        {
            _representative = first;
            _logger = logger;
            TryAddYear(first.ModelYear);
        }

        public void Merge(ManufacturerModelRecord incoming)
        {
            TryAddYear(incoming.ModelYear);

            if (!string.Equals(_representative.ManufacturerName, incoming.ManufacturerName, StringComparison.Ordinal)
                || !string.Equals(_representative.Tozar, incoming.Tozar, StringComparison.Ordinal)
                || !string.Equals(_representative.ModelName, incoming.ModelName, StringComparison.Ordinal)
                || !string.Equals(_representative.CommercialName, incoming.CommercialName, StringComparison.Ordinal)
                || !string.Equals(_representative.TrimLevel, incoming.TrimLevel, StringComparison.Ordinal))
            {
                _logger.LogWarning(
                    "WLTP field variation for ({ManufacturerCode}, {ModelCode}): " +
                    "kept ({KeptName}, {KeptTozar}, {KeptModel}, {KeptCommercial}, {KeptTrim}) vs incoming ({IncomingName}, {IncomingTozar}, {IncomingModel}, {IncomingCommercial}, {IncomingTrim})",
                    _representative.ManufacturerCode,
                    _representative.ModelCode,
                    _representative.ManufacturerName,
                    _representative.Tozar,
                    _representative.ModelName,
                    _representative.CommercialName,
                    _representative.TrimLevel,
                    incoming.ManufacturerName,
                    incoming.Tozar,
                    incoming.ModelName,
                    incoming.CommercialName,
                    incoming.TrimLevel);
            }
        }

        public ManufacturerCsvRow ToCsvRow() => new()
        {
            ManufacturerCode = _representative.ManufacturerCode,
            ManufacturerName = _representative.ManufacturerName,
            Tozar = _representative.Tozar,
            ModelCode = _representative.ModelCode,
            ModelName = _representative.ModelName,
            CommercialName = _representative.CommercialName,
            TrimLevel = _representative.TrimLevel,
            Years = FormatYears(_years)
        };

        private void TryAddYear(string? raw)
        {
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var year) && year > 0)
                _years.Add(year);
        }
    }
}
