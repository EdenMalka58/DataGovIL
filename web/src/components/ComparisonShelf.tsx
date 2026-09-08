import { vehicleTitle } from "../lib/format";
import { formatPlate } from "../lib/plate";
import { COMPARISON_LIMIT, type VehicleRecord } from "../types/vehicle";

type ComparisonShelfProps = {
  items: VehicleRecord[];
  onRemove: (registrationNumber: string) => void;
  onClear: () => void;
};

export function ComparisonShelf({ items, onRemove, onClear }: ComparisonShelfProps) {
  const emptySlots = Math.max(0, COMPARISON_LIMIT - items.length);

  return (
    <section id="compare" className="px-4 pb-16 sm:px-6">
      <div className="mx-auto max-w-6xl">
        <div className="mb-4 flex flex-wrap items-end justify-between gap-3">
          <div>
            <p className="text-xs font-bold tracking-wide text-brand">המוסך שלכם</p>
            <h2 className="text-2xl font-extrabold tracking-tight text-ink">הרכבים שאתם בודקים</h2>
            <p className="mt-1 text-sm text-slate-body">
              עד {COMPARISON_LIMIT} רכבים להשוואה. הרשימה נשמרת במכשיר הזה.
            </p>
          </div>
          {items.length > 0 && (
            <button
              type="button"
              onClick={onClear}
              className="text-sm font-semibold text-slate-body underline-offset-2 hover:text-ink hover:underline"
            >
              נקה הכל
            </button>
          )}
        </div>

        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
          {items.map((vehicle) => {
            const plate = String(vehicle.mispar_rechev ?? "");
            return (
              <article
                key={plate || vehicleTitle(vehicle)}
                className="card-lift flex flex-col rounded-2xl border border-line bg-card p-4"
              >
                <div className="plate-shell h-11">
                  <div className="plate-il compact">
                    <span className="text-[0.55rem]">IL</span>
                  </div>
                  <div className="grid flex-1 place-items-center font-[family-name:var(--font-plate)] text-base font-semibold tracking-[0.12em]">
                    {plate ? formatPlate(plate) : "—"}
                  </div>
                </div>
                <h3 className="mt-3 text-base font-extrabold leading-snug text-ink">
                  {vehicleTitle(vehicle)}
                </h3>
                <p className="mt-1 text-sm text-slate-body">
                  {[vehicle.shnat_yitzur, vehicle.sug_delek_nm, vehicle.tzeva_rechev]
                    .filter((part) => part && String(part).trim())
                    .join(" · ") || "פרטים חלקיים"}
                </p>
                <button
                  type="button"
                  onClick={() => onRemove(plate)}
                  className="mt-4 self-start text-sm font-semibold text-slate-body hover:text-red-700"
                >
                  הסר מההשוואה
                </button>
              </article>
            );
          })}

          {Array.from({ length: emptySlots }).map((_, index) => (
            <div
              key={`empty-${index}`}
              className="flex min-h-[11.5rem] flex-col items-center justify-center rounded-2xl border border-dashed border-line bg-card/50 px-4 text-center"
            >
              <span className="grid h-10 w-10 place-items-center rounded-full border border-line text-lg text-slate-body">
                +
              </span>
              <p className="mt-3 text-sm font-medium text-slate-body">
                {items.length === 0 && index === 0
                  ? "אתרו רכב והוסיפו אותו להשוואה"
                  : "מקום לרכב נוסף"}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
