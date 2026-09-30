# License Fee Calculation – Cursor Implementation Instructions

## Goal

Add a local in-memory C# table for Israeli vehicle annual license fees and a calculator that determines the applicable fee from:

- `kvuzat_agra_cd` – vehicle license-fee group code from the WLTP vehicle database.
- First registration date of the vehicle.
- Fee year.

The implementation must be designed so that annual Ministry of Transport fee updates can be added without changing the calculation logic.

> **Important:** Before wiring `kvuzat_agra_cd` directly to groups `1–7`, verify in the current Ministry of Transport WLTP dataset that the actual values represent the seven license-fee groups. Do not silently assume that an internal code is identical to the displayed fee group.

---

## 1. Create the fee model

Create:

```csharp
public sealed record LicenseFeeEntry(
    int FeeYear,
    int AgraGroup,
    int VehicleAgeGroup,
    decimal Fee);
```

If the project already has an appropriate model/value-object convention, follow the existing project conventions instead of introducing unnecessary new abstractions.

---

## 2. Create `LicenseFeeTable`

Create a local in-memory table, preferably as:

```text
LicenseFeeTable.cs
```

Use a dictionary keyed by:

```csharp
(FeeYear, AgraGroup, VehicleAgeGroup)
```

Do NOT hard-code the year into the calculator.

For the 2026 fee schedule, use the Ministry of Transport values that were verified for the implementation.

The table structure should be:

```csharp
private static readonly Dictionary<
    (int FeeYear, int AgraGroup, int VehicleAgeGroup),
    decimal> Fees
```

Populate the 2026 values according to the verified official fee table.

Keep all fee values in one place so that the 2027 table can later be added without modifying the calculation algorithm.

---

## 3. Vehicle age/registration groups

For the 2026 table, the calculator needs to distinguish the applicable vehicle-age category according to the official fee table.

Do NOT determine the first category from `shnat_yitzur` alone.

The first category is based on the vehicle's **first registration date**, therefore the calculator should receive:

```csharp
DateTime firstRegistrationDate
```

rather than relying only on production year.

Create a private helper:

```csharp
private static int GetVehicleAgeGroup(
    DateTime firstRegistrationDate,
    int feeYear)
```

The helper must implement the exact year ranges from the official fee table.

Keep this logic isolated from the fee lookup itself.

---

## 4. Create the public calculator method

Add:

```csharp
public static decimal? GetLicenseFee(
    int kvuzatAgraCd,
    DateTime firstRegistrationDate,
    int feeYear)
```

Expected behavior:

1. Validate `kvuzatAgraCd`.
2. Validate `firstRegistrationDate`.
3. Determine the vehicle age group.
4. Look up:

```text
FeeYear + AgraGroup + VehicleAgeGroup
```

5. Return the fee when found.
6. Return `null` when the input is invalid or no matching fee exists.

Do not throw exceptions for normal missing-data cases.

---

## 5. Recommended implementation

Use this structure:

```csharp
public static decimal? GetLicenseFee(
    int kvuzatAgraCd,
    DateTime firstRegistrationDate,
    int feeYear)
{
    if (kvuzatAgraCd <= 0)
        return null;

    if (firstRegistrationDate == default)
        return null;

    int vehicleAgeGroup =
        GetVehicleAgeGroup(firstRegistrationDate, feeYear);

    return Fees.TryGetValue(
        (feeYear, kvuzatAgraCd, vehicleAgeGroup),
        out decimal fee)
        ? fee
        : null;
}
```

The exact age-group logic must come from the verified official 2026 fee schedule.

---

## 6. Do not use the vehicle production year as a replacement

The vehicle database may contain:

```text
shnat_yitzur
```

This is useful vehicle information, but it must not automatically be treated as the first-registration year for license-fee calculation.

Prefer an existing field in the application containing the vehicle's actual first registration date.

If the application currently has only `shnat_yitzur` and no first-registration date, do not silently substitute it. Instead:

- identify the existing registration-date field/API;
- or return `null` when the required data is unavailable.

---

## 7. Integration with the vehicle details calculation

Find the existing vehicle details / annual-cost calculation code.

Add the license-fee calculation as a separate component:

```csharp
decimal? annualLicenseFee =
    LicenseFeeTable.GetLicenseFee(
        vehicle.KvuzatAgraCd,
        vehicle.FirstRegistrationDate,
        feeYear);
```

Do not mix license-fee logic with:

- fuel consumption calculations;
- WLTP calculations;
- energy-cost calculations;
- purchase price;
- usage value calculations.

Each calculation should remain independent.

---

## 8. Missing or invalid data

The calculation must safely handle:

- `kvuzat_agra_cd` missing.
- `kvuzat_agra_cd` invalid.
- First registration date missing.
- Unsupported fee year.
- Unsupported agra group.
- No matching entry in the local table.

In all these cases, return:

```csharp
null
```

Do not return `0`, because `0` could incorrectly mean that the vehicle has no license fee.

---

## 9. Future fee-year support

The calculator must NOT contain logic such as:

```csharp
if (feeYear == 2026)
```

The fee year must be a lookup key.

For example, when the 2027 official table becomes available, add:

```text
2027 + group + age group + fee
```

to the local table.

The calculator itself should remain unchanged.

---

## 10. Formatting

The calculation layer should return a numeric value:

```csharp
decimal?
```

Do not return:

```text
"1,698 ₪"
```

from the calculation method.

Formatting with currency symbols and locale-specific separators belongs to the presentation/UI layer.

---

## 11. Tests

Add unit tests covering at least:

### Valid cases

- Each of the seven agra groups.
- A vehicle in each applicable age category.
- A valid 2026 lookup.
- A missing fee-year entry.

### Invalid cases

```text
kvuzatAgraCd = 0
kvuzatAgraCd < 0
missing registration date
unsupported fee year
```

Expected result:

```csharp
null
```

### Important regression test

Verify that changing:

```text
feeYear = 2026
```

to another year does not cause the calculator to use 2026 values.

---

## 12. Do not confuse license fee with usage value

This calculation is only for:

**Annual vehicle license fee / אגרת רישוי**

It is NOT:

- שווי שימוש;
- purchase tax;
- vehicle price;
- fuel tax;
- electricity cost;
- WLTP energy cost.

Keep the implementation completely separate from the existing energy-cost calculation.

---

## 13. Verification of `kvuzat_agra_cd`

Before finalizing the implementation, inspect the actual current WLTP data.

Find several known vehicles and compare:

```text
kvuzat_agra_cd
```

with the official license-fee group.

Document the finding in code comments or implementation notes.

If the WLTP field uses an internal code rather than values `1–7`, add an explicit mapping:

```csharp
private static readonly Dictionary<int, int> AgraGroupMapping
```

Do not reinterpret the value implicitly.

---

## 14. Code quality requirements

Follow the existing project's conventions for:

- namespaces;
- folder structure;
- nullable reference types;
- dependency injection;
- static helper classes;
- naming;
- logging;
- unit-test framework.

Do not introduce a new framework or external NuGet package for this feature.

Keep the implementation small and self-contained.

---

## 15. Expected result

The final code should provide a simple API such as:

```csharp
decimal? fee = LicenseFeeTable.GetLicenseFee(
    vehicle.KvuzatAgraCd,
    vehicle.FirstRegistrationDate,
    2026);
```

Example:

```text
Input:
    kvuzat_agra_cd = 3
    firstRegistrationDate = 2025-06-15
    feeYear = 2026

Output:
    1698
```

The UI can then display:

```text
1,698 ₪
```

---

## Important implementation note

Do not invent or infer official fee values.

The 2026 values must come from the current official Ministry of Transport license-fee table. If the project's existing data differs from the official table, stop and report the discrepancy rather than silently changing the values.

The implementation should make the annual fee table easy to replace/update when the Ministry publishes a new fee schedule.




code of LicenseFeeTable.cs:
public static class LicenseFeeTable
{
    // Ministry of Transport license fees
    // Effective from 01.04.2026
    //
    // VehicleAgeGroup:
    // 1 = first registered in 2026
    // 2 = 2024-2026
    // 3 = 2021-2023
    // 4 = 2017-2020
    // 5 = 2016 and older

    private static readonly Dictionary<(int AgraGroup, int VehicleAgeGroup), decimal> Fees = new()
    {
        // Group 1
        [(1, 1)] = 1266m,
        [(1, 2)] = 1109m,
        [(1, 3)] =  972m,
        [(1, 4)] =  849m,
        [(1, 5)] =  849m,

        // Group 2
        [(2, 1)] = 1610m,
        [(2, 2)] = 1404m,
        [(2, 3)] = 1230m,
        [(2, 4)] = 1076m,
        [(2, 5)] = 1076m,

        // Group 3
        [(3, 1)] = 1941m,
        [(3, 2)] = 1698m,
        [(3, 3)] = 1487m,
        [(3, 4)] = 1297m,
        [(3, 5)] = 1297m,

        // Group 4
        [(4, 1)] = 2315m,
        [(4, 2)] = 1968m,
        [(4, 3)] = 1674m,
        [(4, 4)] = 1422m,
        [(4, 5)] = 1422m,

        // Group 5
        [(5, 1)] = 2651m,
        [(5, 2)] = 2184m,
        [(5, 3)] = 1802m,
        [(5, 4)] = 1490m,
        [(5, 5)] = 1490m,

        // Group 6
        [(6, 1)] = 3764m,
        [(6, 2)] = 2823m,
        [(6, 3)] = 2117m,
        [(6, 4)] = 1585m,
        [(6, 5)] = 1585m,

        // Group 7
        [(7, 1)] = 5364m,
        [(7, 2)] = 3753m,
        [(7, 3)] = 2627m,
        [(7, 4)] = 1840m,
        [(7, 5)] = 1840m
    };

    public static decimal? GetFee(
        int agraGroup,
        int vehicleYear,
        int feeYear = 2026)
    {
        if (agraGroup < 1 || agraGroup > 7)
            return null;

        if (vehicleYear <= 0)
            return null;

        int ageGroup = GetVehicleAgeGroup(vehicleYear, feeYear);

        return Fees.TryGetValue((agraGroup, ageGroup), out var fee)
            ? fee
            : null;
    }

    private static int GetVehicleAgeGroup(
        int vehicleYear,
        int feeYear)
    {
        if (vehicleYear >= feeYear)
            return 1;

        if (vehicleYear >= feeYear - 2)
            return 2;

        if (vehicleYear >= feeYear - 5)
            return 3;

        if (vehicleYear >= feeYear - 9)
            return 4;

        return 5;
    }
}

code of GetLicenseFee:

public static decimal? GetLicenseFee(
    int kvuzatAgraCd,
    DateTime firstRegistrationDate,
    int feeYear = 2026)
{
    if (kvuzatAgraCd < 1 || kvuzatAgraCd > 7)
        return null;

    if (firstRegistrationDate == default)
        return null;

    int registrationYear = firstRegistrationDate.Year;

    int ageGroup;

    if (registrationYear >= feeYear)
    {
        // Vehicle first registered from 01.01.2026
        ageGroup = 1;
    }
    else if (registrationYear >= feeYear - 2)
    {
        // 2024-2025
        ageGroup = 2;
    }
    else if (registrationYear >= feeYear - 5)
    {
        // 2021-2023
        ageGroup = 3;
    }
    else if (registrationYear >= feeYear - 9)
    {
        // 2017-2020
        ageGroup = 4;
    }
    else
    {
        // 2016 and older
        ageGroup = 5;
    }

    return Fees.TryGetValue(
        (kvuzatAgraCd, ageGroup),
        out var fee)
        ? fee
        : null;
}

decimal? licenseFee = LicenseFeeTable.GetLicenseFee(
    kvuzatAgraCd: 3,
    firstRegistrationDate: new DateTime(2025, 6, 15),
    feeYear: 2026);