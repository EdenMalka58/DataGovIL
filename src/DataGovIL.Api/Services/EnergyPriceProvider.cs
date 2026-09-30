using DataGovIL.Api.Models;
using Microsoft.Extensions.Options;

namespace DataGovIL.Api.Services;

/// <summary>
/// Source of energy prices and default consumption figures for the monthly cost estimate.
/// A live source (e.g. a data.gov.il fuel-price resource) can replace the configuration-backed
/// implementation without changing <see cref="VehicleEnergyCostService"/> or the API contract.
/// </summary>
public interface IEnergyPriceProvider
{
    decimal DefaultKmPerMonth { get; }

    /// <summary>ILS per liter (fuel) or per kWh (electric); null for <see cref="VehicleEnergyType.Unknown"/>.</summary>
    decimal? GetPrice(VehicleEnergyType energyType);

    /// <summary>km per liter (fuel) or kWh per 100 km (electric); null for <see cref="VehicleEnergyType.Unknown"/>.</summary>
    decimal? GetDefaultConsumption(VehicleEnergyType energyType);
}

/// <summary>Reads prices and consumption from the "VehicleCost" configuration section.</summary>
public class ConfigEnergyPriceProvider : IEnergyPriceProvider
{
    private readonly IOptionsMonitor<VehicleCostOptions> _options;

    public ConfigEnergyPriceProvider(IOptionsMonitor<VehicleCostOptions> options)
    {
        _options = options;
    }

    public decimal DefaultKmPerMonth => _options.CurrentValue.KmPerMonth;

    public decimal? GetPrice(VehicleEnergyType energyType)
    {
        var prices = _options.CurrentValue.FuelPrices;
        return energyType switch
        {
            VehicleEnergyType.Gasoline => prices.Gasoline95,
            VehicleEnergyType.Diesel => prices.Diesel,
            VehicleEnergyType.Electric => prices.ElectricityPerKwh,
            _ => null
        };
    }

    public decimal? GetDefaultConsumption(VehicleEnergyType energyType)
    {
        var consumption = _options.CurrentValue.EstimatedConsumption;
        return energyType switch
        {
            VehicleEnergyType.Gasoline => consumption.GasolineKmPerLiter,
            VehicleEnergyType.Diesel => consumption.DieselKmPerLiter,
            VehicleEnergyType.Electric => consumption.ElectricKwhPer100Km,
            _ => null
        };
    }
}
