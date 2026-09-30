# UI_UPGRADE_SPEC.md — DataGovIL Website Upgrade (Vehicle Lookup by Registration Number)

> Read `PROJECT_CONTEXT.md` and this file before touching any UI code.
> This spec defines **what** to show and **how**. It is written for an AI assistant (or a human) picking up the repo cold.
> **This spec is written in English, but the website UI itself must be 100% Hebrew (RTL).** All user-facing strings are given in Hebrew where they matter. When you make a new design/architecture decision, add it here.

---

## 1. Goal

Upgrade the website so that a lookup by registration number (`GET /api/Vehicles/{registrationNumber}`) displays **everything the new `VehicleRecord` returns — but only when a value is present**. A field that is `null`, missing, or an empty string is not rendered at all (no "N/A", no empty row, no empty group).

Primary audience: **a person deciding whether to buy a used car.** The screen must quickly answer:

1. Is the vehicle in a valid state for a purchase? (active / permanently cancelled / inactive, valid test, open recalls)
2. Are there red flags? (structural change, color change, tire change, many owners, commercial/taxi/rental ownership, high mileage)
3. What is it worth? (list price → value adjustment → estimated value, and exactly what raised or lowered it)
4. What is this car, really? (model, engine, safety, pollution, equipment)

**Design:** very professional, modern and innovative — light and shadow, layering (glass/elevation), subtle animations, SVG icons, full Hebrew RTL support, mobile-first responsive, dark mode.

---

## 2. Stack and working rules

- **Vanilla HTML / CSS / JS** (no frameworks). ES Modules.
- **No external runtime dependencies** (no CDN libraries). SVG icons inline or as a single `<symbol>` sprite.
- `<html lang="he" dir="rtl">`. Use **CSS Logical Properties** (`margin-inline-start`, `padding-inline`, `inset-inline-end`), never `left/right`.
- Numbers, plates and VINs are rendered with `dir="ltr"` (`<bdi>` or `unicode-bidi: isolate`) so they don't flip inside Hebrew text.
- All colors, radii, shadows and spacing are **CSS custom properties** (design tokens) — see section 9.
- Accessibility: WCAG AA, full keyboard navigation, `aria-expanded` on accordions, `prefers-reduced-motion`, good contrast in dark mode too, `role="status"` / `aria-live` for loading/error messages.

### 2.1 Baseline requirements (mandatory)

**Language:** the entire site is **Hebrew only** — all UI text, labels, error messages, tooltips, `aria-label`, `alt`, `<title>`, meta descriptions and validation messages. No visible English strings (except values coming from the datastore such as VIN/codes). All strings live in a single file (`js/strings.he.js`), not scattered through the code.

**Device support:** must look and work great on **desktop and mobile**.
- Mobile-first. Breakpoints: `≤ 480` (phone), `481–768` (large phone / small tablet), `769–1200` (tablet / small laptop), `> 1200` (desktop).
- Mandatory test widths: 360, 390, 768, 1024, 1440. **No horizontal scroll at any width.**
- Touch: tap targets ≥ 44×44px, comfortable spacing, no essential action depends on hover (hover is enhancement only). Accordions and buttons must be thumb-friendly.
- Viewport: `<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">` and `env(safe-area-inset-*)` support (iPhone notch).
- Mobile search input: `inputmode="numeric"`, `autocomplete="off"`, font-size ≥ 16px (prevents iOS auto-zoom).
- Desktop: wide layout with a sticky summary column (section 9). Mobile: single column, buyer summary and status banner first, sticky anchor nav ("סיכום · שווי · היסטוריה · מפרט").
- Comparison: desktop = side-by-side columns; mobile = horizontal swipe between vehicles with a fixed summary row on top.

**Font:** clear, beautiful, highly readable Hebrew.
- **Primary font: `Heebo`** (clean sans-serif, full Hebrew + Latin, weights 100–900). Alternatives: `Assistant` / `Rubik`. Stack: `"Heebo", "Assistant", "Segoe UI", system-ui, Arial, sans-serif`.
- Loading: `font-display: swap`; load only weights 400 / 500 / 700 (+800 for large headings if needed); prefer **self-hosting (woff2)** for performance and privacy; `preload` the primary font.
- Base size **16px** (mobile) up to **17–18px** (desktop); `line-height` 1.6 for body, 1.25 for headings; fluid sizes with `clamp()`.
- Numbers: `font-variant-numeric: tabular-nums` in tables and amounts. VIN/codes: monospace (`ui-monospace, "SF Mono", Consolas, monospace`).
- Hierarchy: large bold Hero title (700–800), small gray secondary labels (500), prominent values (600–700). Never below 13px for any text. Avoid italics in Hebrew.
- AA contrast minimum, in dark mode as well.

### Suggested file structure

```
wwwroot/  (or the existing UI folder)
  index.html
  css/
    tokens.css        <- design tokens (light/dark)
    base.css          <- reset, typography, RTL
    components.css    <- cards, badges, accordion, tables, timeline
    animations.css    <- keyframes + reveal classes
  js/
    main.js           <- event wiring, search flow
    api.js            <- fetch wrapper + error handling (404/502/network)
    format.js         <- date / currency / number formatting (section 6)
    view-model.js     <- turns VehicleRecord into a view model (flags, scores, groups)
    strings.he.js     <- all Hebrew UI strings
    render/
      hero.js  status.js  buyerSummary.js  valuation.js
      accordion.js  timeline.js  recalls.js  specs.js  compare.js ...
    icons.js          <- sprite / SVG injection
```

---

## 3. API contract (source of truth: Swagger.JSON + the DTO files)

| Endpoint | UI usage |
|---|---|
| `GET /api/Vehicles/{registrationNumber}` | **The main screen.** 200 = `VehicleRecord`, 404 = not found |
| `GET /api/Vehicles/{registrationNumber}/history` | Also exists standalone, but history is already merged into `VehicleRecord.history` — **do not call it additionally**, only for refresh |
| `GET /api/PriceList/{manufacturerCode}/{modelCode}/{manufactureYear}` | Optional: "full price list for model/year" (list of `VehiclePriceListRecord`) inside a lazy-loaded accordion |
| `GET /api/Manufacturers/{manufacturerCode}/{modelCode}` | Not needed — `manufacturerModel` is already embedded in the vehicle (except when it's `null`) |
| `GET /api/Vehicles?q=&page=&pageSize=` | Free-text search (keep it if the site already has it) |

**JSON property names are camelCase** (ASP.NET default), exactly as in the DTOs.
**Errors:** `502` = problem talking to data.gov.il (returned as `ProblemDetails`) → friendly error screen with a retry button. `404` → "vehicle not found" screen with hints (check the number is correct, 7–8 digits).

### 3.1 `VehicleRecord` — all fields with Hebrew UI labels

**Identification**
| Field | Hebrew label | Notes |
|---|---|---|
| `registrationNumber` | מספר רכב | Render as a license plate (see 5.1) |
| `id` | מזהה | Technical — only in the "more datastore fields" accordion |
| `chassisNumber` | מספר שילדה (VIN) | Copy button; LTR |
| `engineNumber` | מספר מנוע | |
| `commercialName` | כינוי מסחרי | Part of the title when present |

**Make & model**
`manufacturerName` (תוצר), `manufacturerCode` (קוד תוצר), `modelName` (דגם), `modelCode` (קוד דגם), `modelType` (סוג דגם), `trimLevel` (רמת גימור), `vehicleTypeName` / `vehicleTypeCode` (סוג רכב), `manufactureYear` (שנת יצור), `importType` (סוג יבוא)

**Engine & fuel**
`engineManufacturer` (תוצר מנוע), `engineModel` (דגם מנוע), `fuelType` (סוג דלק), `totalWeight` (משקל כולל, kg)

**Color & tires**
`color` (צבע רכב), `colorCode` (קוד צבע), `frontTire` (צמיג קדמי), `rearTire` (צמיג אחורי)

**Test & registration**
`lastTestDate` (תאריך מבחן אחרון), `testValidUntil` (תוקף), `roadEntryDate` (מועד עליה לכביש), `registrationOrder` (הוראת רישום), `ownershipType` (בעלות נוכחית), `updatedDate` (תאריך עדכון), `cancellationDate` (תאריך ביטול)

**Safety & pollution (vehicle level)**
`safetyEquipmentLevel` (רמת אבזור בטיחותי), `pollutionGroup` (קבוצת זיהום)

**Status flags**
- `isPermanentlyCancelled` (ביטול סופי) — **critical**
- `isInactive` (לא פעיל) — **critical** (only serialized when `true`; absent = `false`)
- `isSafetyDiscountEligible` (זכאי להנחה לאחר התקנת מערכות בטיחות) — only serialized when `true`

**Nested objects (all optional)**
`manufacturerModel`, `recalls[]`, `history` (`technical` + `ownershipHistory[]`), `depreciation`, `extensionData`

### 3.2 `history`
- `history.technical`: `lastTestOdometer` (קילומטראז' בטסט אחרון), `structureChangeIndicator` (שינוי מבנה), `lpgChangeIndicator` (אינדיקציה לשינוי/התקנת מערכת גפ״מ — datastore column `gapam_ind`; the car can also run on LPG in addition to petrol. **Not** an accident indicator and **no** effect on value or verdict — informational only), `colorChangeIndicator` (שינוי צבע), `tireChangeIndicator` (שינוי צמיגים), `firstRegistrationDate` (רישום ראשון), `originalityName` (מקוריות), `engineNumber`, `registrationNumber`, `id`. The indicators are `bool?` — `null` = **unknown** (render "לא ידוע" in gray, never "no").
- `history.ownershipHistory[]`: `ownershipYearMonth` (ownership month, usually `yyyyMM`), `ownershipType`, `id`, `registrationNumber`.

### 3.3 `recalls[]` (unperformed recalls)
`recallId`, `recallType` (סוג ריקול), `faultType` (סוג תקלה), `faultDescription` (תיאור תקלה), `openedDate` (תאריך פתיחה).

### 3.4 `depreciation` (value adjustment)
`listPrice` (מחיר מחירון), `depreciationPercent` (total % change, **signed**), `depreciationValue` (total change in ILS), `estimatedValue` (ערך משוערך), `carAge` (years, `double`), `kilometers`, `ownerCount`, `originality`, `ownershipType` (the ownership type chosen for the calculation — the worst one), `vehicleCategory`, `ownerCategory` (enums serialized as strings), `lines[]` (`factor`: `Kilometers | OwnerCount | OwnershipType`, `description`, `percent`, `value`).
**Important:** `listPrice` / `depreciationValue` / `estimatedValue` / `value` are `null` when no price-list price was found — in that case show only percentages and explain: "לא נמצא מחירון לדגם, מוצגים אחוזי שינוי בלבד".

**Enum → Hebrew mapping** (use these in the UI instead of the English strings):
- `VehicleCategory`: Private=פרטי, Commercial4t=מסחרי עד 4 טון, Commercial=מסחרי, Taxi=מונית, Minibus=מיניבוס
- `OwnerCategory`: Private=פרטי, Commercial=מסחרי, Public=ציבורי, Rental=השכרה/ליסינג, Kibbutz=קיבוץ, Government=ממשלתי, Taxi=מונית, DriveTech=דרייב-טק
- `Factor`: Kilometers=קילומטראז', OwnerCount=מספר בעלים, OwnershipType=סוג בעלות

### 3.5 `manufacturerModel` (make/model catalog — rich technical data)
**All fields are strings.** Indicators arrive as `"0"` / `"1"` / `"true"` / … or even numbers — normalize with one `toBool()` function (same logic as `VehicleTechnicalHistoryRecord.ToBool`). `null`/empty = don't render the row.

Display groups (see 5.7):
- **General spec:** `bodyType`, `doorCount`, `seatCount`, `horsepower`, `engineDisplacement`, `driveName`/`driveCode`, `automaticTransmissionIndicator`, `driveTechnologyName`, `fuelName`, `curbWeight`, `totalWeight`, `liftingLoadWeight`, `height`, `towingCapacityWithBrakes`, `towingCapacityWithoutBrakes`, `manufacturerCountryName`, `tozar`, `trimLevel`, `commercialName`
- **Comfort equipment:** `airConditioningIndicator`, `powerSteeringIndicator`, `powerWindowsSource`/`powerWindowCount`, `powerSunroofIndicator`, `alloyWheelsIndicator`, `cargoBoxIndicator`
- **Safety:** `safetyScore`, `safetyEquipmentLevel`, `absIndicator`, `airbagCount`/`airbagsSource`, `stabilityControlIndicator`, `laneDepartureControlIndicator`, `forwardDistanceMonitoringIndicator`, `blindSpotDetectionIndicator`, `adaptiveCruiseControlIndicator`, `pedestrianDetectionIndicator`, `brakeAssistIndicator`, `reverseCameraIndicator`, `tirePressureMonitoringIndicator`, `seatbeltReminderIndicator`, `automaticLightingForwardTravelIndicator`, `automaticHighBeamControlIndicator`, `dangerousApproachDetectionIndicator`, `trafficSignRecognitionIndicator`, `twoWheeledVehicleDetection`, `activeLaneControl`, `automaticBrakingReverseTravel`, `speedControlIsa`, `emergencyBrakingPedestriansCyclists`, `sideCollisionBlindSpot`, `alcoLock`
- **Pollution & emissions:** `greenScore`, `pollutionGroup`, `co2Amount`, `noxAmount`, `pm10Amount`, `hcAmount`, `hcNoxAmount`, `coAmount`, the `*City` and `*Highway` variants, and the WLTP metrics: `co2Wltp`, `hcWltp`, `pmWltp`, `noxWltp`, `coWltp`, `co2WltpNedc`
- **Other:** `feeGroupCode` (fee group), `homologationTypeName`/`Code`, `euTypeApproval`, `converterTypeName`/`Code`, `batteryVoltageDg`, `modelType`, `modelYear`, `id`, and all `*RegulationSource` fields

### 3.6 `extensionData`
Every key that isn't mapped is shown in a closed accordion "שדות נוספים מהמאגר" as a key/value table (key as-is, LTR). Same for the `extensionData` of nested objects. **Never drop fields.**

---

## 4. Screen states

1. **Start (empty):** Hero with a large license-plate-style search field, example number, recent searches (localStorage — see "Additions").
2. **Loading:** skeleton screens shaped like the real cards (shimmer), not a generic spinner. `aria-live="polite"`.
3. **Found:** the full screen (section 5).
4. **404:** SVG illustration + "לא נמצא רכב עם המספר X" + tips.
5. **502 / network:** "מאגר הנתונים הממשלתי אינו זמין כרגע" + retry button (`ProblemDetails.detail` inside a "technical details" accordion).
6. **Partial result:** a vehicle found only via `isSafetyDiscountEligible` (no fields except number/updated date) — show "נמצא רק ברשימת ההנחה לבטיחות, אין נתוני רישום מלאים".

**Client-side validation:** accept 5–8 digits; strip spaces and dashes (`12-345-67` → `1234567`); don't send a request if invalid, show a message under the field.

---

## 5. Full screen layout (top to bottom)

### 5.1 Vehicle Hero (always visible)
- Large title: `manufacturerName` + `commercialName ?? modelName` + `trimLevel` + `manufactureYear`.
- A styled **Israeli license plate** (yellow, blue strip with "IL", plate font) with `registrationNumber` in plate format (`12-345-67` / `123-45-678`).
- Chips: fuel type (icon), color (color dot — basic color-name→hex map, else a droplet icon), vehicle type, current ownership.
- Buttons: copy link (with `?plate=`), print/PDF, compare with another vehicle (5.9).

### 5.2 Status banner — **the most prominent element on the page**
Directly under the Hero, full-color with a large icon and an entrance animation. Priority order (highest is the main banner, the rest are small badges):

| State | Condition | Design |
|---|---|---|
| **Permanently cancelled** | `isPermanentlyCancelled` | Deep red, big X/sign icon, text "הרכב בוטל סופית" + `cancellationDate`; subtle red corner accent. A ribbon across the page: "רכב מבוטל — נתוני היסטוריה בלבד" |
| **Inactive** | `isInactive` | Orange/amber, "pause" icon, "הרכב אינו פעיל" |
| **Test expired** | `testValidUntil < today` | Red, "הטסט פג לפני X ימים" |
| **Test expiring soon** | ≤ 30 days | Yellow, "הטסט יפוג בעוד X ימים" |
| **Open recall** | `recalls.length > 0` | Red-orange, "X ריקולים שלא בוצעו" (clickable — scrolls to the accordion) |
| **Active and OK** | none of the above | Green, check icon, "הרכב פעיל · טסט בתוקף עד DD/MM/YYYY" |

Note: when the vehicle is cancelled/inactive — **hide or gray out** value/test widgets that no longer make sense, and explain why.

### 5.3 "Buyer Summary" — main card
A large card with a **computed score/verdict** (in `view-model.js`) plus a flag list:

- **Verdict (3 levels):** green "נראה תקין" / yellow "כדאי לבדוק לפני קנייה" / red "סיכון גבוה". Red if: cancelled / inactive / open recall / structural change. Yellow if: color change / tire change / ≥4 owners / non-private ownership / high mileage relative to age / test expired.
- **Short ✅ / ⚠️ / ❌ list**, each row with an icon and one plain-Hebrew sentence, e.g. "❌ מסומן שינוי מבנה בהיסטוריה", "⚠️ 5 בעלים קודמים", "✅ אין ריקולים פתוחים".
- **Always** add a small disclaimer: "הנתונים מבוססים על מאגרי data.gov.il ואינם תחליף לבדיקה מקצועית במכון בדיקה".
- Threshold logic (km per year etc.) is **centralized in one config object** (`thresholds` in `view-model.js`) so it can be tuned.

### 5.4 Vehicle value (`depreciation`) — prominent
"How much is it worth?" panel:
- **SVG waterfall chart:** list price → each `lines[]` entry → estimated value. Green bars for increases (`percent > 0`), red for decreases (`percent < 0`), animated build on scroll-into-view (IntersectionObserver).
- **The big number:** `estimatedValue` in Israeli `₪` format, next to an arrow and the total percent (`depreciationPercent`) colored (green ▲ / red ▼).
- **Factors table:** per `line`: icon by `factor`, `description`, signed percent with color and arrow, and ILS value. **A factor that raises value = green with `+`; one that lowers = red with `−`.** The most impactful factor gets a "ההשפעה הגדולה ביותר" badge.
- **Basis chips:** car age (`carAge` rounded to one decimal, "X.X שנים"), mileage (`kilometers` + per-year average), owner count, vehicle category and owner category (in Hebrew).
- **`listPrice == null`:** show percentages only + an explanation, hide the ILS waterfall.
- "מה זה אומר?" button (tooltip/short modal) explaining how the adjustment is computed: kilometers, owners, ownership type (the "worst" of owners and originality). **Never present the estimated value as a recommended purchase price** — label it "הערכה בלבד".

### 5.5 History — ownership timeline + history flags
- **Technical history flags** as "light cards" (grid): structural change, color change, tire change. (LPG is shown as a neutral fact next to first registration, not as a flag.) `true` = red/orange with a warning icon and a subtle pulse; `false` = green "לא תועד"; `null` = gray "לא ידוע". Careful wording: **"documented / not documented in the datastore"** (not "yes/no") — absence of a record is not proof.
- **Ownership timeline (vertical):** from `ownershipHistory[]`, sorted newest → oldest (if it can't be sorted, keep arrival order). Each node: formatted month/year (`yyyyMM` → "מרץ 2021"), ownership type with icon and color (private = blue, commercial/rental/taxi = orange, government/public = gray). Header: "X בעלים" + highlight non-private ownerships. **Compute ownership duration** between nodes when possible ("כ-2 שנים ו-3 חודשים").
- Helper row: first registration (`firstRegistrationDate`), originality (`originalityName`), last test odometer (`lastTestOdometer`) with a visual odometer ("X ק"מ — ממוצע Y ק"מ לשנה", compared against a national average defined in config).

### 5.6 Recalls — prominent when present
If `recalls.length > 0`: a red-orange card, open by default, title "X ריקולים שלא בוצעו — חשוב לפני קנייה". A card per recall: recall type, fault type, fault description, opened date, ID. Note: "מומלץ לוודא מול היבואן שהריקול בוצע/יבוצע ללא עלות". If `recalls` is empty/missing — a small green line "לא נמצאו ריקולים פתוחים" (inside the buyer summary only).

### 5.7 Accordions (data groups)
Custom `<details>`-like component (or `<button aria-expanded>` + smooth height animation via `grid-template-rows: 0fr → 1fr`). Each accordion: SVG icon, title, **count badge** ("12 נתונים"), rotating chevron, and a key/value grid (2–3 columns, 1 on mobile). **An accordion renders only if it has at least one field with a value.** "Open all / Close all" buttons.

| Accordion | Default | Content |
|---|---|---|
| Vehicle identity (זהות הרכב) | **open** | plate number, VIN (copy), engine number, make/model/model type/trim, year, import type, commercial name |
| Test & registration (טסט ורישוי) | **open** | last test, valid until (with countdown), road entry, registration order, current ownership, cancellation date, updated date |
| Engine, fuel & weight | **open** | engine model/manufacturer, fuel type, horsepower, displacement, drive, transmission, total/curb weight |
| Recalls | **open if any** | section 5.6 |
| Safety (בטיחות) | closed | safety score (gauge/stars), equipment level, safety-equipment matrix (✔/✖ per system, grouped "present / absent") |
| Equipment & comfort | closed | A/C, power steering, windows, sunroof, alloy wheels, automatic transmission, etc. |
| Pollution & emissions | closed | green score (`greenScore`) as a colored 1–15 scale, pollution group, CO₂/NOx/PM10/HC/CO (combined/city/highway), WLTP |
| Dimensions & towing | closed | height, load capacity, towing with/without brakes, doors, seats |
| Color & tires | closed | color, color code, front/rear tire |
| Approvals & fees | closed | fee group, homologation, `euTypeApproval`, converter, battery voltage |
| Model price list (optional) | closed | lazy-loaded from `/api/PriceList/...` on first open; importer/price table |
| More datastore fields | closed | all `extensionData` |

### 5.8 Data cell
Each data cell: small gray `label` + prominent `value`. Booleans = ✔/✖ chip (green/gray). Numbers with units ("1,598 סמ"ק", "132 כ"ס", "1,340 ק"ג"). Long values — truncate + "עוד".

### 5.9 Vehicle comparison mode
A "השווה" button opens a field for a second registration number (up to 3 vehicles, max 3 requests). **Side-by-side columns** (mobile: swipeable cards with a fixed summary on top). **Comparison shows only what matters to a used-car shopper**, in this order, marking the **winner** of each row (subtle green ✓) when determinable:

1. Status (active / cancelled / inactive), test valid until, open recalls (count)
2. **Estimated value**, list price, total depreciation (%), the most impactful factor
3. Year and age, **mileage** and per-year average
4. **Owner count**, current ownership type, and non-private ownerships (commercial / rental / taxi)
5. History flags: structural change / color change / tire change
6. Originality, import type
7. Safety: safety score, equipment level, key systems (ABS, airbags, stability control, emergency braking, lane keeping, reverse camera)
8. Cost of ownership: fee group, pollution group, green score, fuel type, engine displacement, horsepower
9. Basic spec: transmission, drive, seats, doors

- Rows where values **differ** are highlighted (toggle "show only differences").
- **Summary at top:** "הרכב הכי כדאי לפי הנתונים" — simple, transparent scoring (show why), with a "general recommendation only" note.
- Missing values = gray "—" (not "no", not "0").

### 5.10 Footer
Data source (data.gov.il), last update (`updatedDate` if present), a short legal disclaimer ("המידע כפי שמופיע במאגרים הממשלתיים ועשוי להיות חלקי"), privacy link.

---

## 6. Israeli formatting (`format.js`) — mandatory, one function per type

- **Dates:** `DD/MM/YYYY` (e.g. `05/03/2024`) via `Intl.DateTimeFormat('he-IL', {day:'2-digit', month:'2-digit', year:'numeric'})`. Long form: "5 במרץ 2024".
- **Parsing server dates:** the datastore returns mixed formats (ISO `2024-03-05T00:00:00`, `yyyy-MM-dd`, `yyyyMM` for ownership, sometimes `dd/MM/yyyy`). Write `parseGovDate(str)` that recognizes all of them and returns `Date | null` — **do not** use `new Date(str)` directly. `yyyyMM` → "מרץ 2021" (month + year only). If unparseable, show the raw string.
- **Currency:** `Intl.NumberFormat('he-IL', {style:'currency', currency:'ILS', maximumFractionDigits:0})` → `12,345 ₪`. Signed change: `signDisplay:'always'` with color (green positive / red negative) — **never rely on color alone**; add an arrow/sign too.
- **Percent:** at most one decimal, explicit sign: `−3.5%`, `+2%`.
- **Numbers:** thousands separator (`1,598`). Mileage + "ק"מ". Weight + "ק"ג". Displacement + "סמ"ק". Power + "כ"ס". Age + "שנים".
- **License plate:** 7 digits `XX-XXX-XX`, 8 digits `XXX-XX-XXX`.
- **Booleans:** "כן / לא / לא ידוע".
- Any non-Hebrew value (VIN, engine name, model codes) — `dir="ltr"` inside `<bdi>`.
- **Date math** (days to test etc.) in local time `Asia/Jerusalem`, normalized to midnight (not UTC).

---

## 7. Animations and effects (subtle, professional)

- **Scroll reveal:** `IntersectionObserver` + `opacity/translateY` (staggered — 60ms between cards).
- **Hero:** smooth transition from search → result (the search field collapses into a sticky top bar).
- **Status banner:** bounce-in entrance + the icon draws itself (`stroke-dashoffset`). Red states: gentle `pulse` on the icon ring (never aggressive flashing).
- **Numbers:** short count-up (600–900ms) for `estimatedValue`, mileage and percentages.
- **Waterfall chart:** bars "grow" in sequence.
- **Accordion:** smooth height (`grid-template-rows`), rotating chevron, subtle ripple on click.
- **Cards:** hover — slight lift + deeper shadow; clear focus ring.
- **Skeleton shimmer** while loading.
- **Everything honors `prefers-reduced-motion`** — disable movement, keep a short fade only.
- Performance: only `transform`/`opacity` in animations; no layout thrash.

---

## 8. Icons (SVG)

One sprite (`<symbol>`) in a consistent style (1.75px stroke, rounded): car, plate, check / X / warning / info, test/calendar, engine, fuel, weight, color (droplet), tire, chassis (VIN), owner/person, wrench, shield (safety), leaf (pollution), recall (triangle + exclamation), history (clock), structural change, odometer, price/currency, trend up/down, document, copy, share, print, compare. Ownership types: private, commercial, taxi, rental, government, kibbutz. **Decorative icons get `aria-hidden="true"`**; icons that carry meaning get `sr-only` text.

---

## 9. Design system (`tokens.css`)

- **Palette:** cool neutral (background `#F5F7FB`, card `#FFFFFF`), deep indigo/blue primary, and three status colors: `--ok` green, `--warn` amber, `--bad` red — each with a soft background (`--ok-bg`) and a text shade (`--ok-ink`) at AA contrast.
- **Dark mode:** `@media (prefers-color-scheme: dark)` + a manual toggle (stored in localStorage). In dark mode shadows are replaced by glow/subtle borders.
- **Three shadow levels:** `--shadow-1` (card), `--shadow-2` (hover), `--shadow-3` (modal/banner); combine outer shadow + thin inner highlight for depth.
- **Radii:** 12 / 16 / 24px. **Spacing:** 4px scale.
- **Typography:** see 2.1 (Heebo).
- **Effects:** subtle glassmorphism for the sticky top bar (`backdrop-filter: blur`), subtle gradient in the Hero — **readability before decoration**.
- **Grid:** mobile 1 column, tablet 2, desktop 12-column with a sticky side summary column (buyer summary + value).
- **Print:** `@media print` — all accordions open, no shadows/animations, header with plate number and generation date.

---

## 10. Additions beyond the original request (important for buyers)

1. **Buyer Summary + verdict** (5.3) — the highest-value feature for someone deciding.
2. **Countdown to test expiry** with urgency coloring.
3. **Mileage per year** vs. a national average (config value) — flags suspicious cars (odometer rollback / heavy use).
4. **Non-private ownership** (taxi, rental, leasing, commercial, government) flagged — it heavily affects value.
5. **"Documented / not documented"** instead of "yes / no" in history flags — accuracy and responsibility.
6. **Copy VIN / plate number**, share link (`?plate=`), print / save as PDF.
7. **Recent searches** in localStorage (up to 8, with a clear button) — browser only, never sent to the server.
8. **Comparison mode** (5.9) with highlighted differences and a transparent recommendation.
9. **Visual safety gauge and green score** instead of dry numbers.
10. **Alerts center at the top** — all red flags in one place with anchor links to details.
11. **Safety-equipment matrix** (✔/✖) grouped "present / absent" — lets users compare models and see what's missing.
12. **Lazy loading** of the full price list and rendering closed accordions only on first open (performance).
13. **Keyboard support** (`/` to focus search, `Esc` to close modals) and real **skeletons**.
14. **Soft offline/error mode:** keep the last result in session memory to show during network failure (labeled "נתונים שמורים").
15. **Privacy:** VIN and engine number are identifying data — not stored in search history and not included in share links. (See also the PII note in `PROJECT_CONTEXT.md`.)

---

## 11. Gotchas — read before "fixing" anything that looks like a bug

1. **All fields are strings** (except the status flags, `depreciation`, and the technical-history `*Indicator` fields which are `bool?`). Numbers like `manufactureYear` or `totalWeight` arrive as strings — convert only for math/formatting.
2. **`manufacturerModel` indicators** are strings in mixed formats (`"0"`, `"1"`, `"true"`, numbers) — one shared `toBool()`, and `null` ≠ `false`.
3. **`isInactive` / `isSafetyDiscountEligible` disappear when `false`** (`WhenWritingDefault`), and `manufacturerModel/recalls/history/depreciation/extensionData` disappear when `null`. **Check existence, not value.**
4. **Inactive / personal-import vehicles** arrive with a **partial** `manufacturerModel` built from the record itself — many empty fields. Empty groups are simply not rendered.
5. **Cancelled vehicle:** no valid `testValidUntil/lastTestDate` — do not show "test expired"; show the cancellation status only.
6. **`depreciation` can arrive without a price** (`listPrice = null`) — handle explicitly (5.4).
7. **`ownershipHistory` is not guaranteed sorted** — sort client-side by `ownershipYearMonth` (`yyyyMM` string — lexicographic comparison is valid).
8. **PII:** never log VIN or plate number to `console`/logs in production.
9. **Direction:** Hebrew text in RTL with LTR components (numbers, VIN) — always local `<bdi>`/`dir="ltr"`, otherwise punctuation and minus signs "jump".
10. **Minus sign:** use `−` (U+2212), not `-`, next to numbers in RTL, wrapped in `<bdi>`.

---

## 12. Suggested work order

- [ ] **Phase 1 — Foundation:** `tokens.css`, `base.css`, icon sprite, `format.js` (+ small unit tests for formatters and `parseGovDate`), `api.js`, `strings.he.js`.
- [ ] **Phase 2 — Base screen:** Hero + plate + status banner + loading/404/502 states.
- [ ] **Phase 3 — Accordions:** accordion component + groups from 5.7 (only fields with values).
- [ ] **Phase 4 — The key pieces:** value (waterfall), history (timeline + flags), recalls, buyer summary.
- [ ] **Phase 5 — Animations**, dark mode, print, accessibility.
- [ ] **Phase 6 — Comparison** (5.9) and the additions in section 10.
- [ ] **Phase 7 — Manual testing** using the checklist below.

---

## 13. Acceptance criteria / test checklist

- [ ] Regular active vehicle: green banner, all relevant accordions shown, no empty rows.
- [ ] **Permanently cancelled** vehicle: prominent red banner + page-wide ribbon + irrelevant test/value widgets hidden or grayed.
- [ ] **Inactive** vehicle: amber banner, partial `manufacturerModel` renders without errors.
- [ ] Vehicle with **recalls**: recalls card open + alert in the banner.
- [ ] Vehicle with **structural change**: red/yellow verdict per the logic, flag highlighted.
- [ ] Vehicle with **many owners and commercial/taxi ownership**: highlighted in the timeline and in the depreciation factors.
- [ ] `depreciation.listPrice = null`: no `NaN` / `null ₪`, explanation shown.
- [ ] Positive-percent fields (value-raising) green with `+`/▲; negative red with `−`/▼ — and accessible without color.
- [ ] All dates `DD/MM/YYYY`, all amounts `₪` with thousands separator.
- [ ] `extensionData` fully shown in a closed accordion.
- [ ] RTL correct, VIN/numbers don't flip, no horizontal scroll on mobile (360px).
- [ ] All UI text is Hebrew (no visible English strings), strings centralized in `strings.he.js`.
- [ ] Tested on real phones (iOS + Android) and desktop at 360/390/768/1024/1440; tap targets ≥ 44px; no auto-zoom in the search field.
- [ ] Heebo loads (woff2, `swap`) with a proper fallback; base size ≥ 16px; text is clear in both light and dark modes.
- [ ] `prefers-reduced-motion` and dark mode work; full keyboard navigation; Lighthouse accessibility ≥ 95.
- [ ] Comparing 2–3 vehicles: differences highlighted, missing values "—", no crash when one vehicle lacks `depreciation`/`history`.
- [ ] 404 / 502 / network loss: appropriate screens, no visible stack trace.
