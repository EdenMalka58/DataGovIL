import { displayValue, formatDate, vehicleTitle } from "../lib/format";
import { formatPlate } from "../lib/plate";
import type { VehicleRecord } from "../types/vehicle";

type VehicleResultCardProps = {
  vehicle: VehicleRecord;
  inComparison: boolean;
  comparisonFull: boolean;
  onAdd: () => void;
};

const EXTRA_FIELDS: { key: string; label: string; kind?: "date" }[] = [
  { key: "mivchan_acharon_dt", label: "מבחן אחרון", kind: "date" },
  { key: "kvutzat_zihum", label: "קבוצת זיהום" },
  { key: "zmig_kidmi", label: "צמיג קדמי" },
  { key: "zmig_ahori", label: "צמיג אחורי" },
  { key: "degem_manoa", label: "דגם מנוע" },
  { key: "mishkal_kolel", label: "משקל כולל" },
  { key: "koah_sus", label: "כוח סוס" },
  { key: "mispar_mekomot", label: "מקומות ישיבה" },
  { key: "hanaa_nm", label: "הנעה" },
  { key: "ramat_eivzur_betihuty", label: "רמת אבזור בטיחותי" },
];

export function VehicleResultCard({
  vehicle,
  inComparison,
  comparisonFull,
  onAdd,
}: VehicleResultCardProps) {
  const extras = EXTRA_FIELDS.filter((field) => {
    const value = vehicle[field.key];
    return value != null && String(value).trim() !== "";
  });

  return (
    <section id="result" className="px-4 pb-8 sm:px-6">
      <article className="card-lift mx-auto max-w-4xl overflow-hidden rounded-3xl border border-line bg-card">
        <div className="flex flex-col gap-5 border-b border-line bg-gradient-to-l from-brand/8 to-transparent px-5 py-5 sm:flex-row sm:items-start sm:justify-between sm:px-7">
          <div>
            <p className="text-xs font-bold tracking-wide text-brand">הרכב שאותר</p>
            <h2 className="mt-1 text-2xl font-extrabold tracking-tight text-ink sm:text-3xl">
              {vehicleTitle(vehicle)}
            </h2>
            <p className="mt-1 text-sm text-slate-body">
              {displayValue(vehicle.degem_nm)}
              {vehicle.shnat_yitzur ? ` · שנת ${vehicle.shnat_yitzur}` : ""}
            </p>
          </div>
          <MiniPlate number={vehicle.mispar_rechev} />
        </div>

        <dl className="grid grid-cols-2 gap-px bg-line sm:grid-cols-3">
          <Field label="יצרן" value={displayValue(vehicle.tozeret_nm)} />
          <Field label="שם מסחרי" value={displayValue(vehicle.kinuy_mishari)} />
          <Field label="דגם" value={displayValue(vehicle.degem_nm)} />
          <Field label="שנת ייצור" value={displayValue(vehicle.shnat_yitzur)} />
          <Field label="רמת גימור" value={displayValue(vehicle.ramat_gimur)} />
          <Field label="צבע" value={displayValue(vehicle.tzeva_rechev)} />
          <Field label="סוג דלק" value={displayValue(vehicle.sug_delek_nm)} />
          <Field label="בעלות" value={displayValue(vehicle.baalut)} />
          <Field label="עלייה לכביש" value={formatDate(vehicle.moed_aliya_lakvish)} />
          <Field label="תוקף טסט" value={formatDate(vehicle.tokef_dt)} />
          {extras.map((field) => (
            <Field
              key={field.key}
              label={field.label}
              value={
                field.kind === "date"
                  ? formatDate(String(vehicle[field.key] ?? ""))
                  : displayValue(String(vehicle[field.key]))
              }
            />
          ))}
        </dl>

        <div className="flex flex-col gap-3 px-5 py-5 sm:flex-row sm:items-center sm:justify-between sm:px-7">
          <p className="text-sm text-slate-body">
            {inComparison
              ? "הרכב כבר נמצא ברשימת ההשוואה שלכם."
              : comparisonFull
                ? "ההשוואה מלאה. הסירו רכב אחד כדי לפנות מקום."
                : "שמרו את הרכב כדי להשוות מול אפשרויות נוספות שאתם בודקים."}
          </p>
          <button
            type="button"
            onClick={onAdd}
            disabled={inComparison || comparisonFull}
            className="h-11 shrink-0 rounded-2xl bg-brand px-5 text-sm font-bold text-white transition hover:bg-brand-deep disabled:cursor-not-allowed disabled:bg-line disabled:text-slate-body"
          >
            {inComparison ? "נוסף להשוואה" : "הוסף להשוואה"}
          </button>
        </div>
      </article>
    </section>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-card px-5 py-4">
      <dt className="text-[0.7rem] font-bold tracking-wide text-slate-body">{label}</dt>
      <dd className="mt-1 text-[0.98rem] font-semibold text-ink">{value}</dd>
    </div>
  );
}

function MiniPlate({ number }: { number?: string | null }) {
  return (
    <div className="plate-shell h-12 w-[11.5rem] shrink-0 self-start">
      <div className="plate-il compact">
        <span className="text-[0.55rem]">IL</span>
      </div>
      <div className="grid flex-1 place-items-center px-2 font-[family-name:var(--font-plate)] text-lg font-semibold tracking-[0.12em]">
        {number ? formatPlate(number) : "—"}
      </div>
    </div>
  );
}
