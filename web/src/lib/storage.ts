import type { VehicleRecord } from "../types/vehicle";
import { stripIdentifying } from "./viewModel";

const KEYS = {
  recent: "autoscope.recent.v1",
  theme: "autoscope.theme",
  comparison: "autoscope.comparison.v3",
};

export const RECENT_LIMIT = 8;

/** Only the plate and a display title — never VIN or engine number. */
export type RecentSearch = { plate: string; title: string; at: number };

function read<T>(key: string, fallback: T): T {
  try {
    const raw = localStorage.getItem(key);
    return raw ? (JSON.parse(raw) as T) : fallback;
  } catch {
    return fallback;
  }
}

function write(key: string, value: unknown) {
  try {
    localStorage.setItem(key, JSON.stringify(value));
  } catch {
    // Storage full or disabled — the feature just degrades.
  }
}

export function readRecent(): RecentSearch[] {
  const items = read<RecentSearch[]>(KEYS.recent, []);
  return Array.isArray(items) ? items.filter((i) => i && typeof i.plate === "string").slice(0, RECENT_LIMIT) : [];
}

export function pushRecent(item: RecentSearch): RecentSearch[] {
  const next = [item, ...readRecent().filter((i) => i.plate !== item.plate)].slice(0, RECENT_LIMIT);
  write(KEYS.recent, next);
  return next;
}

export function clearRecent(): RecentSearch[] {
  try {
    localStorage.removeItem(KEYS.recent);
  } catch {
    // ignore
  }
  return [];
}

export type Theme = "light" | "dark";

export function readTheme(): Theme | null {
  try {
    const v = localStorage.getItem(KEYS.theme);
    return v === "light" || v === "dark" ? v : null;
  } catch {
    return null;
  }
}

export function writeTheme(theme: Theme) {
  try {
    localStorage.setItem(KEYS.theme, theme);
  } catch {
    // ignore
  }
}

export function readComparison(limit: number): VehicleRecord[] {
  const items = read<VehicleRecord[]>(KEYS.comparison, []);
  return Array.isArray(items) ? items.slice(0, limit) : [];
}

export function writeComparison(items: VehicleRecord[]) {
  write(KEYS.comparison, items.map(stripIdentifying));
}

/** In-memory cache of results from this visit, used as a soft fallback when the network fails. */
const sessionResults = new Map<string, VehicleRecord>();

export function rememberResult(plate: string, vehicle: VehicleRecord) {
  sessionResults.set(plate, vehicle);
}

export function recallResult(plate: string): VehicleRecord | undefined {
  return sessionResults.get(plate);
}
