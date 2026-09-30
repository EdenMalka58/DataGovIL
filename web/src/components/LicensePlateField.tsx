import { formatPlateInput, normalizePlate, PLATE_MAX_DIGITS } from "../lib/plate";
import { he } from "../strings.he";

type LicensePlateFieldProps = {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  id?: string;
  errorId?: string;
  invalid?: boolean;
};

export function LicensePlateField({
  value,
  onChange,
  disabled,
  id = "plate",
  errorId,
  invalid,
}: LicensePlateFieldProps) {
  return (
    <div className={`plate-input plate-input--lg ${invalid ? "is-invalid" : ""} ${disabled ? "is-disabled" : ""}`}>
      <input
        id={id}
        data-search-input
        type="text"
        inputMode="numeric"
        autoComplete="off"
        autoCorrect="off"
        spellCheck={false}
        enterKeyHint="search"
        disabled={disabled}
        maxLength={12}
        placeholder={he.search.placeholder}
        value={formatPlateInput(value)}
        onChange={(event) => onChange(normalizePlate(event.target.value).slice(0, PLATE_MAX_DIGITS))}
        aria-label={he.search.label}
        aria-invalid={invalid || undefined}
        aria-describedby={errorId}
        dir="ltr"
      />
    </div>
  );
}
