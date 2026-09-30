import type { VehicleEnergyCost } from "../types/energyCost";
import type { VehicleHistoryRecord, VehicleRecord } from "../types/vehicle";
import type { VehiclePriceListRecord } from "../types/priceList";
import { normalizePlate } from "../lib/plate";
import { getJson } from "./client";

export function lookupVehicle(registrationNumber: string, signal?: AbortSignal): Promise<VehicleRecord> {
  const plate = normalizePlate(registrationNumber);
  return getJson<VehicleRecord>(`/api/Vehicles/${encodeURIComponent(plate)}`, signal);
}

/** History is already embedded in VehicleRecord.history — only use this for an explicit refresh. */
export function lookupVehicleHistory(
  registrationNumber: string,
  signal?: AbortSignal,
): Promise<VehicleHistoryRecord> {
  const plate = normalizePlate(registrationNumber);
  return getJson<VehicleHistoryRecord>(`/api/Vehicles/${encodeURIComponent(plate)}/history`, signal);
}

export function lookupPriceList(
  manufacturerCode: string,
  modelCode: string,
  manufactureYear: string,
  signal?: AbortSignal,
): Promise<VehiclePriceListRecord[]> {
  const path = [manufacturerCode, modelCode, manufactureYear].map(encodeURIComponent).join("/");
  return getJson<VehiclePriceListRecord[]>(`/api/PriceList/${path}`, signal);
}

/** Estimated monthly energy cost; omit kmPerMonth to use the server default. */
export function lookupEnergyCost(
  manufacturerCode: string,
  modelCode: string,
  kmPerMonth?: number | null,
  signal?: AbortSignal,
): Promise<VehicleEnergyCost> {
  const path = [manufacturerCode, modelCode].map(encodeURIComponent).join("/");
  const query = kmPerMonth != null ? `?kmPerMonth=${encodeURIComponent(String(kmPerMonth))}` : "";
  return getJson<VehicleEnergyCost>(`/api/Vehicles/${path}/energy-cost${query}`, signal);
}
