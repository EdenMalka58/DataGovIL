import { formatPlate, normalizePlate } from "../lib/plate";

type LicensePlateFieldProps = {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  id?: string;
};

export function LicensePlateField({ value, onChange, disabled, id = "plate" }: LicensePlateFieldProps) {
  const digits = normalizePlate(value);
  const display = formatPlate(digits);

  return (
    <label htmlFor={id} className="block">
      <span className="mb-2 block text-sm font-semibold text-ink-soft">מספר רישוי</span>
      <div className={`plate-shell ${disabled ? "opacity-70" : ""}`}>
        <div className="plate-il" aria-hidden="true">
          <FlagIcon />
          <span className="text-[0.7rem]">IL</span>
        </div>
        <input
          id={id}
          inputMode="numeric"
          autoComplete="off"
          spellCheck={false}
          disabled={disabled}
          maxLength={10}
          placeholder="12-345-67"
          value={display}
          onChange={(event) => onChange(normalizePlate(event.target.value).slice(0, 8))}
          className="plate-input h-[4.4rem] px-3 text-[2rem] sm:h-[5rem] sm:text-[2.45rem]"
          aria-label="מספר רישוי"
        />
      </div>
    </label>
  );
}

function FlagIcon() {
  return (
    <svg viewBox="0 0 18 13" className="h-3 w-4" aria-hidden="true">
      <rect width="18" height="13" fill="white" />
      <rect y="2.2" width="18" height="1.7" fill="#0038b8" />
      <rect y="9.1" width="18" height="1.7" fill="#0038b8" />
      <path
        d="M9 3.4 9.7 5.5h2.2l-1.8 1.3.7 2.1L9 7.6l-1.8 1.3.7-2.1-1.8-1.3h2.2Z"
        fill="#0038b8"
      />
    </svg>
  );
}
