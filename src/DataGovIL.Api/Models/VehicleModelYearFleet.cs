namespace DataGovIL.Api.Models;

/// <summary>
/// Active and inactive vehicles of one model code in one manufacture year.
/// </summary>
public class VehicleModelYearFleet
{
    public int ModelYear { get; set; }

    /// <summary>mispar_rechavim_pailim</summary>
    public int ActiveCount { get; set; }

    /// <summary>mispar_rechavim_le_pailim</summary>
    public int InactiveCount { get; set; }

    /// <summary>Active plus inactive: vehicles of this model year that were registered.</summary>
    public int RegisteredCount => ActiveCount + InactiveCount;

    /// <summary>Active share of the registered total, in percent.</summary>
    public decimal? ActiveSharePercent =>
        RegisteredCount == 0
            ? null
            : Math.Round(ActiveCount * 100m / RegisteredCount, 1, MidpointRounding.AwayFromZero);
}

/// <summary>
/// Every manufacture year of one model code, plus the sum across those years.
/// </summary>
public class VehicleModelFleet
{
    public List<VehicleModelYearFleet> Years { get; set; } = new();

    public int ActiveCount => Years.Sum(year => year.ActiveCount);

    public int InactiveCount => Years.Sum(year => year.InactiveCount);

    public int RegisteredCount => ActiveCount + InactiveCount;

    public decimal? ActiveSharePercent =>
        RegisteredCount == 0
            ? null
            : Math.Round(ActiveCount * 100m / RegisteredCount, 1, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Groups datastore rows by manufacture year. Several rows can share a year
    /// when the commercial name differs; their counts are added together.
    /// </summary>
    public static VehicleModelFleet? FromRows(IEnumerable<VehicleModelCountDatastoreRecord> rows)
    {
        var byYear = new Dictionary<int, (int Active, int Inactive)>();

        foreach (var row in rows)
        {
            if (!int.TryParse(row.ModelYear, out var year))
                continue;

            var hasActive = int.TryParse(row.ActiveVehicleCount, out var activeCount);
            var hasInactive = int.TryParse(row.InactiveVehicleCount, out var inactiveCount);
            if (!hasActive && !hasInactive)
                continue;

            var current = byYear.GetValueOrDefault(year);
            byYear[year] = (current.Active + activeCount, current.Inactive + inactiveCount);
        }

        var years = byYear
            .Select(entry => new VehicleModelYearFleet
            {
                ModelYear = entry.Key,
                ActiveCount = entry.Value.Active,
                InactiveCount = entry.Value.Inactive
            })
            .Where(year => year.RegisteredCount > 0)
            .OrderBy(year => year.ModelYear)
            .ToList();

        return years.Count == 0 ? null : new VehicleModelFleet { Years = years };
    }
}
