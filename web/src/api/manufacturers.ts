import { apiUrl } from "./client";
import type { ManufacturerModelRecord } from "../types/manufacturer";
import type { PagedResult } from "../types/vehicle";

export class ManufacturerLookupError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "ManufacturerLookupError";
    this.status = status;
  }
}

async function ensureOk(response: Response): Promise<void> {
  if (response.status === 404) {
    throw new ManufacturerLookupError(404, "לא מצאנו תוצר/דגם במאגר.");
  }
  if (response.status === 502) {
    throw new ManufacturerLookupError(
      502,
      "שירות הנתונים הממשלתי אינו זמין כרגע. נסו שוב בעוד רגע.",
    );
  }
  if (!response.ok) {
    throw new ManufacturerLookupError(response.status, "האיתור נכשל. נסו שוב בעוד רגע.");
  }
}

export async function listManufacturers(options?: {
  manufacturer?: string;
  model?: string;
  page?: number;
  pageSize?: number;
}): Promise<PagedResult<ManufacturerModelRecord>> {
  const params = new URLSearchParams();
  if (options?.manufacturer) params.set("manufacturer", options.manufacturer);
  if (options?.model) params.set("model", options.model);
  if (options?.page != null) params.set("page", String(options.page));
  if (options?.pageSize != null) params.set("pageSize", String(options.pageSize));

  const query = params.toString();
  const response = await fetch(apiUrl(`/api/manufacturers${query ? `?${query}` : ""}`));
  await ensureOk(response);
  return (await response.json()) as PagedResult<ManufacturerModelRecord>;
}

export async function lookupManufacturerModel(
  manufacturerCode: string,
  modelCode: string,
): Promise<ManufacturerModelRecord> {
  const response = await fetch(
    apiUrl(`/api/manufacturers/${encodeURIComponent(manufacturerCode)}/${encodeURIComponent(modelCode)}`),
  );
  await ensureOk(response);
  return (await response.json()) as ManufacturerModelRecord;
}
