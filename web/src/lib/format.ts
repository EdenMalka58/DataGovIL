export function displayValue(value: string | null | undefined): string {
  if (value == null || String(value).trim() === "") return "—";
  return String(value).trim();
}

export function formatDate(value: string | null | undefined): string {
  if (value == null || String(value).trim() === "") return "—";
  const raw = String(value).trim();

  const iso = raw.match(/^(\d{4})-(\d{2})-(\d{2})/);
  if (iso) return `${iso[3]}/${iso[2]}/${iso[1]}`;

  const dmy = raw.match(/^(\d{1,2})[./-](\d{1,2})[./-](\d{4})$/);
  if (dmy) {
    const day = dmy[1].padStart(2, "0");
    const month = dmy[2].padStart(2, "0");
    return `${day}/${month}/${dmy[3]}`;
  }

  return raw;
}

export function vehicleTitle(vehicle: {
  tozeret_nm?: string | null;
  kinuy_mishari?: string | null;
  degem_nm?: string | null;
}): string {
  const commercial = vehicle.kinuy_mishari?.trim();
  const manufacturer = vehicle.tozeret_nm?.trim();
  if (commercial && manufacturer) return `${manufacturer} ${commercial}`;
  return commercial || manufacturer || vehicle.degem_nm?.trim() || "רכב";
}
