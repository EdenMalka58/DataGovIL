/** Mirrors the API VehicleEnergyCost DTO (GET /api/Vehicles/{mfr}/{model}/energy-cost). */
export type VehicleEnergyType = "Gasoline" | "Diesel" | "Electric" | "Unknown";

export interface VehicleEnergyCost {
  energyType: VehicleEnergyType;
  kmPerMonth: number;
  /** km/l for fuel vehicles, kWh/100km for electric; null when unknown. */
  consumption: number | null;
  consumptionUnit: string;
  energyPrice: number;
  energyPriceUnit: string;
  /** ILS per month, full precision; null when it cannot be calculated. */
  monthlyCost: number | null;
  isEstimated: boolean;
}
