const NON_DIGITS = /[^\d]/g;

export const PLATE_MIN_DIGITS = 5;
export const PLATE_MAX_DIGITS = 8;

/** Strips spaces, dashes and anything else that isn't a digit: "12-345-67" → "1234567". */
export function normalizePlate(value: string): string {
  return value.replace(NON_DIGITS, "");
}

export function isValidPlate(value: string): boolean {
  const digits = normalizePlate(value);
  return digits.length >= PLATE_MIN_DIGITS && digits.length <= PLATE_MAX_DIGITS;
}

/** Israeli plate format: 7 digits → 12-345-67, 8 digits → 123-45-678, otherwise digits as-is. */
export function formatPlate(value: string | null | undefined): string {
  const digits = normalizePlate(String(value ?? ""));
  if (digits.length === 7) return `${digits.slice(0, 2)}-${digits.slice(2, 5)}-${digits.slice(5)}`;
  if (digits.length === 8) return `${digits.slice(0, 3)}-${digits.slice(3, 5)}-${digits.slice(5)}`;
  return digits;
}

/** Progressive formatting while typing (assumes 7-digit layout until an 8th digit is typed). */
export function formatPlateInput(value: string): string {
  const digits = normalizePlate(value);
  if (digits.length <= 7) {
    return [digits.slice(0, 2), digits.slice(2, 5), digits.slice(5, 7)].filter(Boolean).join("-");
  }
  return formatPlate(digits);
}
