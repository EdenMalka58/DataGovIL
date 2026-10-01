using DataGovIL.Api.Models;
using DataGovIL.Api.Services;
using DataGovIL.Client;
using Microsoft.AspNetCore.Mvc;

namespace DataGovIL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : ControllerBase
{
    private const decimal MaxKmPerMonth = 20000m;

    private readonly IVehicleService _vehicleService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IVehicleEnergyCostService _energyCostService;
    private readonly ILogger<VehiclesController> _logger;

    public VehiclesController(
        IVehicleService vehicleService,
        IManufacturerService manufacturerService,
        IVehicleEnergyCostService energyCostService,
        ILogger<VehiclesController> logger)
    {
        _vehicleService = vehicleService;
        _manufacturerService = manufacturerService;
        _energyCostService = energyCostService;
        _logger = logger;
    }

    /// <summary>
    /// Estimated monthly energy cost for a WLTP make/model (<c>tozeret_cd</c> + <c>degem_cd</c>),
    /// from configurable price and consumption defaults. Always an estimate, not an official figure.
    /// Plug-in hybrids and unrecognized fuels return <c>energyType: Unknown</c> with a null cost.
    /// The default-mileage cost is already embedded in <see cref="VehicleRecord.EnergyCost"/>;
    /// call this when the user changes the monthly mileage.
    /// </summary>
    /// <param name="manufacturerCode">Manufacturer code (<c>tozeret_cd</c>).</param>
    /// <param name="modelCode">Model code (<c>degem_cd</c>).</param>
    /// <param name="kmPerMonth">Optional monthly distance (0-20000); defaults to the configured value.</param>
    [HttpGet("{manufacturerCode}/{modelCode}/energy-cost")]
    [ProducesResponseType(typeof(VehicleEnergyCost), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleEnergyCost>> GetEnergyCost(
        string manufacturerCode, string modelCode, [FromQuery] decimal? kmPerMonth, CancellationToken ct)
    {
        if (kmPerMonth is < 0m or > MaxKmPerMonth)
            return BadRequest($"kmPerMonth must be between 0 and {MaxKmPerMonth}.");

        try
        {
            var model = await _manufacturerService.GetByCodesAsync(manufacturerCode, modelCode, ct);
            return model is null ? NotFound() : Ok(_energyCostService.CalculateCost(model, kmPerMonth));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (CkanApiException ex)
        {
            _logger.LogError(ex, "data.gov.il model lookup failed for energy cost {ManufacturerCode}/{ModelCode}", manufacturerCode, modelCode);
            return Problem(title: "Upstream data.gov.il error", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Technical + ownership history for a registration number
    /// (<c>mispar_rechev</c>) from the two history datastore resources.
    /// </summary>
    /// <param name="registrationNumber">e.g. "12345678".</param>
    [HttpGet("{registrationNumber}/history")]
    [ProducesResponseType(typeof(VehicleHistoryRecord), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleHistoryRecord>> GetHistoryByRegistrationNumber(
        string registrationNumber, CancellationToken ct)
    {
        try
        {
            var history = await _vehicleService.GetHistoryByRegistrationNumberAsync(registrationNumber, ct);
            return history is null ? NotFound() : Ok(history);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (CkanApiException ex)
        {
            _logger.LogError(ex, "data.gov.il history lookup failed for registration {RegistrationNumber}", registrationNumber);
            return Problem(title: "Upstream data.gov.il error", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    /// <summary>
    /// Look up a single vehicle by its exact registration number (מספר רישוי).
    /// When the private/commercial registry misses, tries active public vehicles (taxis, shared taxis, buses),
    /// vehicles over 3.5 tons or without a model code (trucks, tractors, trailers), and two-wheelers
    /// (motorcycles, scooters), then permanently-cancelled ("ביטול סופי") resources, inactive ("לא פעיל")
    /// resources, and personal-import vehicles. <c>source</c> names the registry that answered and
    /// <c>vehicleTypeName</c> carries that registry's vehicle type. A found vehicle is also checked
    /// against the safety-systems discount list and the open-recall list, and gets a
    /// depreciation calculation (kilometers, owner count, ownership type) from its history and price list,
    /// the estimated monthly energy cost at the default mileage (<c>energyCost</c>),
    /// the new-registration popularity of its model code
    /// (<c>modelPopularity</c>: total across every closure month, plus the count per month),
    /// and how many vehicles of that model code are still active in every manufacture year
    /// (<c>modelFleet</c>: each year plus the sum across years).
    /// </summary>
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
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
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
