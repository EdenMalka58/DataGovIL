using System.Globalization;

namespace DataGovIL.Api.Services;

/// <summary>
/// Estimated monthly usage value (שווי שימוש חודשי, ₪) by the catalog fee group (קבוצת אגרה).
/// </summary>
public static class FeeGroupUsageValue
{
    private static readonly Dictionary<int, decimal> UsageValueByGroup = new()
    {
        [1] = 2800,
        [2] = 3100,
        [3] = 3500,
        [4] = 3900,
        [5] = 4300,
        [6] = 4700,
        [7] = 5100,
    };

    /// <summary>Returns null when the code is missing, not a whole number, or not in the table.</summary>
    public static decimal? ForFeeGroup(string? feeGroupCode)
    {
        if (!decimal.TryParse(feeGroupCode, NumberStyles.Number, CultureInfo.InvariantCulture, out var code)
            || code != decimal.Truncate(code))
            return null;
        return UsageValueByGroup.TryGetValue((int)code, out var value) ? value : null;
    }
}
