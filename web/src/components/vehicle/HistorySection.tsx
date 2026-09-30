import { useCountUp, useInView } from "../../hooks/useMotion";
import {
  clean,
  formatDate,
  formatDuration,
  formatKm,
  formatMonthYear,
  formatNumber,
  monthsBetween,
  parseGovDate,
  todayInJerusalem,
} from "../../lib/format";
import {
  mileage,
  nonPrivateRowCount,
  ownershipTimeline,
  thresholds,
  type TimelineNode,
} from "../../lib/viewModel";
import { he } from "../../strings.he";
import type { VehicleRecord } from "../../types/vehicle";
import { Icon, type IconName } from "../Icon";
import { Ltr, Reveal, SectionHeader } from "../ui";
import { ownershipIcon } from "./visuals";

type FlagDef = { key: keyof typeof he.history.flags; icon: IconName; value: boolean | null | undefined; severity: "bad" | "warn" };

export function HistorySection({ vehicle }: { vehicle: VehicleRecord }) {
  const tech = vehicle.history?.technical ?? null;
  const timeline = ownershipTimeline(vehicle);
  const m = mileage(vehicle);
  const firstRegistration = formatDate(tech?.firstRegistrationDate);
  const firstRegistrationDate = parseGovDate(tech?.firstRegistrationDate);
  const ageMonths = firstRegistrationDate ? monthsBetween(firstRegistrationDate, todayInJerusalem()) : null;
  const originality = clean(tech?.originalityName);
  const lpg = tech?.lpgChangeIndicator;

  if (!tech && timeline.length === 0 && !m) {
    return (
      <section id="history" className="card" aria-labelledby="history-title">
        <SectionHeader icon="history" id="history-title" title={he.history.title} subtitle={he.history.subtitle} />
        <p className="info-note">
          <Icon name="info" size={18} />
          {he.history.noHistory}
        </p>
      </section>
    );
  }

  const flags: FlagDef[] = tech
    ? [
        { key: "structure", icon: "structure", value: tech.structureChangeIndicator, severity: "bad" },
        { key: "color", icon: "palette", value: tech.colorChangeIndicator, severity: "warn" },
        { key: "tires", icon: "tire", value: tech.tireChangeIndicator, severity: "warn" },
      ]
    : [];

  return (
    <section id="history" className="card history" aria-labelledby="history-title">
      <SectionHeader icon="history" id="history-title" title={he.history.title} subtitle={he.history.subtitle} />

      {flags.length > 0 && (
        <div className="history__block">
          <h3 className="subheading">{he.history.flagsTitle}</h3>
          <ul className="flag-cards">
            {flags.map((f, i) => {
              const state = f.value === true ? f.severity : f.value === false ? "ok" : "unknown";
              return (
                <Reveal as="li" key={f.key} index={i} className={`flag-card flag-card--${state}`}>
                  <span className="flag-card__icon" aria-hidden="true">
                    <Icon name={f.value === true ? "warning" : f.icon} size={24} />
                  </span>
                  <span className="flag-card__name">{he.history.flags[f.key]}</span>
                  <span className="flag-card__state">
                    {f.value === true
                      ? he.history.documented
                      : f.value === false
                        ? he.history.notDocumented
                        : he.history.unknown}
                  </span>
                </Reveal>
              );
            })}
          </ul>
          <p className="fine-print">{he.history.flagsNote}</p>
        </div>
      )}

      {(firstRegistration || originality || lpg != null || m) && (
        <div className="history__facts">
          {firstRegistration && (
            <div className="fact">
              <Icon name="calendar" size={20} />
              <span className="fact__label">{he.history.firstRegistration}</span>
              <strong className="fact__value">
                <Ltr>{firstRegistration}</Ltr>
              </strong>
              {ageMonths != null && ageMonths >= 0 && (
                <span className="fact__sub">{he.history.carAge(formatDuration(ageMonths))}</span>
              )}
            </div>
          )}
          {originality && (
            <div className="fact">
              <Icon name="document" size={20} />
              <span className="fact__label">{he.history.originality}</span>
              <strong className="fact__value">{originality}</strong>
            </div>
          )}
          {lpg != null && (
            <div className="fact">
              <Icon name="fuel" size={20} />
              <span className="fact__label">{he.history.lpg}</span>
              <strong className="fact__value">{lpg ? he.history.lpgInstalled : he.history.notDocumented}</strong>
              {lpg && <span className="fact__sub">{he.history.lpgNote}</span>}
            </div>
          )}
          {m && <Odometer km={m.km} perYear={m.perYear} />}
        </div>
      )}

      {timeline.length > 0 && <OwnershipTimeline vehicle={vehicle} nodes={timeline} />}
    </section>
  );
}

function Odometer({ km, perYear }: { km: number; perYear: number | null }) {
  const [ref, inView] = useInView<HTMLDivElement>();
  const animated = useCountUp(km, inView, 900);
  const avg = thresholds.nationalAvgKmPerYear;
  const ratio = perYear != null ? perYear / avg : null;
  const position = ratio != null ? Math.min(1, ratio / 2) : null;
  const comparison =
    ratio == null ? null : ratio > 1.15 ? he.history.aboveAvg : ratio < 0.85 ? he.history.belowAvg : he.history.nearAvg;
  const tone =
    ratio == null ? "" : ratio * avg > thresholds.highKmPerYear ? "warn" : ratio > 1.15 ? "neutral" : "ok";

  return (
    <div id="odometer" ref={ref} className="fact fact--odometer">
      <Icon name="odometer" size={20} />
      <span className="fact__label">{he.history.odometer}</span>
      <strong className="fact__value odometer__digits">
        {formatKm(Math.round(animated ?? km))}
      </strong>
      {perYear != null && (
        <>
          <span className="fact__sub">
            {he.history.odometerAvg(formatNumber(Math.round(perYear / 100) * 100, 0))}
            {comparison && <span className={`badge badge--${tone || "neutral"}`}>{comparison}</span>}
          </span>
          <span className="odometer__scale" aria-hidden="true">
            <span className="odometer__avg" />
            <span
              className={`odometer__marker ${inView ? "is-visible" : ""}`}
              style={{ ["--pos" as string]: `${(position ?? 0) * 100}%` }}
            />
          </span>
          <span className="fact__sub fact__sub--muted">
            {he.history.odometerVsNational(formatNumber(avg, 0))}
          </span>
        </>
      )}
    </div>
  );
}

function OwnershipTimeline({ vehicle, nodes }: { vehicle: VehicleRecord; nodes: TimelineNode[] }) {
  const nonPrivate = nonPrivateRowCount(vehicle);
  return (
    <div id="timeline" className="history__block">
      <h3 className="subheading subheading--row">
        <span>{he.history.timelineTitle}</span>
        <span className="badge badge--neutral">
          <Icon name="users" size={14} />
          {he.history.owners(nodes.length)}
        </span>
        {nonPrivate > 0 && (
          <span className="badge badge--warn">
            <Icon name="warning" size={14} />
            {he.history.nonPrivateCount(nonPrivate)}
          </span>
        )}
      </h3>
      <ol className="timeline">
        {nodes.map((node, i) => {
          const date = node.date ? formatMonthYear(node.raw.ownershipYearMonth) : clean(node.raw.ownershipYearMonth);
          return (
            <Reveal as="li" key={node.key} index={i} className={`timeline__item timeline__item--${node.tone}`}>
              <span className="timeline__dot" aria-hidden="true">
                <Icon name={ownershipIcon(node.kind)} size={16} />
              </span>
              <div className="timeline__content">
                <div className="timeline__top">
                  <strong className="timeline__type">{clean(node.raw.ownershipType) ?? he.common.unknown}</strong>
                  {node.isCurrent && <span className="badge badge--primary">{he.history.current}</span>}
                </div>
                <span className="timeline__date">{date ?? he.history.unknownDate}</span>
                {node.durationMonths != null && (
                  <span className="timeline__duration">
                    <Icon name="clock" size={14} />
                    {he.history.duration(formatDuration(node.durationMonths))}
                  </span>
                )}
              </div>
            </Reveal>
          );
        })}
      </ol>
    </div>
  );
}
