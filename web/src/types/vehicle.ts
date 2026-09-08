/** Matches the JSON shape returned by GET /api/vehicles/{registrationNumber}. */
export interface VehicleRecord {
  mispar_rechev?: string | null;
  tozeret_cd?: string | null;
  tozeret_nm?: string | null;
  degem_cd?: string | null;
  degem_nm?: string | null;
  kinuy_mishari?: string | null;
  shnat_yitzur?: string | null;
  ramat_gimur?: string | null;
  tzeva_rechev?: string | null;
  sug_delek_nm?: string | null;
  baalut?: string | null;
  misgeret?: string | null;
  moed_aliya_lakvish?: string | null;
  tokef_dt?: string | null;
  [key: string]: unknown;
}

export const COMPARISON_LIMIT = 4;
