import { useLayoutEffect, useRef, useState } from "react";
import { formatMonthYear, formatNumber } from "../../lib/format";
import { he } from "../../strings.he";
import type { VehicleModelMonthlyCount } from "../../types/vehicle";

type MonthlyCountChartProps = {
  points: VehicleModelMonthlyCount[];
  /** Short sparkline for a comparison cell: no axes, scaled to the cell. */
  compact?: boolean;
};

export function MonthlyCountChart({ points, compact = false }: MonthlyCountChartProps) {
  const scrollRef = useRef<HTMLDivElement>(null);
  const [active, setActive] = useState<number | null>(null);

  useLayoutEffect(() => {
    const el = scrollRef.current;
    if (el) el.scrollLeft = el.scrollWidth;
  }, [points]);

  if (points.length === 0) return null;

  const max = Math.max(...points.map((p) => p.count), 1);
  const padL = compact ? 2 : 52;
  const padR = compact ? 2 : 12;
  const padT = compact ? 2 : 10;
  const padB = compact ? 2 : 22;
  const height = compact ? 52 : 176;
  const slot = compact ? 0 : points.length > 96 ? 8 : points.length > 48 ? 12 : 18;
  const plotW = compact ? 160 : Math.max(280, points.length * slot);
  const width = plotW + padL + padR;
  const plotH = height - padT - padB;
  const slotW = plotW / points.length;
  const barW = Math.max(compact ? 1 : 2, slotW * 0.62);
  const baseline = padT + plotH;

  const y = (count: number) => baseline - (count / max) * plotH;

  const yearLabels: { x: number; label: string }[] = [];
  if (!compact) {
    let lastX = -1e9;
    points.forEach((point, i) => {
      const month = point.month % 100;
      if (month !== 1 && i !== 0) return;
      const x = padL + i * slotW + slotW / 2;
      if (x - lastX < 36) return;
      yearLabels.push({ x, label: String(Math.floor(point.month / 100)) });
      lastX = x;
    });
  }

  const shown = active != null && points[active] ? points[active] : points[points.length - 1];
  const shownLabel = formatMonthYear(String(shown.month));

  const svg = (
    <svg
      className={`month-chart ${compact ? "month-chart--compact" : ""}`}
      viewBox={`0 0 ${width} ${height}`}
      width={compact ? undefined : width}
      height={compact ? undefined : height}
      role={compact ? undefined : "img"}
      aria-hidden={compact ? true : undefined}
      aria-label={compact ? undefined : he.popularity.chartAria(formatNumber(points.reduce((sum, p) => sum + p.count, 0), 0))}
    >
      {!compact && (
        <>
          <line className="month-chart__grid" x1={padL} x2={width - padR} y1={y(max)} y2={y(max)} />
          <line className="month-chart__axis" x1={padL} x2={width - padR} y1={baseline} y2={baseline} />
          <text className="month-chart__label" x={padL - 6} y={y(max) + 4} textAnchor="end">
            {formatNumber(max, 0)}
          </text>
          {yearLabels.map((label) => (
            <text key={label.label + label.x} className="month-chart__year" x={label.x} y={height - 4} textAnchor="middle">
              {label.label}
            </text>
          ))}
        </>
      )}
      {points.map((point, i) => {
        const barH = Math.max(point.count > 0 ? 2 : 1, baseline - y(point.count));
        const x = padL + i * slotW + (slotW - barW) / 2;
        const monthLabel = formatMonthYear(String(point.month));
        const isPeak = point.count === max && point.count > 0;
        return (
          <g key={point.month}>
            <rect
              className="month-chart__hit"
              x={padL + i * slotW}
              y={padT}
              width={slotW}
              height={plotH}
              onMouseEnter={() => setActive(i)}
              onFocus={() => setActive(i)}
              onClick={() => setActive(i)}
            >
              {monthLabel && <title>{he.popularity.monthCount(monthLabel, formatNumber(point.count, 0))}</title>}
            </rect>
            <rect
              className={["month-chart__bar", isPeak ? "is-peak" : "", point.count === 0 ? "is-zero" : "", active === i ? "is-active" : ""]
                .filter(Boolean)
                .join(" ")}
              x={x}
              y={baseline - barH}
              width={barW}
              height={barH}
              rx={Math.min(2, barW / 2)}
              pointerEvents="none"
            />
          </g>
        );
      })}
    </svg>
  );

  if (compact) return svg;

  return (
    <figure
      className="popularity-chart"
      onMouseLeave={() => setActive(null)}
    >
      {shownLabel && (
        <p className="popularity-chart__readout">
          {he.popularity.monthCount(shownLabel, formatNumber(shown.count, 0))}
        </p>
      )}
      <div className="popularity-chart__scroll" ref={scrollRef}>
        {svg}
      </div>
    </figure>
  );
}
