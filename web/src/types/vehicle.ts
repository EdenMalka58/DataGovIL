import type { VehicleEnergyCost } from "./energyCost";
import type { ManufacturerModelRecord } from "./manufacturer";

/**
 * Matches the JSON shape returned by GET /api/vehicles/{registrationNumber}.
 * Null values are omitted by the server, and `isInactive` / `isSafetyDiscountEligible`
 * are only serialized when true — always check existence, not value.
 */
export interface VehicleRecord {
  id?: string | null;
  registrationNumber?: string | null;
  manufacturerCode?: string | null;
  modelType?: string | null;
  manufacturerName?: string | null;
  modelCode?: string | null;
  modelName?: string | null;
  vehicleTypeCode?: string | null;
  vehicleTypeName?: string | null;
  trimLevel?: string | null;
  safetyEquipmentLevel?: string | null;
  pollutionGroup?: string | null;
  manufactureYear?: string | null;
  engineManufacturer?: string | null;
  engineModel?: string | null;
  engineNumber?: string | null;
  totalWeight?: string | null;
  lastTestDate?: string | null;
  testValidUntil?: string | null;
  cancellationDate?: string | null;
  ownershipType?: string | null;
  chassisNumber?: string | null;
  colorCode?: string | null;
  color?: string | null;
  frontTire?: string | null;
  rearTire?: string | null;
  fuelType?: string | null;
  registrationOrder?: string | null;
  roadEntryDate?: string | null;
  /** Road entry date + 36 months (`yyyy-MM-dd`). Omitted without a road entry date. */
  estimatedWarrantyEndDate?: string | null;
  /** Whether the estimated manufacturer warranty is still in effect today. Omitted without a road entry date. */
  estimatedWarrantyActive?: boolean | null;
  updatedDate?: string | null;
  importType?: string | null;
  commercialName?: string | null;
  isPermanentlyCancelled?: boolean;
  isInactive?: boolean;
  isSafetyDiscountEligible?: boolean;
  manufacturerModel?: ManufacturerModelRecord | null;
  recalls?: VehicleRecallRecord[] | null;
  history?: VehicleHistoryRecord | null;
  depreciation?: VehicleDepreciationRecord | null;
  /** Estimated monthly energy cost at the default mileage. */
  energyCost?: VehicleEnergyCost | null;
  extensionData?: Record<string, unknown> | null;
}

export interface VehicleRecallRecord {
  id?: string | null;
  recallId?: string | null;
  recallType?: string | null;
  faultType?: string | null;
  faultDescription?: string | null;
  openedDate?: string | null;
}

export type VehicleCategory = "Private" | "Commercial4t" | "Commercial" | "Taxi" | "Minibus";

export type OwnerCategory =
  | "Private"
  | "Commercial"
  | "Public"
  | "Rental"
  | "Kibbutz"
  | "Government"
  | "Taxi"
  | "DriveTech";

export type DepreciationFactor = "Kilometers" | "OwnerCount" | "OwnershipType";

/** Signed percents: negative lowers the value, positive raises it. */
export interface VehicleDepreciationRecord {
  depreciationPercent: number;
  depreciationValue?: number | null;
  listPrice?: number | null;
  estimatedValue?: number | null;
  carAge: number;
  kilometers?: number | null;
  ownerCount: number;
  originality?: string | null;
  ownershipType?: string | null;
  vehicleCategory: VehicleCategory;
  ownerCategory: OwnerCategory;
  lines: VehicleDepreciationLine[];
}

export interface VehicleDepreciationLine {
  factor: DepreciationFactor;
  description: string;
  percent: number;
  value?: number | null;
}

/** Matches GET /api/vehicles/{registrationNumber}/history. */
export interface VehicleHistoryRecord {
  registrationNumber?: string | null;
  technical?: VehicleTechnicalHistoryRecord | null;
  ownershipHistory?: VehicleOwnershipHistoryRecord[] | null;
}

export interface VehicleTechnicalHistoryRecord {
  id?: string | null;
  registrationNumber?: string | null;
  engineNumber?: string | null;
  lastTestOdometer?: string | null;
  structureChangeIndicator?: boolean | null;
  /** LPG (גפ״מ) system installed/changed — not an accident indicator. */
  lpgChangeIndicator?: boolean | null;
  colorChangeIndicator?: boolean | null;
  tireChangeIndicator?: boolean | null;
  firstRegistrationDate?: string | null;
  originalityName?: string | null;
  extensionData?: Record<string, unknown> | null;
}

export interface VehicleOwnershipHistoryRecord {
  id?: string | null;
  registrationNumber?: string | null;
  /** From `baalut_dt` (typically `yyyyMM`). */
  ownershipYearMonth?: string | null;
  ownershipType?: string | null;
  extensionData?: Record<string, unknown> | null;
}

/** Matches paged list endpoints (`PagedResult<T>`). */
export interface PagedResult<T> {
  page: number;
  pageSize: number;
  totalCount?: number | null;
  items: T[];
}

/** RFC 7807 body returned by the API for 502 upstream failures. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
}

export const COMPARISON_LIMIT = 3;
