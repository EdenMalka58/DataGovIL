using System.Globalization;
using DataGovIL.Api.Models;

namespace DataGovIL.Api.Services;

/// <summary>
/// Port of the Delphi <c>TCardData.CalcAutoFactorsValues</c> value factors
/// (kilometers, owner count, previous owner type). Values are computed from the list price
/// unless <c>cumulative</c> is set, in which case each factor applies to the running value.
/// </summary>
public static class VehicleDepreciationCalculator
{
    private const double MaxCarAge = 30;

    // [vehicle category, owner category] -> percent value loss.
    private static readonly byte[,] AddValLoss =
    {
        { 0, 5, 10, 10, 10, 10, 20, 20 },
        { 0, 0, 10, 10, 10, 10, 20, 20 },
        { 0, 0, 10, 10, 10, 10, 20, 20 },
        { 0, 0,  0,  0,  0,  0,  0,  0 },
        { 0, 0,  0,  0,  0,  0,  0,  0 }
    };

    private const decimal Commercial4tMaxWeightKg = 4000;

    /// <summary>Returns null when the car age cannot be derived from the road entry date.</summary>
    public static VehicleDepreciationRecord? Calculate(
        VehicleRecord vehicle,
        VehicleHistoryRecord? history,
        VehiclePriceListRecord? priceList,
        DateTime valuationDate,
        bool cumulative = false)
    {
        var entryDate = ParseYearMonth(vehicle.RoadEntryDate)
            ?? ParseYearMonth(history?.Technical?.FirstRegistrationDate);
        if (entryDate is null)
            return null;

        var carAge = GetCarAge(valuationDate, entryDate.Value);
        var kilometers = ParseLong(history?.Technical?.LastTestOdometer);
        var ownerCount = history?.OwnershipHistory.Count ?? 0;
        var originality = history?.Technical?.OriginalityName?.Trim();
        var vehicleCategory = ResolveVehicleCategory(vehicle, originality);
        var (ownershipType, ownerCategory) = ResolveOwnerCategory(vehicle, history, originality);
        var listPrice = ParseDecimal(priceList?.Price);

        var result = new VehicleDepreciationRecord
        {
            CarAge = Math.Round(carAge, 2),
            Kilometers = kilometers,
            OwnerCount = ownerCount,
            Originality = string.IsNullOrEmpty(originality) ? null : originality,
            OwnershipType = ownershipType,
            VehicleCategory = vehicleCategory,
            OwnerCategory = ownerCategory,
            ListPrice = listPrice
        };

        var actualValue = listPrice;
        void AddLine(DepreciationFactor factor, string description, double diff)
        {
            var hundredths = (int)Math.Round(diff * 100);
            decimal? value = null;
            if (listPrice is decimal list && actualValue is decimal actual)
            {
                var basis = cumulative ? actual : list;
                value = Math.Round(basis * hundredths / 10000m, 0, MidpointRounding.AwayFromZero);
                actualValue = actual + value.Value;
            }

            result.Lines.Add(new VehicleDepreciationLine
            {
                Factor = factor,
                Description = description,
                Percent = hundredths / 100m,
                Value = value
            });
        }

        AddLine(DepreciationFactor.Kilometers, "מספר קילומטרים",
            GetKmDiff(carAge, kilometers ?? 0, vehicleCategory));
        AddLine(DepreciationFactor.OwnerCount, "מספר בעלים קודמים",
            GetOwnerDiff(carAge, ownerCount, vehicleCategory));
        AddLine(DepreciationFactor.OwnershipType, "בעלות קודמת או נוכחית",
            GetPrevOwnerDiff(carAge, vehicleCategory, ownerCategory));

        result.DepreciationPercent = result.Lines.Sum(l => l.Percent);
        if (listPrice is not null)
        {
            result.DepreciationValue = result.Lines.Sum(l => l.Value ?? 0);
            result.EstimatedValue = actualValue;
        }

        return result;
    }

    /// <summary>Delphi <c>GetCarAge</c>: whole months between the two dates, in years.</summary>
    private static double GetCarAge(DateTime valuationDate, DateTime entryDate)
        => (valuationDate.Month - (double)entryDate.Month) / 12 + valuationDate.Year - entryDate.Year;

    private static double GetKmDiff(double carAge, long kilometers, DepreciationVehicleCategory category)
    {
        if (carAge <= 0 || carAge >= MaxCarAge || kilometers <= 0)
            return 0;

        long step = 3000;
        var year = 1;
        while (year < carAge)
        {
            step = (long)Math.Round(step * 1.2);
            year++;
        }

        var km = category switch
        {
            DepreciationVehicleCategory.Private => kilometers,
            DepreciationVehicleCategory.Commercial4t => kilometers / 2,
            _ => kilometers / 4
        };

        double diff = 0;
        if (km > carAge * 20000)
            diff = Math.Round(10 * (carAge * 20000 - km) / step) / 10;
        else if (km < carAge * 15000)
            diff = Math.Round(10 * (carAge * 15000 - km) / step) / 10;

        return Math.Clamp(diff, -20, 20);
    }

    private static double GetOwnerDiff(double carAge, int ownerCount, DepreciationVehicleCategory category)
    {
        if (carAge <= 0 || carAge >= MaxCarAge || ownerCount <= 0 || category == DepreciationVehicleCategory.Taxi)
            return 0;

        var owners = Math.Min(30, ownerCount);
        var diff = carAge / 2 - owners + 1;
        if (diff > 0)
            diff = Math.Max(0, carAge / 3 - owners);

        diff = Math.Clamp(diff * 4, -15, 15);
        return Math.Round(diff * 10) / 10;
    }

    private static double GetPrevOwnerDiff(
        double carAge, DepreciationVehicleCategory vehicleCategory, DepreciationOwnerCategory ownerCategory)
    {
        if (carAge <= 0 || carAge >= MaxCarAge)
            return 0;

        return -AddValLoss[(int)vehicleCategory, (int)ownerCategory];
    }

    /// <summary>
    /// Taxis and minibuses come from the originality (<c>mkoriut_nm</c> מונית / סיור ותיור) or the
    /// vehicle type name. <c>sug_degem</c> "M" is commercial (split by total weight at 4 tons).
    /// Everything else is private.
    /// </summary>
    private static DepreciationVehicleCategory ResolveVehicleCategory(VehicleRecord vehicle, string? originality)
    {
        originality ??= string.Empty;
        var typeName = vehicle.VehicleTypeName ?? string.Empty;
        if (originality.Contains("מונית") || typeName.Contains("מונית"))
            return DepreciationVehicleCategory.Taxi;
        if (originality.Contains("סיור") || typeName.Contains("זעיר"))
            return DepreciationVehicleCategory.Minibus;

        if (!string.Equals(vehicle.ModelType?.Trim(), "M", StringComparison.OrdinalIgnoreCase))
            return DepreciationVehicleCategory.Private;

        var weight = ParseDecimal(vehicle.TotalWeight) ?? ParseDecimal(vehicle.ManufacturerModel?.TotalWeight);
        return weight > Commercial4tMaxWeightKg
            ? DepreciationVehicleCategory.Commercial
            : DepreciationVehicleCategory.Commercial4t;
    }

    /// <summary>
    /// Picks the highest-loss ownership type (enum order) across the current owner, the
    /// ownership history, and the originality. Dealer (סוחר), import, diplomatic, and other
    /// unknown values are ignored.
    /// </summary>
    private static (string? Name, DepreciationOwnerCategory Category) ResolveOwnerCategory(
        VehicleRecord vehicle, VehicleHistoryRecord? history, string? originality)
    {
        var names = new List<string?> { vehicle.OwnershipType };
        if (history is not null)
            names.AddRange(history.OwnershipHistory.Select(o => o.OwnershipType));
        names.Add(originality);

        string? bestName = null;
        var best = DepreciationOwnerCategory.Private;
        foreach (var raw in names)
        {
            var name = raw?.Trim();
            if (string.IsNullOrEmpty(name) || MapOwnerCategory(name) is not DepreciationOwnerCategory category)
                continue;

            if (bestName is null || category > best)
            {
                best = category;
                bestName = name;
            }
        }

        return (bestName, best);
    }

    private static DepreciationOwnerCategory? MapOwnerCategory(string name)
    {
        if (name.Contains("פרטי")) return DepreciationOwnerCategory.Private;
        if (name.Contains("חברה")) return DepreciationOwnerCategory.Commercial;
        if (name.Contains("השכרה") || name.Contains("החכר") || name.Contains("ליסינג"))
            return DepreciationOwnerCategory.Rental;
        if (name.Contains("מונית")) return DepreciationOwnerCategory.Taxi;
        if (name.Contains("לימוד") || name.Contains("נהיגה")) return DepreciationOwnerCategory.DriveTech;
        if (name.Contains("ממשל") || name.Contains("מדינת ישראל")) return DepreciationOwnerCategory.Government;
        if (name.Contains("קיבוץ")) return DepreciationOwnerCategory.Kibbutz;
        if (name.Contains("ציבורי") || name.Contains("סיור")) return DepreciationOwnerCategory.Public;
        return null;
    }

    /// <summary>Accepts <c>yyyy-M</c>, <c>yyyy-MM-dd…</c>, <c>yyyyMM</c>, and <c>dd/MM/yyyy</c>.</summary>
    internal static DateTime? ParseYearMonth(string? raw)
    {
        var value = raw?.Trim();
        if (string.IsNullOrEmpty(value))
            return null;

        string[] formats = ["yyyy-M", "yyyy-MM", "yyyy-M-d", "yyyy-MM-dd", "yyyyMM", "d/M/yyyy", "d.M.yyyy"];
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
            return exact;

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private static long? ParseLong(string? raw)
        => ParseDecimal(raw) is decimal d && d > 0 ? (long)decimal.Truncate(d) : null;

    private static decimal? ParseDecimal(string? raw)
        => decimal.TryParse(raw?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d : null;
}
