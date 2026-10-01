using System.Globalization;

namespace DataGovIL.Api.Models;

/// <summary>New registrations of one model code, summed across every closure month in the dataset.</summary>
public class VehicleModelPopularity
{
    /// <summary>סה״כ רכבים חדשים מאותו קוד דגם, מכל החודשים.</summary>
    public int TotalCount { get; set; }

    /// <summary>כמות לפי חודש סגירה (<c>yyyyMM</c>), מהישן לחדש. חודשים בלי סגירה בין הראשון לאחרון נשמרים עם כמות 0.</summary>
    public List<VehicleModelMonthlyCount> Months { get; set; } = new();

    /// <summary>
    /// Sums <c>car_num</c> by <c>sgira_month</c>. Returns null when no row has a usable month and count.
    /// </summary>
    public static VehicleModelPopularity? FromRows(IEnumerable<NewVehicleMonthlyCountDatastoreRecord> rows)
    {
        var byMonth = new Dictionary<int, int>();
        foreach (var row in rows)
        {
            if (!TryParseMonth(row.ClosureMonth, out var month))
                continue;
            if (!TryParseCount(row.VehicleCount, out var count))
                continue;

            byMonth[month] = byMonth.GetValueOrDefault(month) + count;
        }

        if (byMonth.Count == 0)
            return null;

        var first = byMonth.Keys.Min();
        var last = byMonth.Keys.Max();
        var months = new List<VehicleModelMonthlyCount>();
        for (var cursor = first; cursor <= last && months.Count < 600; cursor = NextMonth(cursor))
        {
            months.Add(new VehicleModelMonthlyCount
            {
                Month = cursor,
                Count = byMonth.GetValueOrDefault(cursor)
            });
        }

        return new VehicleModelPopularity
        {
            TotalCount = byMonth.Values.Sum(),
            Months = months
        };
    }

    private static bool TryParseMonth(string? raw, out int yearMonth)
    {
        yearMonth = 0;
        if (!TryParseCount(raw, out var parsed))
            return false;

        var month = parsed % 100;
        var year = parsed / 100;
        if (year is < 1900 or > 2200 || month is < 1 or > 12)
            return false;

        yearMonth = parsed;
        return true;
    }

    private static bool TryParseCount(string? raw, out int value)
    {
        value = 0;
        if (!decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            || parsed != decimal.Truncate(parsed)
            || parsed is < 0 or > int.MaxValue)
        {
            return false;
        }

        value = (int)parsed;
        return true;
    }

    private static int NextMonth(int yearMonth)
    {
        var year = yearMonth / 100;
        var month = yearMonth % 100;
        return month >= 12 ? (year + 1) * 100 + 1 : year * 100 + month + 1;
    }
}

/// <summary>New registrations of one model code in a single closure month.</summary>
public class VehicleModelMonthlyCount
{
    /// <summary>חודש סגירה, <c>yyyyMM</c>.</summary>
    public int Month { get; set; }

    /// <summary>כמות רכבים חדשים באותו חודש.</summary>
    public int Count { get; set; }
}
