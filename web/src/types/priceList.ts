/** Matches GET /api/pricelist/{manufacturerCode}/{modelCode}/{manufactureYear}. */
export interface VehiclePriceListRecord {
  id?: string | null;
  importerCode?: string | null;
  importerName?: string | null;
  modelType?: string | null;
  manufacturerCode?: string | null;
  manufacturerName?: string | null;
  modelCode?: string | null;
  modelName?: string | null;
  manufactureYear?: string | null;
  price?: string | null;
  commercialName?: string | null;
  extensionData?: Record<string, unknown> | null;
}
