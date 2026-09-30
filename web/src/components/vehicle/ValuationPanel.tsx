import { useState } from "react";
import { useCountUp, useInView } from "../../hooks/useMotion";
import {
  formatAge,
  formatCurrency,
  formatKm,
  formatNumber,
  formatPercent,
  formatSignedCurrency,
} from "../../lib/format";
import { biggestImpactIndex } from "../../lib/viewModel";
import { he } from "../../strings.he";
import type { DepreciationFactor, VehicleDepreciationRecord } from "../../types/vehicle";
import { Icon, type IconName } from "../Icon";
import { Modal, SectionHeader } from "../ui";

const FACTOR_ICON: Record<DepreciationFactor, IconName> = {
  Kilometers: "odometer",
  OwnerCount: "users",
  OwnershipType: "building",
};

type ValuationPanelProps = {
  depreciation: VehicleDepreciationRecord;
  mutedReason: string | null;
};

export function ValuationPanel({ depreciation, mutedReason }: ValuationPanelProps) {
  const [explainOpen, setExplainOpen] = useState(false);
  const [ref, inView] = useInView<HTMLDivElement>();
  const d = depreciation;
  const hasPrice = d.listPrice != null && Number.isFinite(d.listPrice);
  const lines = Array.isArray(d.lines) ? d.lines : [];
  const biggest = biggestImpactIndex(lines);
  const estimated = hasPrice ? (d.estimatedValue ?? d.listPrice! + (d.depreciationValue ?? 0)) : null;
  const animatedValue = useCountUp(estimated, inView);
  const animatedPercent = useCountUp(d.depreciationPercent, inView);
  const percentTone = d.depreciationPercent > 0 ? "up" : d.depreciationPercent < 0 ? "down" : "flat";
  const maxAbs = Math.max(0.0001, ...lines.map((l) => Math.abs(l.percent)));
  const perYear =
    d.kilometers != null && d.carAge >= 0.5 ? formatNumber(Math.round(d.kilometers / d.carAge / 100) * 100, 0) : null;

  return (
    <section
      id="value"
      className={`card valuation ${mutedReason ? "is-muted" : ""}`}
      aria-labelledby="value-title"
    >
      <SectionHeader
        icon="currency"
        id="value-title"
        title={he.value.title}
        subtitle={he.value.subtitle}
        actions={
          <button type="button" className="btn btn--ghost btn--sm" onClick={() => setExplainOpen(true)}>
            <Icon name="help" size={18} />
            {he.value.explainButton}
          </button>
        }
      />

      {mutedReason && (
        <p className="muted-note">
          <Icon name="info" size={18} />
          {mutedReason}
        </p>
      )}

      <div ref={ref} className={`valuation__body ${inView ? "is-visible" : ""}`}>
        <div className="valuation__headline">
          <div className="big-number">
            <span className="big-number__label">
              {hasPrice ? he.value.estimatedValue : he.value.totalChange}
              <span className="badge badge--neutral">{he.value.estimateOnly}</span>
            </span>
            <span className="big-number__row">
              {hasPrice && animatedValue != null ? (
                <strong className="big-number__value">
                  {formatCurrency(Math.round(animatedValue))}
                </strong>
              ) : null}
              <span className={`delta delta--lg delta--${percentTone}`}>
                <Icon name={percentTone === "down" ? "trendDown" : percentTone === "up" ? "trendUp" : "flat"} size={20} />
                {formatPercent(animatedPercent ?? d.depreciationPercent)}
                <span className="sr-only">
                  {percentTone === "down" ? he.value.lowers : percentTone === "up" ? he.value.raises : he.value.neutral}
                </span>
              </span>
            </span>
            {hasPrice && (
              <span className="big-number__meta">
                {he.value.listPrice}: {formatCurrency(d.listPrice!)}
                {d.depreciationValue != null && (
                  <>
                    {" · "}
                    {he.value.totalChange}: {formatSignedCurrency(d.depreciationValue)}
                  </>
                )}
              </span>
            )}
            <span className="big-number__note">{he.value.notPurchasePrice}</span>
          </div>
        </div>

        {!hasPrice && (
          <p className="info-note">
            <Icon name="info" size={18} />
            {he.value.noPrice}
          </p>
        )}

        {hasPrice && lines.length > 0 && (
          <Waterfall
            listPrice={d.listPrice!}
            estimated={estimated!}
            steps={lines.map((l) => ({
              label: he.enums.factor[l.factor] ?? l.factor,
              value: l.value ?? (d.listPrice! * l.percent) / 100,
            }))}
          />
        )}

        {lines.length > 0 && (
          <div className="factors">
            <h3 className="subheading">{he.value.factorsTitle}</h3>
            <ul className="factors__list">
              {lines.map((line, i) => {
                const tone = line.percent > 0 ? "up" : line.percent < 0 ? "down" : "flat";
                return (
                  <li key={`${line.factor}-${i}`} className={`factor factor--${tone}`}>
                    <span className="factor__icon" aria-hidden="true">
                      <Icon name={FACTOR_ICON[line.factor] ?? "info"} size={20} />
                    </span>
                    <span className="factor__body">
                      <span className="factor__name">
                        {he.enums.factor[line.factor] ?? line.factor}
                        {i === biggest && <span className="badge badge--accent">{he.value.biggestImpact}</span>}
                      </span>
                      {line.description && <span className="factor__desc">{line.description}</span>}
                      <span className="factor__bar" aria-hidden="true">
                        <span
                          className="factor__bar-fill"
                          style={{ ["--w" as string]: `${(Math.abs(line.percent) / maxAbs) * 100}%` }}
                        />
                      </span>
                    </span>
                    <span className="factor__nums">
                      <span className={`delta delta--${tone}`}>
                        <Icon name={tone === "down" ? "arrowDown" : tone === "up" ? "arrowUp" : "flat"} size={14} />
                        {formatPercent(line.percent)}
                        <span className="sr-only">
                          {tone === "down" ? he.value.lowers : tone === "up" ? he.value.raises : he.value.neutral}
                        </span>
                      </span>
                      {line.value != null && (
                        <span className="factor__value">
                          {formatSignedCurrency(line.value)}
                        </span>
                      )}
                    </span>
                  </li>
                );
              })}
            </ul>
          </div>
        )}

        <div className="basis">
          <h3 className="subheading">{he.value.basisTitle}</h3>
          <ul className="chips chips--basis">
            {d.carAge > 0 && (
              <li className="chip chip--stat">
                <Icon name="calendar" size={16} />
                <span className="chip__label">{he.value.basis.age}</span>
                <strong>{formatAge(d.carAge)}</strong>
              </li>
            )}
            {d.kilometers != null && (
              <li className="chip chip--stat">
                <Icon name="odometer" size={16} />
                <span className="chip__label">{he.value.basis.km}</span>
                <strong>
                  {formatKm(d.kilometers)}
                </strong>
                {perYear && <span className="chip__sub">{he.value.perYear(perYear)}</span>}
              </li>
            )}
            {d.ownerCount > 0 && (
              <li className="chip chip--stat">
                <Icon name="users" size={16} />
                <span className="chip__label">{he.value.basis.owners}</span>
                <strong>{d.ownerCount}</strong>
              </li>
            )}
            {d.vehicleCategory && (
              <li className="chip chip--stat">
                <Icon name="car" size={16} />
                <span className="chip__label">{he.value.basis.vehicleCategory}</span>
                <strong>{he.enums.vehicleCategory[d.vehicleCategory] ?? d.vehicleCategory}</strong>
              </li>
            )}
            {d.ownerCategory && (
              <li className="chip chip--stat">
                <Icon name="building" size={16} />
                <span className="chip__label">{he.value.basis.ownerCategory}</span>
                <strong>{he.enums.ownerCategory[d.ownerCategory] ?? d.ownerCategory}</strong>
              </li>
            )}
            {d.ownershipType && (
              <li className="chip chip--stat">
                <Icon name="key" size={16} />
                <span className="chip__label">{he.value.basis.ownershipType}</span>
                <strong>{d.ownershipType}</strong>
              </li>
            )}
            {d.originality && (
              <li className="chip chip--stat">
                <Icon name="document" size={16} />
                <span className="chip__label">{he.value.basis.originality}</span>
                <strong>{d.originality}</strong>
              </li>
            )}
          </ul>
        </div>
      </div>

      <Modal open={explainOpen} onClose={() => setExplainOpen(false)} title={he.value.explainTitle} icon="help">
        <ul className="explain-list">
          {he.value.explainBody.map((p) => (
            <li key={p}>{p}</li>
          ))}
        </ul>
      </Modal>
    </section>
  );
}

// ─── Waterfall chart ──────────────────────────────────────────────────────

type WaterfallProps = {
  listPrice: number;
  estimated: number;
  steps: { label: string; value: number }[];
};

function shortCurrency(v: number): string {
  return formatCurrency(Math.round(v));
}

function Waterfall({ listPrice, estimated, steps }: WaterfallProps) {
  const [ref, inView] = useInView<HTMLDivElement>();
  const W = 640;
  const H = 280;
  const top = 34;
  const bottom = 48;
  const side = 12;
  const innerH = H - top - bottom;
  const count = steps.length + 2;
  const slot = (W - side * 2) / count;
  const barW = Math.min(76, slot * 0.58);

  const levels: number[] = [listPrice];
  for (const s of steps) levels.push(levels[levels.length - 1] + s.value);
  const all = [...levels, estimated];
  const max = Math.max(...all);
  const min = Math.min(...all);
  const range = Math.max(max - min, max * 0.04);
  const domainMin = max === min ? 0 : Math.max(0, min - range * 1.4);
  const domainMax = max + (max - domainMin) * 0.06;
  const truncated = domainMin > 0;
  const y = (v: number) => top + ((domainMax - v) / (domainMax - domainMin)) * innerH;
  const baseline = top + innerH;
  // RTL: the first bar sits on the right.
  const cx = (i: number) => W - side - slot * (i + 0.5);

  type Bar = { x: number; y1: number; y2: number; kind: "total" | "up" | "down"; label: string; value: string };
  const bars: Bar[] = [
    { x: cx(0), y1: y(listPrice), y2: baseline, kind: "total", label: he.value.chartStart, value: shortCurrency(listPrice) },
  ];
  steps.forEach((s, i) => {
    const from = levels[i];
    const to = levels[i + 1];
    const a = y(Math.max(from, to));
    const b = Math.max(y(Math.min(from, to)), a + 3);
    bars.push({
      x: cx(i + 1),
      y1: a,
      y2: b,
      kind: s.value >= 0 ? "up" : "down",
      label: s.label,
      value: formatSignedCurrency(Math.round(s.value)),
    });
  });
  bars.push({
    x: cx(count - 1),
    y1: y(estimated),
    y2: baseline,
    kind: "total",
    label: he.value.chartEnd,
    value: shortCurrency(estimated),
  });

  return (
    <figure ref={ref} className={`waterfall ${inView ? "is-visible" : ""}`}>
      <figcaption className="subheading">{he.value.chartTitle}</figcaption>
      <svg viewBox={`0 0 ${W} ${H}`} role="img" aria-label={he.value.chartTitle} preserveAspectRatio="xMidYMid meet">
        <line x1={side} x2={W - side} y1={baseline} y2={baseline} className="waterfall__axis" />
        {bars.map((bar, i) => {
          const next = bars[i + 1];
          const connectorY = bar.kind === "down" ? bar.y2 : bar.y1;
          const isEstimate = i === bars.length - 1;
          return (
            <g key={i} className={`waterfall__bar waterfall__bar--${isEstimate ? "estimate" : bar.kind}`} style={{ ["--i" as string]: i }}>
              {next && (
                <line
                  className="waterfall__connector"
                  x1={bar.x - barW / 2}
                  x2={next.x + barW / 2}
                  y1={next.kind === "total" ? next.y1 : connectorY}
                  y2={next.kind === "total" ? next.y1 : connectorY}
                />
              )}
              <rect
                className="waterfall__rect"
                x={bar.x - barW / 2}
                y={bar.y1}
                width={barW}
                height={Math.max(3, bar.y2 - bar.y1)}
                rx={6}
                style={{ transformOrigin: bar.kind === "down" ? "top" : "bottom" }}
              />
              <text x={bar.x} y={bar.y1 - 10} className="waterfall__value" textAnchor="middle">
                {bar.value}
              </text>
              <text x={bar.x} y={baseline + 22} className="waterfall__label" textAnchor="middle">
                {bar.label}
              </text>
            </g>
          );
        })}
      </svg>
      {truncated && <p className="waterfall__note">{he.value.chartAxisNote}</p>}
    </figure>
  );
}
