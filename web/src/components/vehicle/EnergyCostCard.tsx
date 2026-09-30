import { useEffect, useRef, useState } from "react";
import { lookupEnergyCost } from "../../api/vehicles";
import { formatCurrency, formatKm, formatNumber, formatUnitPrice } from "../../lib/format";
import { he } from "../../strings.he";
import type { VehicleEnergyCost } from "../../types/energyCost";
import { Icon } from "../Icon";
import { SectionHeader } from "../ui";

const SLIDER_MIN = 0;
const SLIDER_MAX = 5000;
const SLIDER_STEP = 100;
const DEBOUNCE_MS = 400;

type EnergyCostCardProps = {
  /** Default-mileage cost embedded in the vehicle lookup response. */
  initial: VehicleEnergyCost;
  /** Needed to recalculate for a different mileage; the slider is disabled without them. */
  manufacturerCode: string | null;
  modelCode: string | null;
};

type CostState = {
  status: "idle" | "loading" | "error";
  data: VehicleEnergyCost;
};

export function EnergyCostCard({ initial, manufacturerCode, modelCode }: EnergyCostCardProps) {
  const [km, setKm] = useState(initial.kmPerMonth);
  const [requestedKm, setRequestedKm] = useState(initial.kmPerMonth);
  const [state, setState] = useState<CostState>({ status: "idle", data: initial });
  const loadedKm = useRef(initial.kmPerMonth);
  const canRecalculate = !!manufacturerCode && !!modelCode;
  const sliderMax = Math.max(SLIDER_MAX, Math.ceil(initial.kmPerMonth / SLIDER_STEP) * SLIDER_STEP);

  useEffect(() => {
    if (!manufacturerCode || !modelCode || requestedKm === loadedKm.current) {
      setState((prev) => (prev.status === "idle" ? prev : { status: "idle", data: prev.data }));
      return;
    }
    const controller = new AbortController();
    setState((prev) => ({ status: "loading", data: prev.data }));
    lookupEnergyCost(manufacturerCode, modelCode, requestedKm, controller.signal)
      .then((data) => {
        loadedKm.current = data.kmPerMonth;
        setState({ status: "idle", data });
      })
      .catch(() => {
        if (controller.signal.aborted) return;
        setState((prev) => ({ status: "error", data: prev.data }));
      });
    return () => controller.abort();
  }, [manufacturerCode, modelCode, requestedKm]);

  useEffect(() => {
    const timer = window.setTimeout(() => setRequestedKm(km), DEBOUNCE_MS);
    return () => window.clearTimeout(timer);
  }, [km]);

  const data = state.data;
  const isElectric = data.energyType === "Electric";
  const calculable = data.energyType !== "Unknown" && data.monthlyCost != null;
  const note = !calculable ? he.energy.noteUnknown : isElectric ? he.energy.noteElectric : he.energy.noteFuel;

  return (
    <section
      id="energy"
      className={`card energy-cost ${state.status === "loading" ? "is-loading" : ""}`}
      aria-labelledby="energy-title"
      aria-busy={state.status === "loading"}
    >
      <SectionHeader
        icon={isElectric ? "bolt" : "fuel"}
        id="energy-title"
        title={he.energy.title}
        subtitle={he.energy.subtitle}
      />

      <div className="energy-cost__body">
        <div className="energy-cost__km">
          <div className="energy-cost__km-head">
            <label className="energy-cost__label" htmlFor="energy-km">
              {he.energy.kmLabel}
            </label>
            <output className="energy-cost__km-value" htmlFor="energy-km">
              {formatKm(km)}
            </output>
          </div>
          <input
            id="energy-km"
            className="energy-cost__slider"
            type="range"
            min={SLIDER_MIN}
            max={sliderMax}
            step={SLIDER_STEP}
            value={km}
            disabled={!canRecalculate}
            onChange={(e) => setKm(Number(e.target.value))}
            aria-valuetext={formatKm(km) ?? undefined}
          />
          <div className="energy-cost__scale" aria-hidden="true">
            <span>{formatNumber(SLIDER_MIN, 0)}</span>
            <span>{formatNumber(sliderMax, 0)}</span>
          </div>
        </div>

        {calculable && (
          <dl className="energy-cost__rows">
            <div className="energy-cost__row">
              <dt>{isElectric ? he.energy.electricConsumption : he.energy.fuelConsumption}</dt>
              <dd>
                {formatNumber(data.consumption!, 1)} {isElectric ? he.energy.kwhPer100Km : he.energy.kmPerLiter}
              </dd>
            </div>
            <div className="energy-cost__row">
              <dt>{isElectric ? he.energy.electricPrice : he.energy.fuelPrice}</dt>
              <dd>
                {formatUnitPrice(data.energyPrice)} {isElectric ? he.energy.perKwh : he.energy.perLiter}
              </dd>
            </div>
          </dl>
        )}

        <div className="big-number energy-cost__total">
          <span className="big-number__label">
            {he.energy.monthlyCost}
            <span className="badge badge--neutral">{he.energy.estimateOnly}</span>
          </span>
          {calculable ? (
            <strong className="big-number__value">{formatCurrency(data.monthlyCost!)}</strong>
          ) : (
            <strong className="energy-cost__na">{he.energy.cannotCalculate}</strong>
          )}
        </div>

        {state.status === "error" && (
          <p className="info-note info-note--bad" role="alert">
            <Icon name="warning" size={18} />
            {he.energy.error}
          </p>
        )}

        <p className="fine-print energy-cost__note">
          {note}
          {calculable && (
            <>
              <br />
              {he.energy.defaultsNote}
            </>
          )}
        </p>
      </div>
    </section>
  );
}
