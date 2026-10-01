import { useMemo, useState, type FormEvent } from "react";
import { clean } from "../../lib/format";
import { isValidPlate, normalizePlate } from "../../lib/plate";
import { buyerSummary, computeStatuses, hasListPrice, isOffRoad } from "../../lib/viewModel";
import { he } from "../../strings.he";
import type { VehicleRecord } from "../../types/vehicle";
import { Icon } from "../Icon";
import { LicensePlateField } from "../LicensePlateField";
import { Reveal } from "../ui";
import { BuyerSummary } from "./BuyerSummary";
import { EnergyCostCard } from "./EnergyCostCard";
import { HistorySection } from "./HistorySection";
import { PopularityCard } from "./PopularityCard";
import { RecallsCard } from "./RecallsCard";
import { SpecsSection } from "./SpecsSection";
import { CancelledRibbon, StatusBanner, scrollToAnchor } from "./StatusBanner";
import { ValuationPanel } from "./ValuationPanel";
import { VehicleHero } from "./VehicleHero";

type VehicleViewProps = {
  vehicle: VehicleRecord;
  cached: boolean;
  inComparison: boolean;
  onCompare: () => void;
  onPrint: () => void;
  onSearch: (plate: string) => void;
  printMode: boolean;
};

const NAV = [
  { id: "summary", label: he.nav.summary },
  { id: "value", label: he.nav.value },
  { id: "history", label: he.nav.history },
  { id: "popularity", label: he.nav.popularity },
  { id: "specs", label: he.nav.specs },
  { id: "energy", label: he.nav.energy },
];

function AnotherSearch({ onSearch }: { onSearch: (plate: string) => void }) {
  const [plate, setPlate] = useState("");
  const [error, setError] = useState<string | null>(null);

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    const digits = normalizePlate(plate);
    if (!digits) return setError(he.search.empty);
    if (!isValidPlate(digits)) return setError(he.search.invalid);
    onSearch(digits);
  }

  return (
    <section className="card another-search no-print" aria-labelledby="another-title">
      <div className="another-search__text">
        <h2 id="another-title" className="another-search__title">
          <Icon name="search" size={22} />
          {he.search.anotherTitle}
        </h2>
        <p>{he.search.anotherText}</p>
      </div>
      <form className="another-search__form" onSubmit={handleSubmit} noValidate role="search">
        <LicensePlateField
          id="plate-another"
          value={plate}
          onChange={(value) => {
            setPlate(value);
            setError(null);
          }}
          invalid={!!error}
          errorId={error ? "another-error" : undefined}
        />
        <button type="submit" className="btn btn--primary btn--lg">
          <Icon name="search" />
          {he.search.submit}
        </button>
        {error && (
          <p id="another-error" className="field-error another-search__error" role="alert">
            {error}
          </p>
        )}
      </form>
    </section>
  );
}

export function VehicleView({
  vehicle,
  cached,
  inComparison,
  onCompare,
  onPrint,
  onSearch,
  printMode,
}: VehicleViewProps) {
  const statuses = useMemo(() => computeStatuses(vehicle), [vehicle]);
  const summary = useMemo(() => buyerSummary(vehicle), [vehicle]);
  const offRoad = isOffRoad(vehicle);
  const mutedReason = vehicle.isPermanentlyCancelled
    ? he.status.grayedCancelled
    : vehicle.isInactive
      ? he.status.grayedInactive
      : null;
  const recalls = Array.isArray(vehicle.recalls) ? vehicle.recalls : [];
  const alerts = summary.flags.filter((f) => f.tone === "bad" || f.tone === "warn");
  const manufacturerCode = clean(vehicle.manufacturerCode) ?? clean(vehicle.manufacturerModel?.manufacturerCode);
  const modelCode = clean(vehicle.modelCode) ?? clean(vehicle.manufacturerModel?.modelCode);
  const valuation = hasListPrice(vehicle) ? vehicle.depreciation : null;
  const hasHistory = !!vehicle.history;
  const hasEnergy = !!vehicle.energyCost;
  const hasFleet = (vehicle.modelFleet?.registeredCount ?? 0) > 0;
  const hasPopularity = (vehicle.modelPopularity?.months.length ?? 0) > 0 || hasFleet;
  const nav = NAV.filter(
    (n) =>
      (n.id !== "value" || valuation) &&
      (n.id !== "history" || hasHistory) &&
      (n.id !== "energy" || hasEnergy) &&
      (n.id !== "popularity" || hasPopularity),
  );

  return (
    <div className={`result ${vehicle.isPermanentlyCancelled ? "result--cancelled" : ""}`}>
      {vehicle.isPermanentlyCancelled && <CancelledRibbon />}

      <nav className="anchor-nav no-print" aria-label={he.nav.label}>
        <ul>
          {nav.map((n) => (
            <li key={n.id}>
              <a
                href={`#${n.id}`}
                onClick={(e) => {
                  e.preventDefault();
                  scrollToAnchor(n.id);
                }}
              >
                {n.label}
              </a>
            </li>
          ))}
        </ul>
      </nav>

      <div className="container result-layout">
        <div className="print-header print-only">
          {he.app.name} · {he.footer.printedAt(new Date().toLocaleDateString("he-IL"))}
        </div>

        {cached && (
          <p className="cached-note" role="status">
            <span className="badge badge--warn">{he.states.cachedBadge}</span>
            {he.states.cachedText}
          </p>
        )}

        <Reveal index={0}>
          <VehicleHero vehicle={vehicle} inComparison={inComparison} onCompare={onCompare} onPrint={onPrint} />
        </Reveal>

        <StatusBanner statuses={statuses} alerts={alerts} />

        <div className="result-grid">
          <aside className="result-grid__aside">
            <div className="sticky-stack">
              <BuyerSummary
                verdict={summary.verdict}
                flags={summary.flags}
                depreciation={valuation}
                valueDisabled={offRoad}
              />
            </div>
          </aside>

          <div className="result-grid__main">
            {recalls.length > 0 && (
              <Reveal index={1}>
                <RecallsCard recalls={recalls} />
              </Reveal>
            )}

            {valuation && (
              <Reveal index={2}>
                <ValuationPanel depreciation={valuation} mutedReason={mutedReason} />
              </Reveal>
            )}

            {hasHistory && (
              <Reveal index={3}>
                <HistorySection vehicle={vehicle} />
              </Reveal>
            )}

            {hasPopularity && (
              <Reveal index={4}>
                <PopularityCard vehicle={vehicle} />
              </Reveal>
            )}

            <Reveal index={5}>
              <SpecsSection vehicle={vehicle} printMode={printMode} />
            </Reveal>

            {vehicle.energyCost && (
              <Reveal index={6}>
                <EnergyCostCard
                  key={vehicle.registrationNumber ?? `${manufacturerCode}/${modelCode}`}
                  initial={vehicle.energyCost}
                  manufacturerCode={manufacturerCode}
                  modelCode={modelCode}
                />
              </Reveal>
            )}

            <AnotherSearch onSearch={onSearch} />

            <p className="back-top no-print">
              <button type="button" className="link-btn" onClick={() => window.scrollTo({ top: 0, behavior: "smooth" })}>
                <Icon name="arrowUp" size={16} />
                {he.app.backToTop}
              </button>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
