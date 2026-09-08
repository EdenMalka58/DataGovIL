import type { FormEvent } from "react";
import { LicensePlateField } from "./LicensePlateField";

type HeroSearchProps = {
  plate: string;
  onPlateChange: (value: string) => void;
  onSubmit: () => void;
  loading: boolean;
  error: string | null;
};

export function HeroSearch({ plate, onPlateChange, onSubmit, loading, error }: HeroSearchProps) {
  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    onSubmit();
  }

  return (
    <section className="relative px-4 pb-6 pt-10 sm:px-6 sm:pt-16">
      <div className="mx-auto max-w-3xl text-center">
        <p className="mb-4 inline-flex items-center gap-2 rounded-full border border-line bg-card px-3 py-1 text-[0.78rem] font-semibold text-ink-soft">
          <span className="h-1.5 w-1.5 rounded-full bg-brand" />
          נתונים רשמיים ממאגר הרישוי הפתוח
        </p>
        <h1 className="text-[2rem] font-extrabold leading-[1.15] tracking-tight text-ink sm:text-5xl">
          בחירת רכב יד שנייה,
          <span className="block text-brand-deep">לפי נתונים — לא לפי תחושת בטן</span>
        </h1>
        <p className="mx-auto mt-4 max-w-xl text-base leading-relaxed text-slate-body sm:text-lg">
          הזינו מספר רישוי, גלו את פרטי הרכב הרשמיים, והוסיפו כמה אפשרויות להשוואה לפני שסוגרים עסקה.
        </p>

        <form
          onSubmit={handleSubmit}
          className="card-lift mx-auto mt-8 max-w-xl rounded-3xl border border-line bg-card p-4 text-right sm:p-6"
        >
          <LicensePlateField value={plate} onChange={onPlateChange} disabled={loading} />
          <button
            type="submit"
            disabled={loading}
            className="mt-4 flex h-12 w-full items-center justify-center gap-2 rounded-2xl bg-ink text-base font-bold text-paper transition hover:bg-ink-soft disabled:cursor-not-allowed disabled:opacity-70"
          >
            {loading ? (
              <>
                <Spinner />
                מאתר במאגר...
              </>
            ) : (
              "איתור רכב"
            )}
          </button>
          {error && (
            <p role="alert" className="mt-3 rounded-xl bg-red-50 px-3 py-2 text-sm font-medium text-red-700">
              {error}
            </p>
          )}
        </form>
      </div>
    </section>
  );
}

function Spinner() {
  return (
    <svg className="h-4 w-4 animate-spin" viewBox="0 0 24 24" aria-hidden="true">
      <circle cx="12" cy="12" r="9" fill="none" stroke="currentColor" strokeOpacity="0.25" strokeWidth="3" />
      <path d="M21 12a9 9 0 0 0-9-9" fill="none" stroke="currentColor" strokeWidth="3" strokeLinecap="round" />
    </svg>
  );
}
