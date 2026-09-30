using System.Globalization;
using DataGovIL.Api.Models;

namespace DataGovIL.Api.Services;

public interface IVehicleEnergyCostService
{
    VehicleEnergyType GetVehicleEnergyType(ManufacturerModelRecord model);

    VehicleEnergyCost CalculateCost(ManufacturerModelRecord model, decimal? customKmPerMonth = null);
}

/// <summary>
/// Estimated monthly energy cost from configurable price and consumption defaults.
/// Never derived from WLTP CO2 values or tax groups.
/// </summary>
public class VehicleEnergyCostService : IVehicleEnergyCostService
{
    // WLTP delek_cd values.
    private const int FuelGasoline = 1;
    private const int FuelDiesel = 2;
    private const int FuelElectric = 4;
    private const int FuelElectricGasoline = 7;
    private const int FuelElectricDiesel = 8;

    // WLTP technologiat_hanaa_cd values.
    private const int DrivePlugIn = 2;
    private const int DriveElectric = 3;

    private readonly IEnergyPriceProvider _prices;

    public VehicleEnergyCostService(IEnergyPriceProvider prices)
    {
        _prices = prices;
    }

    public VehicleEnergyType GetVehicleEnergyType(ManufacturerModelRecord model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return ResolveEnergyType(model.FuelCode, model.FuelName, model.DriveTechnologyCode, model.DriveTechnologyName);
    }

    public VehicleEnergyCost CalculateCost(ManufacturerModelRecord model, decimal? customKmPerMonth = null)
    {
        var energyType = GetVehicleEnergyType(model);
        var km = Math.Max(customKmPerMonth ?? _prices.DefaultKmPerMonth, 0m);

        var consumption = _prices.GetDefaultConsumption(energyType);
        var price = _prices.GetPrice(energyType);

        if (energyType == VehicleEnergyType.Unknown || consumption is null || price is null)
        {
            return new VehicleEnergyCost
            {
                EnergyType = VehicleEnergyType.Unknown,
                KmPerMonth = km,
            };
        }

        return new VehicleEnergyCost
        {
            EnergyType = energyType,
            KmPerMonth = km,
            Consumption = consumption,
            ConsumptionUnit = energyType == VehicleEnergyType.Electric ? "kWh/100km" : "km/l",
            EnergyPrice = price.Value,
            EnergyPriceUnit = energyType == VehicleEnergyType.Electric ? "ILS/kWh" : "ILS/l",
            MonthlyCost = Monthly(energyType, km, consumption.Value, price.Value),
        };
    }

    private static decimal? Monthly(VehicleEnergyType energyType, decimal km, decimal consumption, decimal price)
    {
        if (consumption <= 0m)
            return null;

        var cost = energyType == VehicleEnergyType.Electric
            ? km / 100m * consumption * price
            : km / consumption * price;

        return Math.Round(cost, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Codes win over names; names are matched in Hebrew and English. Plug-in hybrids are checked
    /// first (their names contain both fuel words) and stay <see cref="VehicleEnergyType.Unknown"/>
    /// until per-mode metrics (electric kWh/100km, fuel km/l, electric and fuel km share) exist.
    /// </summary>
    private static VehicleEnergyType ResolveEnergyType(
        string? fuelCode, string? fuelName, string? driveCode, string? driveName)
    {
        var fuel = ParseCode(fuelCode);
        var drive = ParseCode(driveCode);
        var fuelText = Normalize(fuelName);
        var driveText = Normalize(driveName);

        if (fuel is FuelElectricGasoline or FuelElectricDiesel || drive == DrivePlugIn
            || IsPlugInName(fuelText) || IsPlugInName(driveText))
            return VehicleEnergyType.Unknown;

        if (fuel == FuelElectric || drive == DriveElectric)
            return VehicleEnergyType.Electric;
        if (fuel == FuelGasoline)
            return VehicleEnergyType.Gasoline;
        if (fuel == FuelDiesel)
            return VehicleEnergyType.Diesel;

        if (fuel is null)
        {
            if (ContainsAny(fuelText, "חשמל", "electric") || ContainsAny(driveText, "חשמלי", "electric"))
                return VehicleEnergyType.Electric;
            if (ContainsAny(fuelText, "בנזין", "gasoline", "petrol"))
                return VehicleEnergyType.Gasoline;
            if (ContainsAny(fuelText, "דיזל", "סולר", "diesel"))
                return VehicleEnergyType.Diesel;
        }

        return VehicleEnergyType.Unknown;
    }

    private static bool IsPlugInName(string text)
        => text.Contains('/') || ContainsAny(text, "plug", "phev", "נטען");

    private static bool ContainsAny(string text, params string[] needles)
        => needles.Any(n => text.Contains(n, StringComparison.OrdinalIgnoreCase));

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;

    private static int? ParseCode(string? value)
    {
        if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var code)
            || code != decimal.Truncate(code))
            return null;
        return (int)code;
    }
}
