import { Fragment, useMemo, useState, type FormEvent } from "react";
import { ApiError } from "../api/client";
import { lookupVehicle } from "../api/vehicles";
import type { Comparison } from "../hooks/useComparison";
import { buildComparison, type CompareSection } from "../lib/compare";
import { clean, needsLtr, vehicleTitle } from "../lib/format";
import { formatPlate, formatPlateInput, isValidPlate, normalizePlate, PLATE_MAX_DIGITS } from "../lib/plate";
import { he } from "../strings.he";
import { Icon } from "./Icon";
import { ManufacturerLogo } from "./ManufacturerLogo";
import { LogoLottie } from "./LogoLottie";
import { Ltr, Plate } from "./ui";
import { MonthlyCountChart } from "./vehicle/MonthlyCountChart";

type CompareViewProps = {
  comparison: Comparison;
  onOpenVehicle: (plate: string) => void;
};

export function CompareView({ comparison, onOpenVehicle }: CompareViewProps) {
  const { items } = comparison;
  const [plate, setPlate] = useState("");
  const [adding, setAdding] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [onlyDiff, setOnlyDiff] = useState(false);
  const model = useMemo(() => buildComparison(items), [items]);

  async function handleAdd(event: FormEvent) {
    event.preventDefault();
    if (comparison.isFull) {
      setError(he.compare.limit);
      return;
    }
    if (!isValidPlate(plate)) {
      setError(he.search.invalid);
      return;
    }
    const digits = normalizePlate(plate);
    if (comparison.has(digits)) {
      setError(he.compare.duplicate);
      return;
    }
    setAdding(true);
    setError(null);
    try {
      const vehicle = await lookupVehicle(digits);
      const result = comparison.add(vehicle);
      if (result === "duplicate") setError(he.compare.duplicate);
      else if (result === "full") setError(he.compare.limit);
      else setPlate("");
    } catch (cause) {
      if (cause instanceof ApiError && cause.kind === "notFound") setError(he.states.notFoundTitle(formatPlate(digits)));
      else if (cause instanceof ApiError && cause.kind === "upstream") setError(he.states.upstreamTitle);
      else if (cause instanceof ApiError && cause.kind === "network") setError(he.states.networkTitle);
      else setError(he.states.genericText);
    } finally {
      setAdding(false);
    }
  }

  const visibleRows = model.rows.filter((r) => !onlyDiff || r.differs);
  const sections: CompareSection[] = [];
  for (const r of visibleRows) if (!sections.includes(r.section)) sections.push(r.section);

  return (
    <section className="container compare-page" aria-labelledby="compare-title">
      <div className="compare-page__head">
        <div className="compare-page__heading">
          <h1 id="compare-title" className="compare-page__title">
            <Icon name="compare" size={26} />
            {he.compare.title}
            <span className="badge badge--neutral">{he.compare.myCount(items.length, comparison.limit)}</span>
          </h1>
          <p className="compare__subtitle">{he.compare.subtitle}</p>
        </div>
      </div>

      <div className="card compare-page__card">
        <div className="compare__toolbar">
          <form className="compare__add" onSubmit={handleAdd} noValidate>
            <label htmlFor="compare-plate" className="sr-only">
              {he.compare.addLabel}
            </label>
            <div className={`compare__input plate-input ${error ? "is-invalid" : ""}`}>
              <input
                id="compare-plate"
                type="text"
                inputMode="numeric"
                autoComplete="off"
                dir="ltr"
                maxLength={12}
                placeholder={he.search.placeholder}
                value={formatPlateInput(plate)}
                onChange={(e) => {
                  setPlate(normalizePlate(e.target.value).slice(0, PLATE_MAX_DIGITS));
                  setError(null);
                }}
                disabled={adding || comparison.isFull}
                autoFocus={items.length === 0}
                aria-describedby={error ? "compare-error" : undefined}
                aria-invalid={!!error || undefined}
              />
            </div>
            <button type="submit" className="btn btn--primary" disabled={adding || comparison.isFull}>
              {adding ? <LogoLottie size="inline" /> : <Icon name="plus" size={18} />}
              {adding ? he.compare.adding : he.compare.addButton}
            </button>
          </form>
          <div className="compare__toolbar-end">
            <label className="switch">
              <input type="checkbox" checked={onlyDiff} onChange={(e) => setOnlyDiff(e.target.checked)} />
              <span className="switch__track" aria-hidden="true" />
              {he.compare.onlyDiff}
            </label>
            {items.length > 0 && (
              <button type="button" className="link-btn" onClick={comparison.clear}>
                <Icon name="trash" size={16} />
                {he.compare.clear}
              </button>
            )}
          </div>
        </div>
        <p id="compare-error" className="field-error" role="alert">
          {error ?? (comparison.isFull ? he.compare.limit : "")}
        </p>

        {items.length < 2 && (
          <p className="info-note">
            <Icon name="info" size={18} />
            {items.length === 0 ? he.compare.emptyNone : he.compare.emptyOne}
          </p>
        )}

        {items.length >= 2 && (
          <div className={`best ${model.best == null ? "best--none" : ""}`} role="status">
            <span className="best__icon" aria-hidden="true">
              <Icon name="trophy" size={26} />
            </span>
            <div className="best__body">
              <p className="best__title">{he.compare.bestTitle}</p>
              {model.best != null ? (
                <>
                  <p className="best__vehicle">
                    <Plate number={items[model.best].registrationNumber} small />
                    {vehicleTitle(items[model.best])}
                  </p>
                  {model.reasons.length > 0 && (
                    <p className="best__why">
                      <strong>{he.compare.bestWhy}</strong> {model.reasons.join(" · ")}
                    </p>
                  )}
                </>
              ) : (
                <p className="best__why">{he.compare.noBest}</p>
              )}
              <p className="best__note">{he.compare.bestNote}</p>
            </div>
          </div>
        )}

        {items.length > 0 && (
          <div className="compare-table-wrap" tabIndex={0} aria-label={he.compare.title}>
            <table className="compare-table" style={{ ["--cols" as string]: items.length }}>
              <thead>
                <tr>
                  <td className="compare-table__corner" />
                  {items.map((v, i) => {
                    const p = String(v.registrationNumber ?? "");
                    return (
                      <th key={p || i} scope="col" className={i === model.best ? "is-best" : undefined}>
                        <div className="compare-col">
                          <button
                            type="button"
                            className="compare-col__open"
                            onClick={() => onOpenVehicle(p)}
                            title={he.compare.openVehicle}
                          >
                            <Plate number={p} small />
                          </button>
                          <ManufacturerLogo
                            logoSlug={v.manufacturerModel?.logoSlug}
                            name={clean(v.manufacturerName) ?? clean(v.manufacturerModel?.manufacturerName)}
                            className="compare-col__logo"
                          />
                          <span className="compare-col__title">{vehicleTitle(v)}</span>
                          <span className="compare-col__points">
                            {he.compare.points(model.points[i])}
                            {i === model.best && <Icon name="trophy" size={14} />}
                          </span>
                          {model.excluded[i] && <span className="badge badge--bad">{he.compare.excluded}</span>}
                          <button
                            type="button"
                            className="link-btn compare-col__remove"
                            onClick={() => comparison.remove(p)}
                            aria-label={he.compare.removeAria(formatPlate(p))}
                          >
                            <Icon name="close" size={14} />
                            {he.compare.remove}
                          </button>
                        </div>
                      </th>
                    );
                  })}
                </tr>
              </thead>
              <tbody>
                {sections.map((section) => (
                  <Fragment key={section}>
                    <tr className="compare-table__section">
                      <th scope="colgroup" colSpan={items.length + 1}>
                        {he.compare.sections[section]}
                      </th>
                    </tr>
                    {visibleRows
                      .filter((r) => r.section === section)
                      .map((row) => (
                        <Fragment key={row.id}>
                          <tr
                            className={
                              [
                                row.differs && items.length > 1 ? "is-diff" : "",
                                row.insight ? "has-insight" : "",
                                row.series.some((series) => series && series.length > 0) ? "has-chart" : "",
                              ]
                                .filter(Boolean)
                                .join(" ") || undefined
                            }
                          >
                            <th scope="row">{row.label}</th>
                            {row.values.map((value, i) => {
                              const icon = value == null ? null : row.icons[i];
                              const series = row.series[i];
                              return (
                                <td key={i} className={row.winners.has(i) ? "is-winner" : undefined}>
                                  <div className="compare-cell">
                                    <span className="compare-cell__line">
                                      {icon && (
                                        <span className={`cell-icon cell-icon--${icon.tone}`}>
                                          <Icon name={icon.name} size={16} label={icon.label} />
                                        </span>
                                      )}
                                      {value == null ? (
                                        <span className="missing">{he.compare.missing}</span>
                                      ) : needsLtr(value) ? (
                                        <Ltr>{value}</Ltr>
                                      ) : (
                                        value
                                      )}
                                      {row.winners.has(i) && (
                                        <span className="winner-mark">
                                          <Icon name="check" size={14} label={he.compare.winnerAria} />
                                        </span>
                                      )}
                                    </span>
                                    {series && series.length > 0 && <MonthlyCountChart points={series} compact />}
                                  </div>
                                </td>
                              );
                            })}
                          </tr>
                          {row.insight && (
                            <tr className="compare-table__insight">
                              <td colSpan={items.length + 1}>
                                <p className="compare-insight">
                                  <Icon name="sparkle" size={16} />
                                  <span>{row.insight}</span>
                                </p>
                              </td>
                            </tr>
                          )}
                        </Fragment>
                      ))}
                  </Fragment>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  );
}
