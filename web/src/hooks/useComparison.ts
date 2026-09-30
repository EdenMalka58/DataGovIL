import { useCallback, useEffect, useState } from "react";
import { readComparison, writeComparison } from "../lib/storage";
import { COMPARISON_LIMIT, type VehicleRecord } from "../types/vehicle";

function plateOf(vehicle: VehicleRecord): string {
  return String(vehicle.registrationNumber ?? "").trim();
}

export function useComparison() {
  const [items, setItems] = useState<VehicleRecord[]>(() => readComparison(COMPARISON_LIMIT));

  useEffect(() => {
    writeComparison(items);
  }, [items]);

  const has = useCallback(
    (plate: string) => plate !== "" && items.some((item) => plateOf(item) === plate),
    [items],
  );

  const add = useCallback(
    (vehicle: VehicleRecord): "added" | "duplicate" | "full" | "invalid" => {
      const plate = plateOf(vehicle);
      if (!plate) return "invalid";
      if (items.some((item) => plateOf(item) === plate)) return "duplicate";
      if (items.length >= COMPARISON_LIMIT) return "full";
      setItems((current) => [...current, vehicle].slice(0, COMPARISON_LIMIT));
      return "added";
    },
    [items],
  );

  const remove = useCallback((plate: string) => {
    setItems((current) => current.filter((item) => plateOf(item) !== plate));
  }, []);

  const clear = useCallback(() => setItems([]), []);

  return {
    items,
    has,
    add,
    remove,
    clear,
    limit: COMPARISON_LIMIT,
    isFull: items.length >= COMPARISON_LIMIT,
  };
}

export type Comparison = ReturnType<typeof useComparison>;
