using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// Estimated monthly energy cost for a vehicle model. Built from configurable defaults,
/// never from WLTP data, and always flagged as an estimate.
/// </summary>
public class VehicleEnergyCost
{
    public VehicleEnergyType EnergyType { get; set; }

    public decimal KmPerMonth { get; set; }

    /// <summary>km per liter for fuel vehicles, kWh per 100 km for electric; null when unknown.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public decimal? Consumption { get; set; }

    public string ConsumptionUnit { get; set; } = string.Empty;

    public decimal EnergyPrice { get; set; }

    public string EnergyPriceUnit { get; set; } = string.Empty;

    /// <summary>ILS per month; null when the energy type cannot be calculated.</summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public decimal? MonthlyCost { get; set; }

    public bool IsEstimated { get; set; } = true;
}
