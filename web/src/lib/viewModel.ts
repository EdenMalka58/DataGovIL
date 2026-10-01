import { he } from "../strings.he";
import type { VehicleOwnershipHistoryRecord, VehicleRecord } from "../types/vehicle";
import {
  clean,
  daysFromToday,
  formatDate,
  formatNumber,
  monthsBetween,
  parseGovDate,
  todayInJerusalem,
  toNumber,
} from "./format";

/** Tunable buyer-logic thresholds. Keep every magic number here. */
export const thresholds = {
  /** Days before test expiry that trigger the "expiring soon" state. */
  testExpiringSoonDays: 30,
  /** Owner count (including the current one) that triggers a warning. */
  manyOwners: 4,
  /** National average annual mileage (km/year) used for comparisons. */
  nationalAvgKmPerYear: 15_000,
  /** Above this km/year the mileage is flagged as high. */
  highKmPerYear: 22_000,
  /** Below this km/year (for cars older than `lowKmMinAgeYears`) the mileage is flagged for a check. */
  lowKmPerYear: 5_000,
  lowKmMinAgeYears: 3,
  /** Minimum age (years) for a per-year average to be meaningful. */
  minAgeForAverage: 0.5,
  /** Top of the safety equipment level scale (רמת אבזור בטיחותי, 0–8). */
  safetyLevelMax: 8,
};

// ─── Ownership classification ─────────────────────────────────────────────

export type OwnershipKind =
  | "private"
  | "commercial"
  | "rental"
  | "taxi"
  | "government"
  | "public"
  | "kibbutz"
  | "other";

export function classifyOwnership(value: string | null | undefined): OwnershipKind | null {
  const s = clean(value);
  if (!s) return null;
  if (s.includes("פרטי")) return "private";
  if (s.includes("השכר") || s.includes("ליסינג") || s.includes("החכר")) return "rental";
  if (s.includes("מונית") || s.includes("סיור")) return "taxi";
  if (s.includes("ממשל") || s.includes("מדינה") || s.includes("צה\"ל") || s.includes("משטרה"))
    return "government";
  if (s.includes("ציבור")) return "public";
  if (s.includes("קיבוץ") || s.includes("מושב")) return "kibbutz";
  if (s.includes("חברה") || s.includes("מסחרי") || s.includes("סוחר") || s.includes("עסק"))
    return "commercial";
  return "other";
}

/** Color family used in the timeline: private = blue, commercial-ish = orange, public/gov = gray. */
export function ownershipTone(kind: OwnershipKind | null): "private" | "commercial" | "public" {
  if (kind === "private" || kind == null) return "private";
  if (kind === "government" || kind === "public" || kind === "other") return "public";
  return "commercial";
}

export function isNonPrivate(kind: OwnershipKind | null): boolean {
  return kind != null && kind !== "private";
}

// ─── Vehicle kind ─────────────────────────────────────────────────────────

export type VehicleKind =
  | "car"
  | "vehicle"
  | "van"
  | "truck"
  | "bus"
  | "minibus"
  | "taxi"
  | "motorcycle"
  | "scooter"
  | "tractor"
  | "trailer";

/** Vehicles over this total weight (kg) count as trucks rather than light commercial. */
const HEAVY_WEIGHT_KG = 3500;

/**
 * Classifies from the registry's vehicle type text, the EU category (M/N/L/O), the source registry,
 * the private/commercial model type (`sug_degem`), and finally the ownership type.
 */
export function classifyVehicleKind(vehicle: VehicleRecord): VehicleKind {
  const type = clean(vehicle.vehicleTypeName) ?? "";
  const eu = (clean(vehicle.euVehicleTypeCode) ?? clean(vehicle.manufacturerModel?.euTypeApproval) ?? "").toUpperCase();
  const modelType = (clean(vehicle.modelType) ?? clean(vehicle.manufacturerModel?.modelType) ?? "").toUpperCase();
  const weight = toNumber(vehicle.totalWeight) ?? toNumber(vehicle.manufacturerModel?.totalWeight);

  if (type.includes("קטנוע")) return "scooter";
  if (type.includes("אופנוע") || eu.startsWith("L") || vehicle.source === "TwoWheeled") return "motorcycle";
  if (type.includes("גרור") || eu.startsWith("O")) return "trailer";
  if (type.includes("טרקטור")) return "tractor";
  if (type.includes("זוטובוס") || type.includes("זעיר") || eu === "M2") return "minibus";
  if (type.includes("אוטובוס") || eu === "M3") return "bus";
  if (type.includes("מונית")) return "taxi";
  if (type.includes("משא") || eu === "N2" || eu === "N3") return "truck";
  if (type.includes("מסחרי") || modelType === "M" || eu === "N1") {
    return weight != null && weight > HEAVY_WEIGHT_KG ? "truck" : "van";
  }
  if (classifyOwnership(vehicle.ownershipType) === "taxi") return "taxi";
  if (type.includes("פרטי") || modelType === "P" || eu === "M1") return "car";
  return "vehicle";
}

// ─── Status ───────────────────────────────────────────────────────────────

export type StatusKind = "cancelled" | "inactive" | "testExpired" | "testSoon" | "recall" | "ok";
export type Tone = "ok" | "warn" | "bad" | "info";

export type StatusItem = {
  kind: StatusKind;
  tone: Tone;
  title: string;
  text: string;
  anchor?: string;
};

export type TestInfo = { date: Date; days: number; formatted: string } | null;

export function testInfo(vehicle: VehicleRecord): TestInfo {
  const date = parseGovDate(vehicle.testValidUntil);
  if (!date) return null;
  return { date, days: daysFromToday(date), formatted: formatDate(vehicle.testValidUntil) ?? "" };
}

export function recallCount(vehicle: VehicleRecord): number {
  return Array.isArray(vehicle.recalls) ? vehicle.recalls.length : 0;
}

/** True when the vehicle is off the road — test/value widgets no longer apply. */
export function isOffRoad(vehicle: VehicleRecord): boolean {
  return vehicle.isPermanentlyCancelled === true || vehicle.isInactive === true;
}

/** The valuation needs a price-list row; without one there is no value to show. */
export function hasListPrice(vehicle: VehicleRecord): boolean {
  const price = vehicle.depreciation?.listPrice;
  return price != null && Number.isFinite(price);
}

/** The fleet row for this vehicle's manufacture year, when the model code has one. */
export function vehicleYearFleet(vehicle: VehicleRecord) {
  const year = toNumber(vehicle.manufactureYear);
  if (year == null) return null;
  return vehicle.modelFleet?.years.find((row) => row.modelYear === year) ?? null;
}

/** Statuses in priority order; the first one is the main banner. Always returns ≥ 1 item. */
export function computeStatuses(vehicle: VehicleRecord): StatusItem[] {
  const items: StatusItem[] = [];
  const s = he.status;

  if (vehicle.isPermanentlyCancelled) {
    const date = formatDate(vehicle.cancellationDate);
    items.push({
      kind: "cancelled",
      tone: "bad",
      title: s.cancelledTitle,
      text: date ? s.cancelledSince(date) : s.cancelledNoDate,
    });
  }
  if (vehicle.isInactive) {
    items.push({ kind: "inactive", tone: "warn", title: s.inactiveTitle, text: s.inactiveText });
  }

  if (!isOffRoad(vehicle)) {
    const test = testInfo(vehicle);
    if (test && test.days < 0) {
      items.push({
        kind: "testExpired",
        tone: "bad",
        title: s.testExpiredTitle(-test.days),
        text: s.testExpiredText(test.formatted),
        anchor: "group-test",
      });
    } else if (test && test.days <= thresholds.testExpiringSoonDays) {
      items.push({
        kind: "testSoon",
        tone: "warn",
        title: s.testSoonTitle(test.days),
        text: s.testSoonText(test.formatted),
        anchor: "group-test",
      });
    }
  }

  const recalls = recallCount(vehicle);
  if (recalls > 0) {
    items.push({
      kind: "recall",
      tone: "bad",
      title: s.recallTitle(recalls),
      text: s.recallText,
      anchor: "recalls",
    });
  }

  if (items.length === 0) {
    const test = testInfo(vehicle);
    items.push({
      kind: "ok",
      tone: "ok",
      title: s.okTitle,
      text: test ? s.okTestUntil(test.formatted) : s.okNoTest,
    });
  }
  return items;
}

/** A record found only in the safety-discount list carries no registration data. */
export function isPartialRecord(vehicle: VehicleRecord): boolean {
  return (
    vehicle.isSafetyDiscountEligible === true &&
    !clean(vehicle.manufacturerName) &&
    !clean(vehicle.modelName) &&
    !clean(vehicle.chassisNumber) &&
    !clean(vehicle.manufactureYear)
  );
}

// ─── Age & mileage ────────────────────────────────────────────────────────

export function carAgeYears(vehicle: VehicleRecord): number | null {
  if (vehicle.depreciation && Number.isFinite(vehicle.depreciation.carAge) && vehicle.depreciation.carAge > 0) {
    return vehicle.depreciation.carAge;
  }
  const start =
    parseGovDate(vehicle.roadEntryDate) ?? parseGovDate(vehicle.history?.technical?.firstRegistrationDate);
  if (start) return Math.max(0, monthsBetween(start, todayInJerusalem()) / 12);
  const year = toNumber(vehicle.manufactureYear);
  if (year) return Math.max(0, todayInJerusalem().getFullYear() - year + 0.5);
  return null;
}

export function kilometers(vehicle: VehicleRecord): number | null {
  const fromDep = vehicle.depreciation?.kilometers;
  if (fromDep != null && Number.isFinite(fromDep)) return fromDep;
  return toNumber(vehicle.history?.technical?.lastTestOdometer);
}

export type MileageLevel = "high" | "low" | "normal";
export type Mileage = { km: number; age: number | null; perYear: number | null; level: MileageLevel | null };

export function mileage(vehicle: VehicleRecord): Mileage | null {
  const km = kilometers(vehicle);
  if (km == null) return null;
  const age = carAgeYears(vehicle);
  const perYear = age != null && age >= thresholds.minAgeForAverage ? km / age : null;
  let level: MileageLevel | null = null;
  if (perYear != null) {
    if (perYear > thresholds.highKmPerYear) level = "high";
    else if (perYear < thresholds.lowKmPerYear && (age ?? 0) >= thresholds.lowKmMinAgeYears) level = "low";
    else level = "normal";
  }
  return { km, age, perYear, level };
}

// ─── Ownership timeline ───────────────────────────────────────────────────

export type TimelineNode = {
  key: string;
  raw: VehicleOwnershipHistoryRecord;
  date: Date | null;
  kind: OwnershipKind | null;
  tone: "private" | "commercial" | "public";
  durationMonths: number | null;
  isCurrent: boolean;
};

export function ownershipRows(vehicle: VehicleRecord): VehicleOwnershipHistoryRecord[] {
  return Array.isArray(vehicle.history?.ownershipHistory) ? vehicle.history!.ownershipHistory! : [];
}

/** Newest → oldest. Keeps arrival order when the yyyyMM values aren't all sortable. */
export function ownershipTimeline(vehicle: VehicleRecord): TimelineNode[] {
  const rows = ownershipRows(vehicle);
  const sortable = rows.length > 0 && rows.every((r) => /^\d{6}/.test(clean(r.ownershipYearMonth) ?? ""));
  const ordered = sortable
    ? [...rows].sort((a, b) =>
        String(clean(b.ownershipYearMonth)).localeCompare(String(clean(a.ownershipYearMonth))),
      )
    : rows;

  const today = todayInJerusalem();
  return ordered.map((raw, index) => {
    const date = parseGovDate(raw.ownershipYearMonth);
    const newer = index === 0 ? today : parseGovDate(ordered[index - 1].ownershipYearMonth);
    const durationMonths = sortable && date && newer ? Math.max(0, monthsBetween(date, newer)) : null;
    const kind = classifyOwnership(raw.ownershipType);
    return {
      key: `${raw.id ?? ""}-${index}`,
      raw,
      date,
      kind,
      tone: ownershipTone(kind),
      durationMonths,
      isCurrent: index === 0 && sortable,
    };
  });
}

export function ownerCount(vehicle: VehicleRecord): number | null {
  const rows = ownershipRows(vehicle).length;
  if (rows > 0) return rows;
  const dep = vehicle.depreciation?.ownerCount;
  return dep != null && dep > 0 ? dep : null;
}

/** Distinct non-private ownership kinds across history and the current ownership. */
export function nonPrivateKinds(vehicle: VehicleRecord): OwnershipKind[] {
  const kinds = new Set<OwnershipKind>();
  for (const row of ownershipRows(vehicle)) {
    const k = classifyOwnership(row.ownershipType);
    if (isNonPrivate(k)) kinds.add(k!);
  }
  const current = classifyOwnership(vehicle.ownershipType);
  if (isNonPrivate(current)) kinds.add(current!);
  return [...kinds];
}

export function nonPrivateRowCount(vehicle: VehicleRecord): number {
  return ownershipRows(vehicle).filter((r) => isNonPrivate(classifyOwnership(r.ownershipType))).length;
}

// ─── Buyer summary ────────────────────────────────────────────────────────

export type Verdict = "ok" | "warn" | "bad";
export type SummaryFlag = { id: string; tone: Tone; text: string; anchor?: string; bold?: boolean };

export function buyerSummary(vehicle: VehicleRecord): { verdict: Verdict; flags: SummaryFlag[] } {
  const f = he.summary.flags;
  const flags: SummaryFlag[] = [];
  const offRoad = isOffRoad(vehicle);

  if (vehicle.isPermanentlyCancelled) flags.push({ id: "cancelled", tone: "bad", text: f.cancelled, anchor: "status" });
  if (vehicle.isInactive) flags.push({ id: "inactive", tone: "bad", text: f.inactive, anchor: "status" });
  if (vehicle.isSafetyDiscountEligible === true) {
    flags.push({ id: "safetyDiscount", tone: "ok", text: f.safetyDiscount, bold: true });
  }

  const recalls = recallCount(vehicle);
  flags.push(
    recalls > 0
      ? { id: "recalls", tone: "bad", text: f.recalls(recalls), anchor: "recalls" }
      : { id: "recalls", tone: "ok", text: f.noRecalls },
  );

  const tech = vehicle.history?.technical;
  if (tech) {
    if (tech.structureChangeIndicator === true)
      flags.push({ id: "structure", tone: "bad", text: f.structure, anchor: "history" });
    if (tech.lpgChangeIndicator === true) flags.push({ id: "lpg", tone: "info", text: f.lpg, anchor: "history" });
    if (tech.colorChangeIndicator === true) flags.push({ id: "color", tone: "warn", text: f.color, anchor: "history" });
    if (tech.tireChangeIndicator === true) flags.push({ id: "tires", tone: "warn", text: f.tires, anchor: "history" });
  } else if (!isPartialRecord(vehicle)) {
    flags.push({ id: "technical", tone: "info", text: f.noTechnical });
  }

  const owners = ownerCount(vehicle);
  if (owners != null) {
    flags.push({
      id: "owners",
      tone: owners >= thresholds.manyOwners ? "warn" : "ok",
      text: f.owners(owners),
      anchor: owners >= thresholds.manyOwners ? "timeline" : undefined,
    });
  }

  const nonPrivate = nonPrivateKinds(vehicle);
  if (nonPrivate.length > 0) {
    flags.push({
      id: "nonPrivate",
      tone: "warn",
      text: f.nonPrivate(nonPrivate.map((k) => he.ownership[k]).join(", ")),
      anchor: "timeline",
    });
  } else if (owners != null && ownershipRows(vehicle).length > 0) {
    flags.push({ id: "nonPrivate", tone: "ok", text: f.privateOnly });
  }

  const m = mileage(vehicle);
  if (m?.perYear != null) {
    const perYear = formatNumber(Math.round(m.perYear / 100) * 100, 0);
    if (m.level === "high") flags.push({ id: "km", tone: "warn", text: f.highKm(perYear), anchor: "odometer" });
    else if (m.level === "low") flags.push({ id: "km", tone: "info", text: f.lowKm(perYear), anchor: "odometer" });
    else flags.push({ id: "km", tone: "ok", text: f.normalKm(perYear) });
  }

  if (!offRoad) {
    const test = testInfo(vehicle);
    if (test && test.days < 0) flags.push({ id: "test", tone: "warn", text: f.testExpired, anchor: "group-test" });
    else if (test && test.days <= thresholds.testExpiringSoonDays)
      flags.push({ id: "test", tone: "info", text: f.testSoon(test.days), anchor: "group-test" });
    else if (test) flags.push({ id: "test", tone: "ok", text: f.testValid(test.formatted) });
  }

  const order: Record<Tone, number> = { bad: 0, warn: 1, info: 2, ok: 3 };
  flags.sort((a, b) => order[a.tone] - order[b.tone]);

  const verdict: Verdict = flags.some((x) => x.tone === "bad")
    ? "bad"
    : flags.some((x) => x.tone === "warn")
      ? "warn"
      : "ok";
  return { verdict, flags };
}

// ─── Depreciation helpers ─────────────────────────────────────────────────

/** Index of the line with the largest absolute percent, or -1. */
export function biggestImpactIndex(lines: { percent: number }[]): number {
  let best = -1;
  let bestAbs = 0;
  lines.forEach((line, i) => {
    const abs = Math.abs(line.percent);
    if (abs > bestAbs) {
      bestAbs = abs;
      best = i;
    }
  });
  return best;
}

/** Strips identifying fields before anything is persisted in the browser. */
export function stripIdentifying(vehicle: VehicleRecord): VehicleRecord {
  const copy: VehicleRecord = { ...vehicle, chassisNumber: undefined, engineNumber: undefined };
  if (copy.history?.technical) {
    copy.history = { ...copy.history, technical: { ...copy.history.technical, engineNumber: undefined } };
  }
  return copy;
}
