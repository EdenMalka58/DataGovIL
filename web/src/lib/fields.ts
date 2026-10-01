import { he } from "../strings.he";
import type { ManufacturerModelRecord } from "../types/manufacturer";
import type { VehicleRecord } from "../types/vehicle";
import {
  clean,
  daysFromToday,
  formatCc,
  formatCurrency,
  formatDate,
  formatHp,
  formatKg,
  formatMonthYear,
  formatNumber,
  needsLtr,
  parseGovDate,
  parseGovDateDetailed,
  toBool,
  toNumber,
  withUnit,
} from "./format";
import { formatPlate } from "./plate";
import { isOffRoad, thresholds } from "./viewModel";

export type CellTone = "ok" | "warn" | "bad" | "muted";

export type DataCell = {
  key: string;
  label: string;
  /** Display text. Cells without a value are never created. */
  value: string;
  bool?: boolean;
  ltr?: boolean;
  mono?: boolean;
  copyable?: boolean;
  hint?: string;
  hintTone?: CellTone;
};

const L = he.labels;
type MM = ManufacturerModelRecord;
type MMKey = Exclude<keyof MM, "extensionData">;

function textCell(key: string, value: unknown, opts: Partial<DataCell> = {}): DataCell | null {
  const v = clean(value);
  if (!v) return null;
  return { key, label: opts.label ?? L[key] ?? key, value: v, ltr: opts.ltr ?? needsLtr(v), ...opts };
}

function formattedCell(
  key: string,
  raw: unknown,
  format: (v: unknown) => string | null,
  opts: Partial<DataCell> = {},
): DataCell | null {
  if (!clean(raw)) return null;
  const v = format(raw);
  if (!v) return null;
  return { key, label: opts.label ?? L[key] ?? key, value: v, ...opts };
}

function dateCell(key: string, raw: unknown, opts: Partial<DataCell> = {}): DataCell | null {
  return formattedCell(key, raw, formatDate, opts);
}

/** Indicator cell: ✔/✖ chip for recognizable booleans, raw text otherwise, nothing for null. */
function boolCell(key: string, raw: unknown, opts: Partial<DataCell> = {}): DataCell | null {
  const v = clean(raw);
  if (!v) return null;
  const b = toBool(v);
  if (b == null) return textCell(key, v, opts);
  return { key, label: opts.label ?? L[key] ?? key, value: b ? he.common.yes : he.common.no, bool: b, ...opts };
}

function compact(cells: (DataCell | null)[]): DataCell[] {
  return cells.filter((c): c is DataCell => c != null);
}

/**
 * Vehicle-level value first; the catalog value is only added when it's the sole source or it differs.
 * Marks the catalog key as used either way.
 */
function withCatalog(
  used: Set<string>,
  key: string,
  vehicleValue: unknown,
  mmKey: MMKey,
  mm: MM | null | undefined,
  make: (key: string, value: unknown, label?: string) => DataCell | null,
): (DataCell | null)[] {
  used.add(mmKey);
  const own = clean(vehicleValue);
  const cat = clean(mm?.[mmKey]);
  if (own && cat && own !== cat) {
    return [make(key, own), make(`mm.${mmKey}`, cat, `${L[mmKey] ?? L[key] ?? key} (קטלוג)`)];
  }
  return [make(key, own ?? cat)];
}

function mmCells(
  used: Set<string>,
  mm: MM | null | undefined,
  keys: MMKey[],
  make: (key: string, value: unknown) => DataCell | null,
): (DataCell | null)[] {
  return keys.map((k) => {
    used.add(k);
    return make(k, mm?.[k]);
  });
}

const t = (key: string, value: unknown, label?: string) => textCell(key, value, label ? { label } : {});
const b = (key: string, value: unknown, label?: string) => boolCell(key, value, label ? { label } : {});
const kg = (key: string, value: unknown, label?: string) =>
  formattedCell(key, value, formatKg, label ? { label } : {});
const gkm = (key: string, value: unknown) =>
  formattedCell(key, value, (v) => withUnit(v, he.units.gPerKm, 3));

export const SAFETY_SYSTEM_KEYS: MMKey[] = [
  "absIndicator",
  "stabilityControlIndicator",
  "laneDepartureControlIndicator",
  "forwardDistanceMonitoringIndicator",
  "blindSpotDetectionIndicator",
  "adaptiveCruiseControlIndicator",
  "pedestrianDetectionIndicator",
  "brakeAssistIndicator",
  "reverseCameraIndicator",
  "tirePressureMonitoringIndicator",
  "seatbeltReminderIndicator",
  "automaticLightingForwardTravelIndicator",
  "automaticHighBeamControlIndicator",
  "dangerousApproachDetectionIndicator",
  "trafficSignRecognitionIndicator",
  "twoWheeledVehicleDetection",
  "activeLaneControl",
  "automaticBrakingReverseTravel",
  "speedControlIsa",
  "emergencyBrakingPedestriansCyclists",
  "sideCollisionBlindSpot",
  "alcoLock",
];

export type SafetyMatrix = { present: DataCell[]; absent: DataCell[]; other: DataCell[] };

export type GroupId =
  | "identity"
  | "test"
  | "engine"
  | "safety"
  | "comfort"
  | "pollution"
  | "dimensions"
  | "colorTires"
  | "approvals";

export type FieldGroup = {
  id: GroupId;
  cells: DataCell[];
  safety?: { level: number | null; matrix: SafetyMatrix };
  pollution?: { group: number | null };
};

function testCountdown(vehicle: VehicleRecord): Pick<DataCell, "hint" | "hintTone"> {
  if (isOffRoad(vehicle)) return {};
  const date = parseGovDate(vehicle.testValidUntil);
  if (!date) return {};
  const days = daysFromToday(date);
  if (days < 0) return { hint: `פג לפני ${he.days(-days)}`, hintTone: "bad" };
  if (days === 0) return { hint: "פג היום", hintTone: "bad" };
  if (days <= thresholds.testExpiringSoonDays) return { hint: `בעוד ${he.days(days)}`, hintTone: "warn" };
  return { hint: `בעוד ${he.days(days)}`, hintTone: "ok" };
}

/** Month-only road entry dates (`yyyy-M`) give a month-precision end date, so it is shown as month + year. */
function warrantyCell(vehicle: VehicleRecord): DataCell | null {
  const active = vehicle.estimatedWarrantyActive;
  const monthOnly = parseGovDateDetailed(vehicle.roadEntryDate)?.monthOnly ?? false;
  return formattedCell(
    "estimatedWarrantyEndDate",
    vehicle.estimatedWarrantyEndDate,
    monthOnly ? formatMonthYear : formatDate,
    active == null
      ? {}
      : active
        ? { hint: he.warranty.active, hintTone: "ok" }
        : { hint: he.warranty.expired, hintTone: "muted" },
  );
}

/** Builds every data group. Groups without any cell are dropped. */
export function buildFieldGroups(vehicle: VehicleRecord): FieldGroup[] {
  const mm = vehicle.manufacturerModel ?? null;
  const used = new Set<string>(["extensionData", "id"]);

  const identity = compact([
    clean(vehicle.registrationNumber)
      ? {
          key: "registrationNumber",
          label: L.registrationNumber,
          value: formatPlate(vehicle.registrationNumber),
          ltr: true,
          copyable: true,
        }
      : null,
    textCell("chassisNumber", vehicle.chassisNumber, { ltr: true, mono: true, copyable: true }),
    textCell("engineNumber", clean(vehicle.engineNumber) ?? vehicle.history?.technical?.engineNumber, {
      ltr: true,
      mono: true,
    }),
    ...withCatalog(used, "manufacturerName", vehicle.manufacturerName, "manufacturerName", mm, t),
    ...withCatalog(used, "manufacturerCode", vehicle.manufacturerCode, "manufacturerCode", mm, t),
    ...mmCells(used, mm, ["manufacturerCountryName", "tozar"], t),
    ...withCatalog(used, "modelName", vehicle.modelName, "modelName", mm, t),
    ...withCatalog(used, "modelCode", vehicle.modelCode, "modelCode", mm, t),
    ...withCatalog(used, "modelType", vehicle.modelType, "modelType", mm, t),
    ...withCatalog(used, "trimLevel", vehicle.trimLevel, "trimLevel", mm, t),
    ...withCatalog(used, "commercialName", vehicle.commercialName, "commercialName", mm, t),
    t("manufactureYear", vehicle.manufactureYear),
    ...mmCells(used, mm, ["modelYear"], t),
    t("vehicleTypeName", vehicle.vehicleTypeName),
    t("vehicleTypeCode", vehicle.vehicleTypeCode),
    t("euVehicleTypeCode", vehicle.euVehicleTypeCode),
    t("euVehicleTypeName", vehicle.euVehicleTypeName),
    t("importType", vehicle.importType),
    t("originality", vehicle.originality),
    t("source", vehicle.source ? he.source[vehicle.source] : null),
  ]);

  const test = compact([
    dateCell("lastTestDate", vehicle.lastTestDate),
    dateCell("testValidUntil", vehicle.testValidUntil, testCountdown(vehicle)),
    dateCell("roadEntryDate", vehicle.roadEntryDate),
    warrantyCell(vehicle),
    t("registrationOrder", vehicle.registrationOrder),
    t("ownershipType", vehicle.ownershipType),
    dateCell("cancellationDate", vehicle.cancellationDate, { hintTone: "bad" }),
    t("cancellationReason", vehicle.cancellationReason),
    dateCell("updatedDate", vehicle.updatedDate),
  ]);

  const engine = compact([
    t("engineManufacturer", vehicle.engineManufacturer),
    t("engineModel", vehicle.engineModel),
    ...withCatalog(used, "fuelType", vehicle.fuelType, "fuelName", mm, t),
    ...mmCells(used, mm, ["fuelCode"], t),
    ...mmCells(used, mm, ["horsepower"], (k, v) => formattedCell(k, v, formatHp)),
    formattedCell("enginePowerKw", vehicle.enginePowerKw, (v) => withUnit(v, he.units.kw)),
    ...mmCells(used, mm, ["engineDisplacement"], (k, v) => formattedCell(k, v, formatCc)),
    ...mmCells(used, mm, ["driveName", "driveCode", "driveTechnologyName", "driveTechnologyCode"], t),
    ...mmCells(used, mm, ["automaticTransmissionIndicator"], b),
    ...withCatalog(used, "totalWeight", vehicle.totalWeight, "totalWeight", mm, kg),
    ...mmCells(used, mm, ["curbWeight"], kg),
  ]);

  // Safety
  const safetyLevel = withCatalog(
    used,
    "safetyEquipmentLevel",
    vehicle.safetyEquipmentLevel,
    "safetyEquipmentLevel",
    mm,
    t,
  );
  const levelNum = toNumber(clean(vehicle.safetyEquipmentLevel) ?? clean(mm?.safetyEquipmentLevel));
  const level =
    levelNum != null && Number.isInteger(levelNum) && levelNum >= 0 && levelNum <= thresholds.safetyLevelMax
      ? levelNum
      : null;
  const matrix: SafetyMatrix = { present: [], absent: [], other: [] };
  for (const key of SAFETY_SYSTEM_KEYS) {
    used.add(key);
    const cell = boolCell(key, mm?.[key]);
    if (!cell) continue;
    if (cell.bool === true) matrix.present.push(cell);
    else if (cell.bool === false) matrix.absent.push(cell);
    else matrix.other.push(cell);
  }
  const safetyCells = compact([
    ...safetyLevel,
    ...mmCells(used, mm, ["safetyScore"], t),
    ...mmCells(used, mm, ["airbagCount", "airbagsSource"], t),
  ]);
  const hasSafety = safetyCells.length + matrix.present.length + matrix.absent.length + matrix.other.length > 0;

  const comfort = compact([
    ...mmCells(used, mm, ["airConditioningIndicator", "powerSteeringIndicator"], b),
    ...mmCells(used, mm, ["powerWindowsSource", "powerWindowCount"], t),
    ...mmCells(used, mm, ["powerSunroofIndicator", "alloyWheelsIndicator", "cargoBoxIndicator"], b),
  ]);

  // Pollution
  used.add("pollutionGroup");
  const pollutionGroupRaw = clean(vehicle.pollutionGroup) ?? clean(mm?.pollutionGroup);
  const pollutionCells = compact([
    ...withCatalog(new Set(), "pollutionGroup", vehicle.pollutionGroup, "pollutionGroup", mm, t),
    ...mmCells(used, mm, ["greenScore"], t),
    ...mmCells(
      used,
      mm,
      [
        "co2Amount",
        "noxAmount",
        "pm10Amount",
        "hcAmount",
        "hcNoxAmount",
        "coAmount",
        "co2AmountCity",
        "noxAmountCity",
        "pm10AmountCity",
        "hcAmountCity",
        "coAmountCity",
        "co2AmountHighway",
        "noxAmountHighway",
        "pm10AmountHighway",
        "hcAmountHighway",
        "coAmountHighway",
        "co2Wltp",
        "hcWltp",
        "pmWltp",
        "noxWltp",
        "coWltp",
        "co2WltpNedc",
      ],
      gkm,
    ),
  ]);
  const pollutionGroupNum = toNumber(pollutionGroupRaw);
  const greenNum = toNumber(mm?.greenScore);
  const scaleValue =
    pollutionGroupNum != null && pollutionGroupNum >= 1 && pollutionGroupNum <= 15
      ? pollutionGroupNum
      : greenNum != null && greenNum >= 1 && greenNum <= 15
        ? greenNum
        : null;

  const dimensions = compact([
    ...mmCells(used, mm, ["bodyType"], t),
    ...mmCells(used, mm, ["doorCount"], t),
    ...withCatalog(used, "seatCount", vehicle.seatCount, "seatCount", mm, t),
    t("seatsBesideDriver", vehicle.seatsBesideDriver),
    t("axles", vehicle.axles),
    t("towHitch", vehicle.towHitch),
    ...mmCells(used, mm, ["height"], (k, v) => formattedCell(k, v, (x) => {
      const n = toNumber(x);
      return n == null ? clean(x) : formatNumber(n, 1);
    })),
    ...mmCells(used, mm, ["liftingLoadWeight", "towingCapacityWithBrakes", "towingCapacityWithoutBrakes"], kg),
  ]);

  const colorTires = compact([
    t("color", vehicle.color),
    t("colorCode", vehicle.colorCode),
    t("frontTire", vehicle.frontTire),
    t("rearTire", vehicle.rearTire),
  ]);

  const approvals = compact([
    ...mmCells(used, mm, ["feeGroupCode"], t),
    ...mmCells(used, mm, ["estimatedLicenseFee", "feeGroupPrice"], (k, v) =>
      formattedCell(k, v, (x) => {
        const n = toNumber(x);
        return n == null ? null : formatCurrency(n);
      }),
    ),
    ...mmCells(
      used,
      mm,
      [
        "homologationTypeName",
        "homologationTypeCode",
        "euTypeApproval",
        "converterTypeName",
        "converterTypeCode",
      ],
      t,
    ),
    ...mmCells(used, mm, ["batteryVoltageDg"], t),
    ...mmCells(
      used,
      mm,
      [
        "laneDepartureControlRegulationSource",
        "forwardDistanceMonitoringRegulationSource",
        "pedestrianDetectionRegulationSource",
        "automaticHighBeamControlRegulationSource",
        "trafficSignRecognitionRegulationSource",
      ],
      t,
    ),
  ]);

  // Safety net: any catalog field not placed above still gets shown.
  if (mm) {
    for (const [key, value] of Object.entries(mm)) {
      if (used.has(key)) continue;
      const cell = key.endsWith("Indicator") ? boolCell(key, value) : textCell(key, value);
      if (cell) approvals.push(cell);
    }
  }

  const groups: FieldGroup[] = [
    { id: "identity", cells: identity },
    { id: "test", cells: test },
    { id: "engine", cells: engine },
    {
      id: "safety",
      cells: safetyCells,
      safety: { level, matrix },
    },
    { id: "comfort", cells: comfort },
    { id: "pollution", cells: pollutionCells, pollution: { group: scaleValue } },
    { id: "dimensions", cells: dimensions },
    { id: "colorTires", cells: colorTires },
    { id: "approvals", cells: approvals },
  ];

  return groups.filter((g) => (g.id === "safety" ? hasSafety : g.cells.length > 0));
}

export function groupCellCount(group: FieldGroup): number {
  const m = group.safety?.matrix;
  return group.cells.length + (m ? m.present.length + m.absent.length + m.other.length : 0);
}