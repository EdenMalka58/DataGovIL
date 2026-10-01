import { useEffect, useMemo, useState, type ReactNode } from "react";
import { ApiError } from "../../api/client";
import { lookupPriceList } from "../../api/vehicles";
import { buildFieldGroups, groupCellCount, type FieldGroup, type GroupId } from "../../lib/fields";
import { clean, formatCurrency, toNumber } from "../../lib/format";
import { thresholds } from "../../lib/viewModel";
import { he } from "../../strings.he";
import type { VehiclePriceListRecord } from "../../types/priceList";
import type { VehicleRecord } from "../../types/vehicle";
import { Icon, type IconName } from "../Icon";
import { LogoLottie } from "../LogoLottie";
import { Ltr, SectionHeader } from "../ui";
import { Accordion, DataGrid } from "./Accordion";
import { VehicleLicenseButton } from "./VehicleLicense";

const GROUP_ICON: Record<GroupId, IconName> = {
  identity: "chassis",
  test: "calendar",
  engine: "engine",
  safety: "shield",
  comfort: "seat",
  pollution: "leaf",
  dimensions: "ruler",
  colorTires: "palette",
  approvals: "layers",
};

const DEFAULT_OPEN: GroupId[] = ["identity", "test", "engine"];

type SpecsSectionProps = {
  vehicle: VehicleRecord;
  printMode: boolean;
};

export function SpecsSection({ vehicle, printMode }: SpecsSectionProps) {
  const groups = useMemo(() => buildFieldGroups(vehicle), [vehicle]);
  const priceKeys = {
    manufacturer: clean(vehicle.manufacturerCode),
    model: clean(vehicle.modelCode),
    year: clean(vehicle.manufactureYear),
  };
  const hasPriceList = !!(priceKeys.manufacturer && priceKeys.model && priceKeys.year);

  const allIds = useMemo(() => {
    const ids = groups.map((g) => `group-${g.id}`);
    if (hasPriceList) ids.push("group-priceList");
    return ids;
  }, [groups, hasPriceList]);

  const [open, setOpen] = useState<Set<string>>(() => new Set(DEFAULT_OPEN.map((g) => `group-${g}`)));

  const isOpen = (id: string) => printMode || open.has(id);
  const toggle = (id: string) =>
    setOpen((current) => {
      const next = new Set(current);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  const allOpen = allIds.every((id) => open.has(id));

  return (
    <section id="specs" className="specs" aria-labelledby="specs-title">
      <SectionHeader
        icon="list"
        id="specs-title"
        title={he.accordion.sectionTitle}
        subtitle={he.accordion.sectionSubtitle}
        actions={
          <div className="specs__toggle no-print">
            <button
              type="button"
              className="btn btn--ghost btn--sm"
              onClick={() => setOpen(new Set(allIds))}
              disabled={allOpen}
            >
              {he.accordion.openAll}
            </button>
            <button
              type="button"
              className="btn btn--ghost btn--sm"
              onClick={() => setOpen(new Set())}
              disabled={open.size === 0}
            >
              {he.accordion.closeAll}
            </button>
          </div>
        }
      />

      <div className="accordion-stack">
        {groups.map((g) => (
          <GroupAccordion
            key={g.id}
            group={g}
            open={isOpen(`group-${g.id}`)}
            onToggle={toggle}
            extra={g.id === "identity" ? <VehicleLicenseButton vehicle={vehicle} /> : undefined}
          />
        ))}

        {hasPriceList && (
          <Accordion
            id="group-priceList"
            icon="tag"
            title={he.accordion.groups.priceList}
            open={isOpen("group-priceList")}
            onToggle={toggle}
          >
            {() => (
              <PriceListTable manufacturer={priceKeys.manufacturer!} model={priceKeys.model!} year={priceKeys.year!} />
            )}
          </Accordion>
        )}
      </div>
    </section>
  );
}

function GroupAccordion({
  group,
  open,
  onToggle,
  extra,
}: {
  group: FieldGroup;
  open: boolean;
  onToggle: (id: string) => void;
  extra?: ReactNode;
}) {
  return (
    <Accordion
      id={`group-${group.id}`}
      icon={GROUP_ICON[group.id]}
      title={he.accordion.groups[group.id]}
      count={groupCellCount(group)}
      open={open}
      onToggle={onToggle}
    >
      {() => (
        <>
          {extra && <div className="group-actions no-print">{extra}</div>}
          {group.safety?.level != null && (
            <LevelScale
              icon="shield"
              title={he.safety.scaleTitle}
              valueText={he.safety.scaleValue(group.safety.level, thresholds.safetyLevelMax)}
              value={group.safety.level}
              steps={thresholds.safetyLevelMax}
              higherIsBetter
              lowLabel={he.safety.scaleLow}
              highLabel={he.safety.scaleHigh}
            />
          )}
          {group.pollution?.group != null && (
            <LevelScale
              icon="leaf"
              title={he.pollution.scaleTitle}
              valueText={he.pollution.scaleValue(Math.round(group.pollution.group))}
              value={Math.round(group.pollution.group)}
              steps={15}
              lowLabel={he.pollution.scaleLow}
              highLabel={he.pollution.scaleHigh}
            />
          )}
          <DataGrid cells={group.cells} />
          {group.safety && <SafetyMatrixView matrix={group.safety.matrix} />}
        </>
      )}
    </Accordion>
  );
}

// ─── Level scales (safety, pollution) ─────────────────────────────────────

type LevelScaleProps = {
  icon: IconName;
  title: string;
  valueText: string;
  value: number;
  steps: number;
  /** Colors run red → green when true (safety), green → red otherwise (pollution). */
  higherIsBetter?: boolean;
  lowLabel: string;
  highLabel: string;
};

function LevelScale({ icon, title, valueText, value, steps, higherIsBetter, lowLabel, highLabel }: LevelScaleProps) {
  const hue = (n: number) => {
    const t = (n - 1) / (steps - 1);
    return Math.round((higherIsBetter ? t : 1 - t) * 140);
  };
  return (
    <div className="level-scale">
      <p className="level-scale__title">
        <Icon name={icon} size={18} />
        {title}: <strong>{valueText}</strong>
      </p>
      <ol className="level-scale__bar" aria-hidden="true" style={{ ["--steps" as string]: steps }}>
        {Array.from({ length: steps }, (_, i) => i + 1).map((n) => (
          <li
            key={n}
            className={`level-scale__step ${n === value ? "is-current" : ""}`}
            style={{ ["--h" as string]: hue(n) }}
          >
            {n === value ? <Ltr>{n}</Ltr> : null}
          </li>
        ))}
      </ol>
      <div className="level-scale__legend" aria-hidden="true">
        <span>{lowLabel}</span>
        <span>{highLabel}</span>
      </div>
    </div>
  );
}

// ─── Safety ───────────────────────────────────────────────────────────────

function SafetyMatrixView({ matrix }: { matrix: NonNullable<FieldGroup["safety"]>["matrix"] }) {
  if (matrix.present.length + matrix.absent.length + matrix.other.length === 0) return null;
  return (
    <div className="safety-matrix">
      <h4 className="subheading">{he.safety.matrixTitle}</h4>
      <div className="safety-matrix__cols">
        {matrix.present.length > 0 && (
          <div>
            <p className="safety-matrix__title safety-matrix__title--yes">
              <Icon name="checkCircle" size={18} />
              {he.safety.present} ({matrix.present.length})
            </p>
            <ul className="safety-matrix__list">
              {matrix.present.map((c) => (
                <li key={c.key} className="system system--yes">
                  <Icon name="check" size={16} label={he.common.yes} />
                  {c.label}
                </li>
              ))}
            </ul>
          </div>
        )}
        {matrix.absent.length > 0 && (
          <div>
            <p className="safety-matrix__title safety-matrix__title--no">
              <Icon name="xCircle" size={18} />
              {he.safety.absent} ({matrix.absent.length})
            </p>
            <ul className="safety-matrix__list">
              {matrix.absent.map((c) => (
                <li key={c.key} className="system system--no">
                  <Icon name="x" size={16} label={he.common.no} />
                  {c.label}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>
      {matrix.other.length > 0 && <DataGrid cells={matrix.other} />}
    </div>
  );
}

// ─── Price list (lazy) ────────────────────────────────────────────────────

type PriceState =
  | { status: "loading" }
  | { status: "empty" }
  | { status: "error" }
  | { status: "done"; rows: VehiclePriceListRecord[] };

function PriceListTable({ manufacturer, model, year }: { manufacturer: string; model: string; year: string }) {
  const [state, setState] = useState<PriceState>({ status: "loading" });

  useEffect(() => {
    const controller = new AbortController();
    setState({ status: "loading" });
    lookupPriceList(manufacturer, model, year, controller.signal)
      .then((rows) => setState(rows.length ? { status: "done", rows } : { status: "empty" }))
      .catch((err) => {
        if (controller.signal.aborted) return;
        setState(err instanceof ApiError && err.kind === "notFound" ? { status: "empty" } : { status: "error" });
      });
    return () => controller.abort();
  }, [manufacturer, model, year]);

  if (state.status === "loading") {
    return (
      <p className="inline-status" role="status" aria-live="polite">
        <LogoLottie size="inline" />
        {he.priceList.loading}
      </p>
    );
  }
  if (state.status === "empty") return <p className="info-note">{he.priceList.empty}</p>;
  if (state.status === "error") return <p className="info-note info-note--bad">{he.priceList.error}</p>;

  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead>
          <tr>
            <th scope="col">{he.priceList.importer}</th>
            <th scope="col">{he.priceList.commercialName}</th>
            <th scope="col">{he.priceList.modelType}</th>
            <th scope="col">{he.priceList.year}</th>
            <th scope="col" className="num">
              {he.priceList.price}
            </th>
          </tr>
        </thead>
        <tbody>
          {state.rows.map((row, i) => {
            const price = toNumber(row.price);
            return (
              <tr key={row.id ?? i}>
                <td>{clean(row.importerName) ?? "—"}</td>
                <td>{clean(row.commercialName) ?? clean(row.modelName) ?? "—"}</td>
                <td>{clean(row.modelType) ?? "—"}</td>
                <td>
                  <Ltr>{clean(row.manufactureYear) ?? "—"}</Ltr>
                </td>
                <td className="num">
                  {price != null ? formatCurrency(price) : <Ltr>{clean(row.price) ?? "—"}</Ltr>}
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
