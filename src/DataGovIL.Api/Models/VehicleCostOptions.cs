namespace DataGovIL.Api.Models;

/// <summary>
/// Bound from the "VehicleCost" configuration section. Defaults for the estimated
/// monthly energy cost; these are app defaults, not official Ministry of Transport figures.
/// </summary>
public class VehicleCostOptions
{
    public decimal KmPerMonth { get; set; } = 1500m;

    public VehicleFuelPrices FuelPrices { get; set; } = new();

    public VehicleEstimatedConsumption EstimatedConsumption { get; set; } = new();
}

public class VehicleFuelPrices
{
    /// <summary>ILS per liter.</summary>
    public decimal Gasoline95 { get; set; } = 7.20m;

    /// <summary>ILS per liter.</summary>
    public decimal Diesel { get; set; } = 7.00m;

    /// <summary>ILS per kWh.</summary>
    public decimal ElectricityPerKwh { get; set; } = 0.64m;
}

public class VehicleEstimatedConsumption
{
    public decimal GasolineKmPerLiter { get; set; } = 15.0m;

    public decimal DieselKmPerLiter { get; set; } = 14.0m;

    public decimal ElectricKwhPer100Km { get; set; } = 18.0m;
}
