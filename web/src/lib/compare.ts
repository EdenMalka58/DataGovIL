import type { IconName } from "../components/Icon";
import { he } from "../strings.he";
import type { VehicleRecord } from "../types/vehicle";
import {
  clean,
  daysFromToday,
  formatAge,
  formatCc,
  formatCurrency,
  formatDate,
  formatDuration,
  formatHp,
  formatKm,
  formatMonthYear,
  formatNumber,
  formatPercent,
  parseGovDate,
  toBool,
  toNumber,
} from "./format";
import { formatPlate } from "./plate";
import {
  biggestImpactIndex,
  carAgeYears,
  classifyOwnership,
  isOffRoad,
  mileage,
  nonPrivateRowCount,
  ownerCount,
  recallCount,
} from "./viewModel";

export type CompareSection = keyof typeof he.compare.sections;
type Reason = keyof typeof he.compare.reasons;

export type CellIcon = { name: IconName; label: string; tone: "ok" };

type Extracted = { display: string | null; score?: number | null; icon?: CellIcon };

type InsightContext = {
  vehicles: VehicleRecord[];
  extracted: Extracted[];
  /** Short per-vehicle names for insight sentences. */
  names: string[];
};

type RowDef = {
  id: string;
  section: CompareSection;
  label: string;
  get: (v: VehicleRecord) => Extracted;
  /** Which direction wins; omitted = no winner for this row. */
  better?: "high" | "low";
  weight?: number;
  reason?: Reason;
  /** One-line takeaway across the compared vehicles; only called with 2+ vehicles. */
  insight?: (ctx: InsightContext) => string | null;
};

export type CompareRow = {
  id: string;
  section: CompareSection;
  label: string;
  values: (string | null)[];
  icons: (CellIcon | null)[];
  winners: Set<number>;
  differs: boolean;
  insight: string | null;
};

const R = he.compare.rows;
const I = he.compare.insights;
const mm = (v: VehicleRecord) => v.manufacturerModel ?? undefined;

type Scored = { i: number; s: number };

function scoredOf(extracted: Extracted[]): Scored[] {
  return extracted
    .map((e, i) => ({ i, s: e.score }))
    .filter((x): x is Scored => x.s != null && Number.isFinite(x.s));
}

/** Model name when it is unique among the compared vehicles, otherwise the plate. */
function shortNames(vehicles: VehicleRecord[]): string[] {
  const models = vehicles.map((v) => clean(v.commercialName) ?? clean(v.modelName) ?? clean(v.manufacturerName));
  return vehicles.map((v, i) => {
    const model = models[i];
    return model && models.filter((m) => m === model).length === 1 ? model : formatPlate(v.registrationNumber);
  });
}

/**
 * Cheapest vs. most expensive among vehicles with a value. Differences under 1 ₪ count as equal.
 * Scores are ILS amounts.
 */
function costInsight(
  differs: (ctx: InsightContext, low: Scored, high: Scored, diff: number) => string,
  same: (ctx: InsightContext, value: number) => string,
) {
  return (ctx: InsightContext): string | null => {
    const scored = scoredOf(ctx.extracted);
    if (scored.length < 2) return null;
    const low = scored.reduce((a, b) => (b.s < a.s ? b : a));
    const high = scored.reduce((a, b) => (b.s > a.s ? b : a));
    const diff = Math.round(high.s - low.s);
    return diff < 1 ? same(ctx, low.s) : differs(ctx, low, high, diff);
  };
}

const DAYS_PER_MONTH = 30.44;
const WARRANTY_MONTHS = 36;

/** Comparison entries saved before the API sent the end date only have the road entry date. */
function warrantyEnd(v: VehicleRecord): Date | null {
  const end = parseGovDate(v.estimatedWarrantyEndDate);
  if (end) return end;
  const entry = parseGovDate(v.roadEntryDate);
  return entry ? new Date(entry.getFullYear(), entry.getMonth() + WARRANTY_MONTHS, entry.getDate()) : null;
}

function warrantyInsight({ extracted, names }: InsightContext): string | null {
  const scored = scoredOf(extracted);
  if (scored.length < 2) return null;
  const months = (days: number) => Math.floor(days / DAYS_PER_MONTH);
  const active = scored.filter((x) => x.s > 0).sort((a, b) => b.s - a.s);

  let text: string;
  if (active.length === 0) {
    text = I.warrantyNone;
  } else if (active.length === 1) {
    text = I.warrantyOnly(names[active[0].i], formatDuration(months(active[0].s)));
  } else {
    const [best, next] = active;
    const diff = months(best.s) - months(next.s);
    text =
      diff < 1
        ? I.warrantySame
        : I.warrantyLonger(names[best.i], formatDuration(months(best.s)), names[next.i], formatDuration(diff));
  }
  return `${text} ${I.warrantyNote}`;
}

function flagRow(
  id: string,
  label: string,
  pick: (v: VehicleRecord) => boolean | null | undefined,
  weight: number,
): RowDef {
  return {
    id,
    section: "flags",
    label,
    better: "high",
    weight,
    reason: "flags",
    get: (v) => {
      const value = v.history?.technical ? pick(v) : undefined;
      if (value === true) return { display: he.history.documented, score: 0 };
      if (value === false) return { display: he.history.notDocumented, score: 1 };
      return { display: v.history?.technical ? he.history.unknown : null, score: null };
    },
  };
}

function boolRow(id: string, label: string, pick: (v: VehicleRecord) => unknown): RowDef {
  return {
    id,
    section: "safety",
    label,
    better: "high",
    weight: 0.5,
    reason: "safety",
    get: (v) => {
      const b = toBool(pick(v));
      if (b == null) return { display: clean(pick(v)), score: null };
      return { display: b ? he.common.yes : he.common.no, score: b ? 1 : 0 };
    },
  };
}

function numRow(
  id: string,
  section: CompareSection,
  label: string,
  pick: (v: VehicleRecord) => unknown,
  opts: Partial<RowDef> & { format?: (n: number) => string } = {},
): RowDef {
  return {
    id,
    section,
    label,
    ...opts,
    get: (v) => {
      const raw = pick(v);
      const n = toNumber(raw);
      if (n == null) return { display: clean(raw), score: null };
      return { display: opts.format ? opts.format(n) : formatNumber(n), score: n };
    },
  };
}

function lpgStatus(value: boolean | null | undefined): string | null {
  return value === true ? he.history.lpgInstalled : value === false ? he.history.notDocumented : null;
}

function textRow(id: string, section: CompareSection, label: string, pick: (v: VehicleRecord) => unknown): RowDef {
  return { id, section, label, get: (v) => ({ display: clean(pick(v)) }) };
}

const ROWS: RowDef[] = [
  {
    id: "status",
    section: "status",
    label: R.status,
    better: "high",
    weight: 5,
    reason: "status",
    get: (v) =>
      v.isPermanentlyCancelled
        ? { display: he.compare.statusCancelled, score: 0 }
        : v.isInactive
          ? { display: he.compare.statusInactive, score: 0 }
          : { display: he.compare.statusActive, score: 1 },
  },
  {
    id: "testUntil",
    section: "status",
    label: R.testUntil,
    better: "high",
    weight: 1,
    reason: "test",
    get: (v) => {
      if (isOffRoad(v)) return { display: null, score: null };
      const date = parseGovDate(v.testValidUntil);
      return { display: formatDate(v.testValidUntil), score: date ? daysFromToday(date) : null };
    },
  },
  {
    id: "recalls",
    section: "status",
    label: R.recalls,
    better: "low",
    weight: 3,
    reason: "recalls",
    get: (v) => {
      const n = recallCount(v);
      return { display: n === 0 ? he.compare.none : String(n), score: n };
    },
  },
  {
    id: "estimatedValue",
    section: "value",
    label: R.estimatedValue,
    get: (v) => ({ display: v.depreciation?.estimatedValue != null ? formatCurrency(v.depreciation.estimatedValue) : null }),
  },
  {
    id: "listPrice",
    section: "value",
    label: R.listPrice,
    get: (v) => ({ display: v.depreciation?.listPrice != null ? formatCurrency(v.depreciation.listPrice) : null }),
  },
  {
    id: "depreciation",
    section: "value",
    label: R.depreciation,
    better: "high",
    weight: 1,
    reason: "depreciation",
    get: (v) =>
      v.depreciation
        ? { display: formatPercent(v.depreciation.depreciationPercent), score: v.depreciation.depreciationPercent }
        : { display: null },
  },
  {
    id: "topFactor",
    section: "value",
    label: R.topFactor,
    get: (v) => {
      const lines = v.depreciation?.lines ?? [];
      const i = biggestImpactIndex(lines);
      if (i < 0) return { display: null };
      const percent = formatPercent(lines[i].percent);
      return { display: `${he.enums.factor[lines[i].factor] ?? lines[i].factor} (${percent})` };
    },
  },
  numRow("year", "age", R.year, (v) => v.manufactureYear, {
    better: "high",
    weight: 1,
    reason: "year",
    format: (n) => String(n),
  }),
  {
    id: "age",
    section: "age",
    label: R.age,
    better: "low",
    get: (v) => {
      const age = carAgeYears(v);
      return age == null ? { display: null } : { display: formatAge(age), score: age };
    },
  },
  {
    id: "km",
    section: "age",
    label: R.km,
    better: "low",
    weight: 2,
    reason: "km",
    get: (v) => {
      const m = mileage(v);
      return m ? { display: formatKm(m.km), score: m.km } : { display: null };
    },
  },
  {
    id: "kmPerYear",
    section: "age",
    label: R.kmPerYear,
    better: "low",
    get: (v) => {
      const m = mileage(v);
      return m?.perYear != null
        ? { display: formatKm(Math.round(m.perYear / 100) * 100), score: m.perYear }
        : { display: null };
    },
  },
  {
    id: "owners",
    section: "owners",
    label: R.owners,
    better: "low",
    weight: 2,
    reason: "owners",
    get: (v) => {
      const n = ownerCount(v);
      return n == null ? { display: null } : { display: String(n), score: n };
    },
  },
  {
    id: "currentOwnership",
    section: "owners",
    label: R.currentOwnership,
    better: "high",
    weight: 1,
    reason: "currentOwnership",
    get: (v) => {
      const s = clean(v.ownershipType);
      if (!s) return { display: null };
      return { display: s, score: classifyOwnership(s) === "private" ? 1 : 0 };
    },
  },
  {
    id: "nonPrivate",
    section: "owners",
    label: R.nonPrivate,
    better: "low",
    weight: 2,
    reason: "nonPrivate",
    get: (v) => {
      if (!v.history?.ownershipHistory?.length) return { display: null };
      const n = nonPrivateRowCount(v);
      return { display: n === 0 ? he.compare.none : String(n), score: n };
    },
  },
  flagRow("structure", R.structure, (v) => v.history?.technical?.structureChangeIndicator, 3),
  flagRow("color", R.color, (v) => v.history?.technical?.colorChangeIndicator, 1),
  flagRow("tires", R.tires, (v) => v.history?.technical?.tireChangeIndicator, 1),
  textRow("originality", "origin", R.originality, (v) => v.history?.technical?.originalityName ?? v.depreciation?.originality),
  textRow("importType", "origin", R.importType, (v) => v.importType),
  numRow("safetyScore", "safety", R.safetyScore, (v) => mm(v)?.safetyScore, { better: "high", weight: 2, reason: "safety" }),
  numRow("safetyLevel", "safety", R.safetyLevel, (v) => v.safetyEquipmentLevel ?? mm(v)?.safetyEquipmentLevel, {
    better: "high",
    weight: 1,
    reason: "safety",
  }),
  boolRow("abs", R.abs, (v) => mm(v)?.absIndicator),
  numRow("airbags", "safety", R.airbags, (v) => mm(v)?.airbagCount, { better: "high", weight: 0.5, reason: "safety" }),
  boolRow("stability", R.stability, (v) => mm(v)?.stabilityControlIndicator),
  boolRow("emergencyBraking", R.emergencyBraking, (v) => mm(v)?.emergencyBrakingPedestriansCyclists),
  boolRow("laneKeeping", R.laneKeeping, (v) => mm(v)?.laneDepartureControlIndicator ?? mm(v)?.activeLaneControl),
  boolRow("reverseCamera", R.reverseCamera, (v) => mm(v)?.reverseCameraIndicator),
  numRow("estimatedLicenseFee", "cost", R.estimatedLicenseFee, (v) => mm(v)?.estimatedLicenseFee, {
    better: "low",
    weight: 0.5,
    reason: "licenseFee",
    format: formatCurrency,
    insight: costInsight(
      ({ names }, low, high, diff) => I.licenseFee(names[low.i], names[high.i], formatCurrency(diff)),
      (_, fee) => I.licenseFeeSame(formatCurrency(fee)),
    ),
  }),
  // Lower is only better for company-car users (less income tax), so it marks a winner without scoring points.
  numRow("feeGroupPrice", "cost", R.feeGroupPrice, (v) => mm(v)?.feeGroupPrice, {
    better: "low",
    format: formatCurrency,
    insight: costInsight(
      ({ names }, low, high, diff) =>
        I.usageValue(names[low.i], names[high.i], formatCurrency(diff), formatCurrency(diff * 12)),
      (_, value) => I.usageValueSame(formatCurrency(value)),
    ),
  }),
  {
    id: "energyCost",
    section: "cost",
    label: R.energyCost,
    better: "low",
    weight: 1,
    reason: "energyCost",
    get: (v) => {
      const cost = v.energyCost?.monthlyCost;
      return cost == null ? { display: null } : { display: formatCurrency(Math.round(cost)), score: cost };
    },
    insight: costInsight(
      ({ names, vehicles }, low, high, diff) =>
        I.energyCost(
          names[low.i],
          names[high.i],
          formatCurrency(diff),
          formatCurrency(diff * 12),
          formatKm(vehicles[low.i].energyCost?.kmPerMonth) ?? "",
        ),
      ({ vehicles, extracted }, value) => {
        const i = extracted.findIndex((e) => e.score != null);
        return I.energyCostSame(formatCurrency(Math.round(value)), formatKm(vehicles[i].energyCost?.kmPerMonth) ?? "");
      },
    ),
  },
  {
    id: "warranty",
    section: "cost",
    label: R.warranty,
    better: "high",
    weight: 1,
    reason: "warranty",
    get: (v) => {
      const end = warrantyEnd(v);
      if (!end) return { display: null };
      const days = daysFromToday(end);
      const month = formatMonthYear(`${end.getFullYear()}-${end.getMonth() + 1}`) ?? "";
      const active = days >= 0;
      return {
        display: active ? he.compare.warrantyUntil(month) : he.compare.warrantyEnded(month),
        score: active ? Math.max(days, 0) : 0,
        icon: active ? { name: "shield", label: he.compare.warrantyActiveAria, tone: "ok" } : undefined,
      };
    },
    insight: warrantyInsight,
  },
  numRow("feeGroup", "cost", R.feeGroup, (v) => mm(v)?.feeGroupCode, { better: "low" }),
  numRow("pollutionGroup", "cost", R.pollutionGroup, (v) => v.pollutionGroup ?? mm(v)?.pollutionGroup, {
    better: "low",
    weight: 1,
    reason: "pollution",
  }),
  numRow("greenScore", "cost", R.greenScore, (v) => mm(v)?.greenScore, { better: "low" }),
  textRow("fuel", "cost", R.fuel, (v) => v.fuelType ?? mm(v)?.fuelName),
  textRow("driveTechnology", "cost", R.driveTechnology, (v) => mm(v)?.driveTechnologyName),
  textRow("lpg", "cost", R.lpg, (v) => lpgStatus(v.history?.technical?.lpgChangeIndicator)),
  {
    id: "displacement",
    section: "cost",
    label: R.displacement,
    get: (v) => ({ display: clean(mm(v)?.engineDisplacement) ? formatCc(mm(v)?.engineDisplacement) : null }),
  },
  {
    id: "horsepower",
    section: "cost",
    label: R.horsepower,
    get: (v) => ({ display: clean(mm(v)?.horsepower) ? formatHp(mm(v)?.horsepower) : null }),
  },
  {
    id: "transmission",
    section: "spec",
    label: R.transmission,
    get: (v) => {
      const b = toBool(mm(v)?.automaticTransmissionIndicator);
      return { display: b == null ? null : b ? he.compare.automatic : he.compare.manual };
    },
  },
  textRow("drive", "spec", R.drive, (v) => mm(v)?.driveName),
  textRow("seats", "spec", R.seats, (v) => mm(v)?.seatCount),
  textRow("doors", "spec", R.doors, (v) => mm(v)?.doorCount),
];

export type CompareModel = {
  rows: CompareRow[];
  points: number[];
  best: number | null;
  reasons: string[];
  excluded: boolean[];
};

export function buildComparison(vehicles: VehicleRecord[]): CompareModel {
  const points = vehicles.map(() => 0);
  const wonReasons: Map<Reason, number>[] = vehicles.map(() => new Map());
  const rows: CompareRow[] = [];
  const names = shortNames(vehicles);

  for (const def of ROWS) {
    const extracted = vehicles.map((v) => def.get(v));
    const values = extracted.map((e) => e.display);
    if (values.every((x) => x == null)) continue;

    const winners = new Set<number>();
    if (def.better && vehicles.length > 1) {
      const scored = scoredOf(extracted);
      if (scored.length >= 2) {
        const best = def.better === "high" ? Math.max(...scored.map((x) => x.s)) : Math.min(...scored.map((x) => x.s));
        const allEqual = scored.every((x) => x.s === best);
        if (!allEqual) scored.filter((x) => x.s === best).forEach((x) => winners.add(x.i));
      }
    }

    for (const i of winners) {
      points[i] += def.weight ?? 0;
      if (def.reason && def.weight) {
        const map = wonReasons[i];
        map.set(def.reason, (map.get(def.reason) ?? 0) + def.weight);
      }
    }

    const distinct = new Set(values.map((x) => x ?? "—"));
    const insight = def.insight && vehicles.length > 1 ? def.insight({ vehicles, extracted, names }) : null;
    rows.push({
      id: def.id,
      section: def.section,
      label: def.label,
      values,
      icons: extracted.map((e) => e.icon ?? null),
      winners,
      differs: distinct.size > 1,
      insight,
    });
  }

  const excluded = vehicles.map((v) => isOffRoad(v));
  let best: number | null = null;
  if (vehicles.length > 1) {
    const eligible = points.map((p, i) => ({ p, i })).filter((x) => !excluded[x.i]);
    if (eligible.length > 0) {
      const top = Math.max(...eligible.map((x) => x.p));
      const leaders = eligible.filter((x) => x.p === top);
      if (leaders.length === 1 && top > 0) best = leaders[0].i;
    }
  }

  const reasons =
    best == null
      ? []
      : [...wonReasons[best].entries()]
          .sort((a, b) => b[1] - a[1])
          .slice(0, 4)
          .map(([r]) => he.compare.reasons[r]);

  return { rows, points: points.map((p) => Math.round(p * 10) / 10), best, reasons, excluded };
}
