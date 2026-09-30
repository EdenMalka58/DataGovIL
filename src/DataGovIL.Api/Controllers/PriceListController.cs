using DataGovIL.Api.Models;
using DataGovIL.Api.Services;
using DataGovIL.Client;
using Microsoft.AspNetCore.Mvc;

namespace DataGovIL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PriceListController : ControllerBase
{
    private readonly IPriceListService _priceListService;
    private readonly ILogger<PriceListController> _logger;

    public PriceListController(IPriceListService priceListService, ILogger<PriceListController> logger)
    {
        _priceListService = priceListService;
        _logger = logger;
    }

    /// <summary>
    /// Look up new-vehicle price-list rows by manufacturer code (<c>tozeret_cd</c>),
    /// model code (<c>degem_cd</c>), and manufacture year (<c>shnat_yitzur</c>).
    /// </summary>
    /// <param name="manufacturerCode">e.g. "5".</param>
    /// <param name="modelCode">e.g. "179".</param>
    /// <param name="manufactureYear">e.g. "2024".</param>
    [HttpGet("{manufacturerCode}/{modelCode}/{manufactureYear}")]
    [ProducesResponseType(typeof(IReadOnlyList<VehiclePriceListRecord>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<VehiclePriceListRecord>>> GetByCodesAndYear(
        string manufacturerCode,
        string modelCode,
        string manufactureYear,
        CancellationToken ct)
    {
        try
        {
            var items = await _priceListService.GetByCodesAndYearAsync(
                manufacturerCode, modelCode, manufactureYear, ct);
            return items.Count == 0 ? NotFound() : Ok(items);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (CkanApiException ex)
        {
            _logger.LogError(
                ex,
                "data.gov.il price-list lookup failed for manufacturer {ManufacturerCode} / model {ModelCode} / year {ManufactureYear}",
                manufacturerCode,
                modelCode,
                manufactureYear);
            return Problem(title: "Upstream data.gov.il error", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
