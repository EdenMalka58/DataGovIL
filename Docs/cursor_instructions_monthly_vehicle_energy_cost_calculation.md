# Cursor Instructions: Monthly Vehicle Energy Cost Calculation

## Objective

Add an estimated monthly vehicle energy cost calculation to the system and display it on the Vehicle Details screen **directly above the existing WLTP data section**.

The calculated cost is strictly an estimate based on configurable defaults and must **never** be presented as an official figure from the Ministry of Transport or the WLTP database.

---

## 1. Core Principles & Constraints

1. **Supported Energy Types**:
   * **Gasoline**
   * **Diesel**
   * **Electric**
   * (*PHEV / Plug-in Hybrid*: Explicitly unsupported in Phase 1; handle as `Unknown` or non-calculable until distinct fuel and electricity consumption data is integrated).

2. **WLTP Database Integrity**:
   * **DO NOT** attempt to calculate or extrapolate fuel consumption from `CO2_WLTP`.
   * **DO NOT** modify the existing `WLTP` database schema or append fictitious fields (e.g., `fuelConsumption`, `kmPerLiter`, `kwhPer100Km`) directly to the core `Vehicle` or `WLTP` entity.
   * **DO NOT** use vehicle tax group codes (`kvuzat_agra_cd`) to estimate fuel consumption.
   * Keep the feature strictly decoupled as an isolated layer:

```
Vehicle
  ├── Basic Data
  ├── Energy Cost (NEW)
  └── WLTP Data
```

3. **Disclaimer & Official Data Constraints**:
   * Never refer to default energy prices as "Government Official Prices."
   * Always explicitly flag API responses with `"isEstimated": true`.

---

## 2. Configuration Setup

Add central configuration settings to `appsettings.json` (or equivalent configuration provider). All price and consumption defaults must be dynamically configurable without requiring application re-compilation.

```json
{
  "VehicleCost": {
    "KmPerMonth": 1500,
    "FuelPrices": {
      "Gasoline95": 7.20,
      "Diesel": 7.00,
      "ElectricityPerKwh": 0.64
    },
    "EstimatedConsumption": {
      "GasolineKmPerLiter": 15.0,
      "DieselKmPerLiter": 14.0,
      "ElectricKwhPer100Km": 18.0
    }
  }
}
```

---

## 3. Data Models & Classification

### 3.1 Enum Definition

Create a standard enum to categorize vehicle energy types safely:

```csharp
public enum VehicleEnergyType
{
    Gasoline,
    Diesel,
    Electric,
    Unknown
}
```

### 3.2 Vehicle Energy Cost Model

Create a dedicated domain/DTO model for the energy cost data:

```csharp
public class VehicleEnergyCost
{
    public VehicleEnergyType EnergyType { get; set; }
    public decimal KmPerMonth { get; set; }
    public decimal? Consumption { get; set; }
    public string ConsumptionUnit { get; set; } = string.Empty;
    public decimal EnergyPrice { get; set; }
    public string EnergyPriceUnit { get; set; } = string.Empty;
    public decimal? MonthlyCost { get; set; }
    public bool IsEstimated { get; set; } = true;
}
```

---

## 4. Energy Type Resolution Logic

Construct a centralized utility/service function to determine the vehicle's `VehicleEnergyType` based on existing vehicle fields:
* `delek_cd`
* `delek_nm`
* `technologiat_hanaa_cd`
* `technologiat_hanaa_nm`

### Identification Requirements:
* Check **both** numerical codes and string descriptions (handling Hebrew and English variations).
* If the vehicle is identified as a **Plug-in Hybrid (PHEV)** or if the energy type cannot be resolved deterministically:
  * Set `EnergyType = VehicleEnergyType.Unknown`
  * Set `MonthlyCost = null`
* **Future Extension Readiness**: Structure the mapping interface to allow future addition of PHEV metrics:
  * `ElectricKwhPer100Km`
  * `FuelKmPerLiter`
  * `ElectricKmPercentage`
  * `FuelKmPercentage`

---

## 5. Calculation Logic & Mathematical Formulas

All internal calculations must be executed using higher-precision decimal types (`decimal`). Safeguards against division by zero must be strictly enforced.

### 5.1 Gasoline Calculation

$$\text{Monthly Cost} = \left(\frac{\text{KmPerMonth}}{\text{GasolineKmPerLiter}}\right) \times \text{GasolinePrice}$$

*Example:* $\frac{1500}{15} \times 7.20 = 720.00\text{ ILS}$

### 5.2 Diesel Calculation

$$\text{Monthly Cost} = \left(\frac{\text{KmPerMonth}}{\text{DieselKmPerLiter}}\right) \times \text{DieselPrice}$$

*Example:* $\frac{1500}{14} \times 7.00 = 750.00\text{ ILS}$

### 5.3 Electric Vehicle Calculation

$$\text{Monthly Cost} = \left(\frac{\text{KmPerMonth}}{100}\right) \times \text{ElectricKwhPer100Km} \times \text{ElectricityPrice}$$

*Example:* $\frac{1500}{100} \times 18 \times 0.64 = 172.80\text{ ILS}$

---

## 6. Architectural Layers & Service Abstraction

Do **NOT** place calculation logic inside Controllers or UI components.

### 6.1 Service Interface & Implementation

Create a dedicated domain service:

```csharp
public interface IVehicleEnergyCostService
{
    VehicleEnergyType GetVehicleEnergyType(Vehicle vehicle);
    VehicleEnergyCost CalculateCost(Vehicle vehicle, decimal? customKmPerMonth = null);
}
```

### 6.2 Architectural Flow & Future Extensibility

Design the service layer so that configuration values can eventually be retrieved from an external live API (e.g., government fuel database on data.gov.il) without altering calculation logic or UI contracts:

```
[ Current Implementation ]
Configuration ---> VehicleEnergyCostService ---> API Controller / UI

[ Future Extensibility Architecture ]
Data.gov.il / External API ---> EnergyPriceService ---> VehicleEnergyCostService ---> API Controller / UI
```

---

## 7. API Specification

Create a dedicated endpoint to retrieve energy cost metrics. Allow an optional `kmPerMonth` query parameter to support interactive user customization.

**Endpoint**: `GET /api/vehicles/{tozeretCd}/{degemCd}/energy-cost?kmPerMonth={optionalValue}`

### 7.1 Response Examples

#### Gasoline Response
```json
{
  "energyType": "Gasoline",
  "kmPerMonth": 1500,
  "consumption": 15.0,
  "consumptionUnit": "km/l",
  "energyPrice": 7.20,
  "energyPriceUnit": "ILS/l",
  "monthlyCost": 720.00,
  "isEstimated": true
}
```

#### Electric Response
```json
{
  "energyType": "Electric",
  "kmPerMonth": 1500,
  "consumption": 18.0,
  "consumptionUnit": "kWh/100km",
  "energyPrice": 0.64,
  "energyPriceUnit": "ILS/kWh",
  "monthlyCost": 172.80,
  "isEstimated": true
}
```

#### Unsupported / Unknown / PHEV Response
```json
{
  "energyType": "Unknown",
  "kmPerMonth": 1500,
  "consumption": null,
  "consumptionUnit": "",
  "energyPrice": 0,
  "energyPriceUnit": "",
  "monthlyCost": null,
  "isEstimated": true
}
```

---

## 8. User Interface & User Interaction Requirements

### 8.1 UI Placement
Position the new **Estimated Monthly Energy Cost** card directly **ABOVE** the existing WLTP section on the Vehicle Details page.

### 8.2 Interactive Monthly Mileage Customization
* Display a user-editable input field or numerical slider initialized with the default monthly distance (e.g., `1,500 km`).
* When the user modifies the monthly mileage, trigger a dynamic local recalculation or API fetch to update the estimated cost seamlessly in real time.

### 8.3 Cost Rounding & Formatting
* Internal calculations must remain precise `decimal` values.
* Display formatted final user values rounded to the nearest integer Israeli Shekel (e.g., `720 ₪`, `173 ₪`).

### 8.4 UI Mockups & Hebrew Copy

#### Gasoline / Diesel Layout
```
עלות צריכת אנרגיה חודשית
────────────────────────────────────────────────────────────

נסועה חודשית משוערת:    [ 1,500 ] ק"מ  (Editable Input)

צריכת דלק:               15 ק"מ לליטר
מחיר דלק:                7.20 ₪ לליטר

עלות חודשית משוערת:      720 ₪

* החישוב משוער ותלוי בנסועה, בצריכת הרכב ובמחירי האנרגיה.
```

#### Electric Layout
```
עלות צריכת אנרגיה חודשית
────────────────────────────────────────────────────────────

נסועה חודשית משוערת:    [ 1,500 ] ק"מ  (Editable Input)

צריכת חשמל:              18 kWh / 100 km
מחיר חשמל:               0.64 ₪ / kWh

עלות חודשית משוערת:      173 ₪

* החישוב משוער ותלוי בנסועה, בצריכת הרכב ובמחיר החשמל.
```

#### Unsupported / PHEV Layout
```
עלות צריכת אנרגיה חודשית
────────────────────────────────────────────────────────────

נסועה חודשית משוערת:    [ 1,500 ] ק"מ

עלות חודשית משוערת:      לא ניתן לחשב

* נתוני צריכת האנרגיה אינם זמינים עבור סוג רכב זה.
```

---

## 9. Unit Testing Requirements

Provide exhaustive Unit Tests covering the `VehicleEnergyCostService`:

1. **Gasoline Test**:
   * Input: `1500 km`, `15 km/l`, `7.20 ₪`
   * Expected: `MonthlyCost = 720.00` (UI formatted as `720 ₪`).

2. **Diesel Test**:
   * Input: `1500 km`, `14 km/l`, `7.00 ₪`
   * Expected: `MonthlyCost = 750.00` (UI formatted as `750 ₪`).

3. **Electric Test**:
   * Input: `1500 km`, `18 kWh/100km`, `0.64 ₪`
   * Expected: `MonthlyCost = 172.80` (UI formatted as `173 ₪`).

4. **Unknown / Unmapped Fuel Test**:
   * Input: Vehicle record with PHEV tags or missing fuel codes.
   * Expected: `MonthlyCost = null`.

5. **Edge Cases**:
   * Zero mileage (`KmPerMonth = 0`).
   * Zero consumption (`Consumption = 0` — verify zero division guard).
   * Zero energy price (`EnergyPrice = 0`).
   * Null values across vehicle metadata attributes.

6. **Custom Mileage Test**:
   * Pass custom `kmPerMonth = 2000` to verification service and validate correct mathematical scaling.

---

## 10. Out of Scope (Explicit Prohibitions)

* **DO NOT** perform external API requests per individual vehicle call.
* **DO NOT** alter the WLTP database or schema.
* **DO NOT** store estimated fuel consumption inside the core `Vehicle` record.
* **DO NOT** use `CO2_WLTP` to extrapolate fuel efficiency.
* **DO NOT** present any calculated figures as official government statistics.
* **DO NOT** attempt partial or hybrid PHEV estimations without full dataset availability.
* **DO NOT** modify existing unrelated API endpoints.