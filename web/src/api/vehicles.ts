import type { VehicleRecord } from "../types/vehicle";
import { normalizePlate } from "../lib/plate";

export class VehicleLookupError extends Error {
  readonly status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = "VehicleLookupError";
    this.status = status;
  }
}

export async function lookupVehicle(registrationNumber: string): Promise<VehicleRecord> {
  const plate = normalizePlate(registrationNumber);
  const response = await fetch(`/api/vehicles/${encodeURIComponent(plate)}`);

  if (response.status === 404) {
    throw new VehicleLookupError(404, "לא מצאנו רכב עם מספר הרישוי הזה במאגר.");
  }

  if (response.status === 502) {
    throw new VehicleLookupError(
      502,
      "שירות הנתונים הממשלתי אינו זמין כרגע. נסו שוב בעוד רגע.",
    );
  }

  if (!response.ok) {
    throw new VehicleLookupError(response.status, "האיתור נכשל. נסו שוב בעוד רגע.");
  }

  return (await response.json()) as VehicleRecord;
}
