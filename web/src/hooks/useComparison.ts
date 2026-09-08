import { useCallback, useEffect, useState } from "react";
import { COMPARISON_LIMIT, type VehicleRecord } from "../types/vehicle";

const STORAGE_KEY = "autoscope.comparison.v1";

function plateOf(vehicle: VehicleRecord): string {
  return String(vehicle.mispar_rechev ?? "").trim();
}

function readStored(): VehicleRecord[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw) as VehicleRecord[];
    return Array.isArray(parsed) ? parsed.slice(0, COMPARISON_LIMIT) : [];
  } catch {
    return [];
  }
}

export function useComparison() {
  const [items, setItems] = useState<VehicleRecord[]>(readStored);

  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  }, [items]);

  const has = useCallback(
    (vehicle: VehicleRecord) => {
      const plate = plateOf(vehicle);
      return plate !== "" && items.some((item) => plateOf(item) === plate);
    },
    [items],
  );

  const add = useCallback((vehicle: VehicleRecord) => {
    const plate = plateOf(vehicle);
    if (!plate) return { ok: false as const, reason: "missing-plate" };
    setItems((current) => {
      if (current.some((item) => plateOf(item) === plate)) return current;
      if (current.length >= COMPARISON_LIMIT) return current;
      return [...current, vehicle];
    });
    return { ok: true as const };
  }, []);

  const remove = useCallback((registrationNumber: string) => {
    setItems((current) =>
      current.filter((item) => plateOf(item) !== String(registrationNumber).trim()),
    );
  }, []);

  const clear = useCallback(() => setItems([]), []);

  return { items, has, add, remove, clear, limit: COMPARISON_LIMIT, isFull: items.length >= COMPARISON_LIMIT };
}
