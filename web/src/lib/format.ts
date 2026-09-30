import { he } from "../strings.he";

/** U+2212 — used instead of "-" next to numbers so it doesn't jump in RTL text. */
export const MINUS = "\u2212";

const LRI = "\u2066";
const PDI = "\u2069";

/** Wraps a signed number in an LTR isolate so its sign stays put while a Hebrew unit / ₪ follows in RTL order. */
function isolate(s: string): string {
  return `${LRI}${s}${PDI}`;
}

/** True for values to render in an LTR container (VIN, codes, Latin names) — no Hebrew and not already isolated. */
export function needsLtr(value: string): boolean {
  return !/[\u0590-\u05FF\u2066]/.test(value);
}

export function hasValue(value: unknown): boolean {
  if (value == null) return false;
  if (typeof value === "string") return value.trim() !== "";
  if (typeof value === "number") return Number.isFinite(value);
  if (Array.isArray(value)) return value.length > 0;
  return true;
}

export function clean(value: unknown): string | null {
  if (!hasValue(value)) return null;
  if (typeof value === "object") return JSON.stringify(value);
  return String(value).trim();
}

/** Parses datastore strings like "1598", "1,598.0" or "0.5". Returns null for anything else. */
export function toNumber(value: unknown): number | null {
  if (typeof value === "number") return Number.isFinite(value) ? value : null;
  if (typeof value !== "string") return null;
  const normalized = value.trim().replace(/,/g, "");
  if (normalized === "" || !/^[-+]?\d*\.?\d+$/.test(normalized)) return null;
  const n = Number(normalized);
  return Number.isFinite(n) ? n : null;
}

/**
 * Normalizes the datastore's mixed indicator formats ("0" / "1" / "true" / numbers).
 * Mirrors VehicleTechnicalHistoryRecord.ToBool on the server. null ≠ false.
 */
export function toBool(value: unknown): boolean | null {
  if (value == null) return null;
  if (typeof value === "boolean") return value;
  if (typeof value === "number") return Number.isFinite(value) ? value !== 0 : null;
  const s = String(value).trim().toLowerCase();
  if (s === "") return null;
  if (s === "1" || s === "true" || s === "כן" || s === "y" || s === "yes") return true;
  if (s === "0" || s === "false" || s === "לא" || s === "n" || s === "no") return false;
  const n = Number(s);
  if (Number.isInteger(n)) return n !== 0;
  return null;
}

// ─── Dates ────────────────────────────────────────────────────────────────

export type ParsedGovDate = { date: Date; monthOnly: boolean };

function makeDate(y: number, m: number, d: number): Date | null {
  if (y < 1900 || y > 2200 || m < 1 || m > 12 || d < 1 || d > 31) return null;
  const date = new Date(y, m - 1, d);
  if (date.getMonth() !== m - 1) return null;
  return date;
}

/**
 * Recognizes the datastore's date formats and returns a local-midnight Date.
 * Supports ISO (with or without time), yyyy-MM, yyyyMMdd, yyyyMM, dd/MM/yyyy, dd.MM.yyyy.
 */
export function parseGovDateDetailed(value: unknown): ParsedGovDate | null {
  const raw = clean(value);
  if (!raw) return null;

  let m = raw.match(/^(\d{4})-(\d{1,2})-(\d{1,2})(?:[T\s].*)?$/);
  if (m) {
    const date = makeDate(+m[1], +m[2], +m[3]);
    return date ? { date, monthOnly: false } : null;
  }

  m = raw.match(/^(\d{4})-(\d{1,2})$/);
  if (m) {
    const date = makeDate(+m[1], +m[2], 1);
    return date ? { date, monthOnly: true } : null;
  }

  m = raw.match(/^(\d{4})(\d{2})(\d{2})$/);
  if (m) {
    const date = makeDate(+m[1], +m[2], +m[3]);
    if (date) return { date, monthOnly: false };
  }

  m = raw.match(/^(\d{4})(\d{2})$/);
  if (m) {
    const date = makeDate(+m[1], +m[2], 1);
    return date ? { date, monthOnly: true } : null;
  }

  m = raw.match(/^(\d{1,2})[./-](\d{1,2})[./-](\d{4})(?:\s.*)?$/);
  if (m) {
    const date = makeDate(+m[3], +m[2], +m[1]);
    return date ? { date, monthOnly: false } : null;
  }

  return null;
}

export function parseGovDate(value: unknown): Date | null {
  return parseGovDateDetailed(value)?.date ?? null;
}

const shortDateFormatter = new Intl.DateTimeFormat("he-IL", {
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
});
const longDateFormatter = new Intl.DateTimeFormat("he-IL", {
  day: "numeric",
  month: "long",
  year: "numeric",
});
const monthYearFormatter = new Intl.DateTimeFormat("he-IL", { month: "long", year: "numeric" });

/** DD/MM/YYYY. he-IL uses dots by default, so the parts are joined explicitly. */
export function formatDateShort(date: Date): string {
  const parts = shortDateFormatter.formatToParts(date);
  const get = (type: string) => parts.find((p) => p.type === type)?.value ?? "";
  return `${get("day")}/${get("month")}/${get("year")}`;
}

/** Formats a datastore date as DD/MM/YYYY ("מרץ 2021" for yyyyMM). Falls back to the raw string. */
export function formatDate(value: unknown): string | null {
  const raw = clean(value);
  if (!raw) return null;
  const parsed = parseGovDateDetailed(raw);
  if (!parsed) return raw;
  return parsed.monthOnly ? monthYearFormatter.format(parsed.date) : formatDateShort(parsed.date);
}

/** "5 במרץ 2024". */
export function formatDateLong(value: unknown): string | null {
  const raw = clean(value);
  if (!raw) return null;
  const parsed = parseGovDateDetailed(raw);
  if (!parsed) return raw;
  return parsed.monthOnly
    ? monthYearFormatter.format(parsed.date)
    : longDateFormatter.format(parsed.date);
}

/** "מרץ 2021". */
export function formatMonthYear(value: unknown): string | null {
  const raw = clean(value);
  if (!raw) return null;
  const parsed = parseGovDateDetailed(raw);
  return parsed ? monthYearFormatter.format(parsed.date) : raw;
}

const jerusalemParts = new Intl.DateTimeFormat("en-CA", {
  timeZone: "Asia/Jerusalem",
  year: "numeric",
  month: "2-digit",
  day: "2-digit",
});

/** Today's calendar date in Asia/Jerusalem, as a local-midnight Date. */
export function todayInJerusalem(now: Date = new Date()): Date {
  const parts = jerusalemParts.formatToParts(now);
  const get = (type: string) => Number(parts.find((p) => p.type === type)?.value);
  return new Date(get("year"), get("month") - 1, get("day"));
}

function dayNumber(date: Date): number {
  return Math.round(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()) / 86_400_000);
}

/** Whole days from today (Asia/Jerusalem, midnight) to `date`. Negative = in the past. */
export function daysFromToday(date: Date, now: Date = new Date()): number {
  return dayNumber(date) - dayNumber(todayInJerusalem(now));
}

/** Whole months between two dates (b − a). */
export function monthsBetween(a: Date, b: Date): number {
  let months = (b.getFullYear() - a.getFullYear()) * 12 + (b.getMonth() - a.getMonth());
  if (b.getDate() < a.getDate()) months -= 1;
  return months;
}

/** "כ-2 שנים ו-3 חודשים". */
export function formatDuration(totalMonths: number): string {
  if (totalMonths < 1) return he.duration.lessThanMonth;
  const years = Math.floor(totalMonths / 12);
  const months = totalMonths % 12;
  const parts: string[] = [];
  if (years > 0) parts.push(he.duration.years(years));
  if (months > 0) parts.push(he.duration.months(months));
  return he.common.about(parts.reduce(he.common.and));
}

// ─── Numbers & currency ───────────────────────────────────────────────────

const numberFormatter = new Intl.NumberFormat("he-IL", { maximumFractionDigits: 1 });
const integerFormatter = new Intl.NumberFormat("he-IL", { maximumFractionDigits: 0 });
/** Strips the bidi marks Intl inserts; values are isolated with LRI/PDI instead. */
function stripBidi(s: string): string {
  return s.replace(/[\u200e\u200f\u061c]/g, "");
}

export function formatNumber(value: number, maxFractionDigits = 1): string {
  const f =
    maxFractionDigits === 0
      ? integerFormatter
      : maxFractionDigits === 1
        ? numberFormatter
        : new Intl.NumberFormat("he-IL", { maximumFractionDigits: maxFractionDigits });
  const formatted = stripBidi(f.format(Math.abs(value)));
  return value < 0 ? isolate(`${MINUS}${formatted}`) : formatted;
}

function currency(value: number, sign: string): string {
  return `${isolate(sign + integerFormatter.format(Math.round(Math.abs(value))))}\u00a0₪`;
}

/** "12,345 ₪". */
export function formatCurrency(value: number): string {
  return currency(value, value < 0 ? MINUS : "");
}

/** "+12,345 ₪" / "−12,345 ₪". */
export function formatSignedCurrency(value: number): string {
  return currency(value, value > 0 ? "+" : value < 0 ? MINUS : "");
}

const unitPriceFormatter = new Intl.NumberFormat("he-IL", { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** "7.20 ₪" — per-liter / per-kWh prices keep two decimals. */
export function formatUnitPrice(value: number): string {
  return `${isolate(stripBidi(unitPriceFormatter.format(value)))}\u00a0₪`;
}

/** At most one decimal, explicit sign: "−3.5%", "+2%". */
export function formatPercent(value: number, signed = true): string {
  const rounded = Math.round(value * 10) / 10;
  const abs = numberFormatter.format(Math.abs(rounded));
  const sign = rounded < 0 ? MINUS : signed && rounded > 0 ? "+" : "";
  return isolate(`${sign}${abs}%`);
}

export function withUnit(value: unknown, unit: string, maxFractionDigits = 1): string | null {
  const n = toNumber(value);
  if (n == null) return clean(value);
  return `${formatNumber(n, maxFractionDigits)} ${unit}`;
}

export const formatKm = (v: unknown) => withUnit(v, he.units.km, 0);
export const formatKg = (v: unknown) => withUnit(v, he.units.kg, 0);
export const formatCc = (v: unknown) => withUnit(v, he.units.cc, 0);
export const formatHp = (v: unknown) => withUnit(v, he.units.hp, 0);

/** "7.4 שנים". */
export function formatAge(years: number): string {
  return he.common.years(formatNumber(Math.round(years * 10) / 10, 1));
}

export function formatBool(value: boolean | null | undefined): string {
  if (value === true) return he.common.yes;
  if (value === false) return he.common.no;
  return he.common.unknown;
}

// ─── Vehicle helpers ──────────────────────────────────────────────────────

export function vehicleTitle(vehicle: {
  manufacturerName?: string | null;
  commercialName?: string | null;
  modelName?: string | null;
  trimLevel?: string | null;
}): string {
  const parts = [
    clean(vehicle.manufacturerName),
    clean(vehicle.commercialName) ?? clean(vehicle.modelName),
    clean(vehicle.trimLevel),
  ].filter(Boolean);
  return parts.length ? parts.join(" ") : he.hero.fallbackTitle;
}