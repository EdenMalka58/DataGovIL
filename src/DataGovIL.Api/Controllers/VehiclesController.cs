using DataGovIL.Api.Models;
using DataGovIL.Api.Services;
using DataGovIL.Client;
using Microsoft.AspNetCore.Mvc;

namespace DataGovIL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(IVehicleService vehicleService, ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _logger = logger;
    }

    /// <summary>Look up a single vehicle by its exact registration number (מספר רישוי).</summary>
    /// <param name="registrationNumber">e.g. "12345678".</param>
    [HttpGet("{registrationNumber}")]
    [ProducesResponseType(typeof(VehicleRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleRecord>> GetByRegistrationNumber(
        string registrationNumber, CancellationToken ct)
    {
        try
        {
            var vehicle = await _vehicleService.SearchByRegistrationNumberAsync(registrationNumber, ct);
            return vehicle is null ? NotFound() : Ok(vehicle);
        }
        catch (CkanApiException ex)
        {
            _logger.LogError(ex, "data.gov.il lookup failed for registration {RegistrationNumber}", registrationNumber);
            return Problem(title: "Upstream data.gov.il error", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>Paged/free-text search across the vehicle registration dataset.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<VehicleRecord>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<VehicleRecord>>> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 500);
        page = Math.Max(page, 1);

        try
        {
            var result = await _vehicleService.SearchAsync(q, page, pageSize, ct);
            return Ok(result);
        }
        catch (CkanApiException ex)
        {
            _logger.LogError(ex, "data.gov.il vehicle search failed");
            return Problem(title: "Upstream data.gov.il error", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
