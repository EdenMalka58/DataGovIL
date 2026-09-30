namespace DataGovIL.Api.Services;

/// <summary>
/// Annual vehicle license fee (אגרת רישוי, ₪) by fee year, license-fee group (<c>kvuzat_agra_cd</c>),
/// and vehicle age group derived from the first registration date.
/// </summary>
/// <remarks>
/// WLTP <c>kvuzat_agra_cd</c> holds exactly the values 1–7 (checked against the live data.gov.il
/// resource), matching the seven Ministry of Transport fee groups, so it is used as the group directly.
/// To add a new fee schedule, add its rows under the new fee year; the lookup does not change.
/// </remarks>
public static class LicenseFeeTable
{
    // 2026: Ministry of Transport license fees, effective from 01.04.2026.
    // Vehicle age group for fee year 2026 by first registration year:
    //   1 = 2026, 2 = 2024–2025, 3 = 2021–2023, 4 = 2017–2020, 5 = 2016 and older.
    private static readonly Dictionary<(int FeeYear, int AgraGroup, int VehicleAgeGroup), decimal> Fees = new()
    {
        [(2026, 1, 1)] = 1266m,
        [(2026, 1, 2)] = 1109m,
        [(2026, 1, 3)] = 972m,
        [(2026, 1, 4)] = 849m,
        [(2026, 1, 5)] = 849m,

        [(2026, 2, 1)] = 1610m,
        [(2026, 2, 2)] = 1404m,
        [(2026, 2, 3)] = 1230m,
        [(2026, 2, 4)] = 1076m,
        [(2026, 2, 5)] = 1076m,

        [(2026, 3, 1)] = 1941m,
        [(2026, 3, 2)] = 1698m,
        [(2026, 3, 3)] = 1487m,
        [(2026, 3, 4)] = 1297m,
        [(2026, 3, 5)] = 1297m,

        [(2026, 4, 1)] = 2315m,
        [(2026, 4, 2)] = 1968m,
        [(2026, 4, 3)] = 1674m,
        [(2026, 4, 4)] = 1422m,
        [(2026, 4, 5)] = 1422m,

        [(2026, 5, 1)] = 2651m,
        [(2026, 5, 2)] = 2184m,
        [(2026, 5, 3)] = 1802m,
        [(2026, 5, 4)] = 1490m,
        [(2026, 5, 5)] = 1490m,

        [(2026, 6, 1)] = 3764m,
        [(2026, 6, 2)] = 2823m,
        [(2026, 6, 3)] = 2117m,
        [(2026, 6, 4)] = 1585m,
        [(2026, 6, 5)] = 1585m,

        [(2026, 7, 1)] = 5364m,
        [(2026, 7, 2)] = 3753m,
        [(2026, 7, 3)] = 2627m,
        [(2026, 7, 4)] = 1840m,
        [(2026, 7, 5)] = 1840m,
    };

    /// <summary>
    /// Returns null when the group or date is missing/invalid, or the table has no row for the
    /// fee year, group, and age group.
    /// </summary>
    public static decimal? GetLicenseFee(int kvuzatAgraCd, DateTime firstRegistrationDate, int feeYear)
    {
        if (kvuzatAgraCd <= 0)
            return null;

        if (firstRegistrationDate == default)
            return null;

        var vehicleAgeGroup = GetVehicleAgeGroup(firstRegistrationDate, feeYear);

        return Fees.TryGetValue((feeYear, kvuzatAgraCd, vehicleAgeGroup), out var fee)
            ? fee
            : null;
    }

    private static int GetVehicleAgeGroup(DateTime firstRegistrationDate, int feeYear)
    {
        var registrationYear = firstRegistrationDate.Year;

        if (registrationYear >= feeYear)
            return 1;

        if (registrationYear >= feeYear - 2)
            return 2;

        if (registrationYear >= feeYear - 5)
            return 3;

        if (registrationYear >= feeYear - 9)
            return 4;

        return 5;
    }
}
