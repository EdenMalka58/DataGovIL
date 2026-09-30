namespace DataGovIL.Api.Services;

/// <summary>
/// Drains <see cref="IManufacturerExportWorkQueue"/> on the host lifetime so a client
/// disconnect or request timeout cannot cancel the export.
/// </summary>
public sealed class ManufacturerExportBackgroundService : BackgroundService
{
    private readonly IManufacturerExportWorkQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IManufacturerExportStatusStore _status;
    private readonly ILogger<ManufacturerExportBackgroundService> _logger;

    public ManufacturerExportBackgroundService(
        IManufacturerExportWorkQueue queue,
        IServiceScopeFactory scopeFactory,
        IManufacturerExportStatusStore status,
        ILogger<ManufacturerExportBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _status = status;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Manufacturer catalog sync worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _queue.WaitForWorkAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var export = scope.ServiceProvider.GetRequiredService<IManufacturerCsvExportService>();
                await export.ExportAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _status.MarkFailed("export cancelled because the host is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manufacturer catalog sync worker caught an unhandled failure");
                _status.MarkFailed(ex.Message);
            }
        }

        _logger.LogInformation("Manufacturer catalog sync worker stopped");
    }
}
