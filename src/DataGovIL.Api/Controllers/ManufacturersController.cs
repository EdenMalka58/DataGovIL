using DataGovIL.Api.Models;
using DataGovIL.Api.Services;
using DataGovIL.Client;
using Microsoft.AspNetCore.Mvc;

namespace DataGovIL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturersController : ControllerBase
{
    private readonly IManufacturerService _manufacturerService;
    private readonly IManufacturerExportWorkQueue _exportQueue;
    private readonly IManufacturerExportStatusStore _exportStatus;
    private readonly ILogger<ManufacturersController> _logger;

    public ManufacturersController(
        IManufacturerService manufacturerService,
        IManufacturerExportWorkQueue exportQueue,
        IManufacturerExportStatusStore exportStatus,
        ILogger<ManufacturersController> logger)
    {
        _manufacturerService = manufacturerService;
        _exportQueue = exportQueue;
        _exportStatus = exportStatus;
        _logger = logger;
    }

    /// <summary>
    /// Paginated list of car manufacturers and models (from the WLTP makes/models dataset).
    /// </summary>
    /// <param name="manufacturer">Optional exact manufacturer name filter (e.g. "טויוטה יפן").</param>
    /// <param name="model">Optional commercial model-name search (e.g. "COROLLA" also matches "COROLLA CROSS").</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Rows per page (max 500).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ManufacturerModelRecord>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ManufacturerModelRecord>>> GetManufacturersAndModels(
        [FromQuery] string? manufacturer,
        [FromQuery] string? model,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 500);
        page = Math.Max(page, 1);

        try
        {
            var result = await _manufacturerService.GetManufacturersAndModelsAsync(manufacturer, model, page, pageSize, ct);
            return Ok(result);
        }
        catch (CkanApiException ex)
        {
            _logger.LogError(ex, "data.gov.il manufacturer/model lookup failed");
            return Problem(title: "Upstream data.gov.il error", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Queues a background export of the full WLTP make/model catalog to data.csv
    /// (one row per unique tozeret_cd + degem_cd). Returns immediately.
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(ManufacturerExportStatus), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ManufacturerExportStatus), StatusCodes.Status409Conflict)]
    public ActionResult<ManufacturerExportStatus> StartExport()
    {
        if (!_exportStatus.TryBegin())
            return Conflict(_exportStatus.Snapshot);

        if (!_exportQueue.TryEnqueue())
        {
            _exportStatus.MarkFailed("failed to queue the export");
            return Problem(title: "Failed to queue manufacturer export", statusCode: StatusCodes.Status500InternalServerError);
        }

        return AcceptedAtAction(nameof(GetExportStatus), _exportStatus.Snapshot);
    }

    /// <summary>Poll progress/completion of the manufacturer CSV export.</summary>
    [HttpGet("export/status")]
    [ProducesResponseType(typeof(ManufacturerExportStatus), StatusCodes.Status200OK)]
    public ActionResult<ManufacturerExportStatus> GetExportStatus()
        => Ok(_exportStatus.Snapshot);
}
