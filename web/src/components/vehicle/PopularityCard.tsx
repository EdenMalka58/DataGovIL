import { formatMonthYear, formatNumber, formatPercent, toNumber } from "../../lib/format";
import { isOffRoad, vehicleYearFleet } from "../../lib/viewModel";
import { he } from "../../strings.he";
import type { VehicleModelYearFleet, VehicleRecord } from "../../types/vehicle";
import { Icon } from "../Icon";
import { SectionHeader, Ltr } from "../ui";
import { MonthlyCountChart } from "./MonthlyCountChart";

type PopularityCardProps = {
  vehicle: VehicleRecord;
};

type FleetBand = "high" | "mid" | "low";

function fleetBand(fleet: VehicleModelYearFleet): FleetBand {
  const rate = fleet.activeSharePercent ?? 0;
  const age = new Date().getFullYear() - fleet.modelYear;
  const high = age <= 5 ? 92 : age <= 12 ? 85 : 75;
  const low = age <= 5 ? 80 : age <= 12 ? 70 : 55;
  if (rate >= high) return "high";
  if (rate < low) return "low";
  return "mid";
}

function fleetConclusion(vehicle: VehicleRecord): { text: string; band: FleetBand } | null {
  const fleet = vehicleYearFleet(vehicle) ?? (vehicle.modelFleet?.years.length === 1 ? vehicle.modelFleet.years[0] : null);
  if (!fleet || fleet.registeredCount <= 0 || fleet.activeSharePercent == null) return null;
  const rate = formatPercent(fleet.activeSharePercent, false);
  const band = fleetBand(fleet);
  const offRoad = isOffRoad(vehicle);
  const text = offRoad
    ? band === "low"
      ? he.fleet.insightOffLow(rate)
      : he.fleet.insightOff(rate)
    : band === "high"
      ? he.fleet.insightOnHigh(rate)
      : band === "low"
        ? he.fleet.insightOnLow(rate)
        : he.fleet.insightMid(rate);
  return { text, band };
}

export function PopularityCard({ vehicle }: PopularityCardProps) {
  const popularity = vehicle.modelPopularity;
  const fleet = vehicle.modelFleet;
  const months = popularity?.months ?? [];
  const hasMonths = months.length > 0;
  const hasFleet = !!fleet && fleet.registeredCount > 0 && fleet.activeSharePercent != null;
  if (!hasMonths && !hasFleet) return null;

  const peak = hasMonths ? months.reduce((best, point) => (point.count > best.count ? point : best)) : null;
  const peakMonth = peak ? formatMonthYear(String(peak.month)) : null;
  const first = hasMonths ? formatMonthYear(String(months[0].month)) : null;
  const last = hasMonths ? formatMonthYear(String(months[months.length - 1].month)) : null;
  const conclusion = fleetConclusion(vehicle);

  return (
    <section id="popularity" className="card popularity" aria-labelledby="popularity-title">
      <SectionHeader
        icon="chart"
        id="popularity-title"
        title={he.popularity.title}
        subtitle={hasMonths ? he.popularity.subtitle : he.fleet.note}
      />

      {hasMonths && popularity && (
        <>
          <div className="popularity__head">
            <div className="big-number">
              <span className="big-number__label">{he.popularity.totalLabel}</span>
              <span className="big-number__value">
                <Ltr>{formatNumber(popularity.totalCount, 0)}</Ltr>
              </span>
            </div>
            {peakMonth && peak && peak.count > 0 && (
              <p className="popularity__peak">{he.popularity.peak(peakMonth, formatNumber(peak.count, 0))}</p>
            )}
          </div>

          <MonthlyCountChart points={months} />

          <p className="popularity__note">
            {first && last ? `${he.popularity.range(first, last)}. ` : ""}
            {he.popularity.note}
          </p>
        </>
      )}

      {hasFleet && fleet && (
        <FleetBlock vehicle={vehicle} split={hasMonths} conclusion={conclusion} />
      )}
    </section>
  );
}

function FleetBlock({
  vehicle,
  split,
  conclusion,
}: {
  vehicle: VehicleRecord;
  split: boolean;
  conclusion: { text: string; band: FleetBand } | null;
}) {
  const fleet = vehicle.modelFleet!;
  const years = fleet.years;
  const several = years.length > 1;
  const vehicleYear = toNumber(vehicle.manufactureYear);
  const span =
    several && fleet.activeSharePercent != null
      ? he.fleet.modelSpan(formatNumber(fleet.registeredCount, 0), formatPercent(fleet.activeSharePercent, false))
      : null;

  return (
    <div className={`fleet ${split ? "fleet--split" : ""}`}>
      <h3 className="fleet__title">
        {several ? he.fleet.allTitle : he.fleet.title(String(years[0].modelYear))}
      </h3>

      <dl className="fleet-stats">
        <div className="fleet-stat">
          <dt>{he.fleet.registered}</dt>
          <dd>
            <Ltr>{formatNumber(fleet.registeredCount, 0)}</Ltr>
          </dd>
        </div>
        <div className="fleet-stat fleet-stat--active">
          <dt>{he.fleet.active}</dt>
          <dd>
            <Ltr>{formatNumber(fleet.activeCount, 0)}</Ltr>
          </dd>
        </div>
        <div className="fleet-stat fleet-stat--inactive">
          <dt>{he.fleet.inactive}</dt>
          <dd>
            <Ltr>{formatNumber(fleet.inactiveCount, 0)}</Ltr>
          </dd>
        </div>
      </dl>

      <div className="fleet__rate">
        <p className="fleet__rate-label">{several ? he.fleet.rateAll : he.fleet.rate}</p>
        <p className="fleet__formula">
          <Ltr>
            {formatNumber(fleet.activeCount, 0)} / {formatNumber(fleet.registeredCount, 0)} ={" "}
            {formatNumber(fleet.activeSharePercent!, 1)}%
          </Ltr>
        </p>
        <div
          className="fleet-bar"
          role="img"
          aria-label={he.fleet.barAria(formatNumber(fleet.activeCount, 0), formatNumber(fleet.inactiveCount, 0))}
        >
          <span className="fleet-bar__active" style={{ flex: fleet.activeCount }} />
          <span className="fleet-bar__inactive" style={{ flex: fleet.inactiveCount }} />
        </div>
      </div>

      {several && (
        <div className="fleet-years-wrap">
          <table className="fleet-years">
            <caption className="sr-only">{he.fleet.yearsAria}</caption>
            <thead>
              <tr>
                <th scope="col">{he.fleet.colYear}</th>
                <th scope="col">{he.fleet.colRegistered}</th>
                <th scope="col">{he.fleet.colActive}</th>
                <th scope="col">{he.fleet.colInactive}</th>
                <th scope="col">{he.fleet.colRate}</th>
              </tr>
            </thead>
            <tbody>
              {years.map((year) => (
                <YearRow key={year.modelYear} year={year} isVehicle={year.modelYear === vehicleYear} />
              ))}
            </tbody>
            <tfoot>
              <tr>
                <th scope="row">{he.fleet.total}</th>
                <td>
                  <Ltr>{formatNumber(fleet.registeredCount, 0)}</Ltr>
                </td>
                <td>
                  <Ltr>{formatNumber(fleet.activeCount, 0)}</Ltr>
                </td>
                <td>
                  <Ltr>{formatNumber(fleet.inactiveCount, 0)}</Ltr>
                </td>
                <td>
                  <Ltr>{formatPercent(fleet.activeSharePercent ?? 0, false)}</Ltr>
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      )}

      {conclusion && (
        <p className={`fleet-insight fleet-insight--${conclusion.band}`}>
          <Icon name="sparkle" size={16} />
          <span>
            {conclusion.text}
            {span ? ` ${span}` : ""}
          </span>
        </p>
      )}

      <p className="popularity__note">{several ? he.fleet.noteAll : he.fleet.note}</p>
    </div>
  );
}

function YearRow({ year, isVehicle }: { year: VehicleModelYearFleet; isVehicle: boolean }) {
  return (
    <tr className={isVehicle ? "is-vehicle" : undefined}>
      <th scope="row">
        <Ltr>{year.modelYear}</Ltr>
        {isVehicle && <span className="fleet-years__mark">{he.fleet.thisYear}</span>}
      </th>
      <td>
        <Ltr>{formatNumber(year.registeredCount, 0)}</Ltr>
      </td>
      <td>
        <Ltr>{formatNumber(year.activeCount, 0)}</Ltr>
      </td>
      <td>
        <Ltr>{formatNumber(year.inactiveCount, 0)}</Ltr>
      </td>
      <td>
        <Ltr>{year.activeSharePercent == null ? "—" : formatPercent(year.activeSharePercent, false)}</Ltr>
      </td>
    </tr>
  );
}
