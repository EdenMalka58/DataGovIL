const DIGITS = /[^\d]/g;

export function normalizePlate(value: string): string {
  return value.replace(DIGITS, "");
}

/** Formats an Israeli plate: 8 digits → 123-45-678, 7 → 12-345-67. */
export function formatPlate(value: string): string {
  const digits = normalizePlate(value);
  if (digits.length <= 7) {
    const a = digits.slice(0, 2);
    const b = digits.slice(2, 5);
    const c = digits.slice(5, 7);
    return [a, b, c].filter(Boolean).join("-");
  }
  const a = digits.slice(0, 3);
  const b = digits.slice(3, 5);
  const c = digits.slice(5, 8);
  return [a, b, c].filter(Boolean).join("-");
}

export function isLikelyPlate(value: string): boolean {
  const digits = normalizePlate(value);
  return digits.length >= 6 && digits.length <= 8;
}
