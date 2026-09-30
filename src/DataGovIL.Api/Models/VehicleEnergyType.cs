using System.Text.Json.Serialization;

namespace DataGovIL.Api.Models;

/// <summary>
/// Energy source used for the monthly cost estimate. Plug-in hybrids are reported as
/// <see cref="Unknown"/> until separate fuel and electricity consumption data is available.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum VehicleEnergyType
{
    Gasoline,
    Diesel,
    Electric,
    Unknown
}
