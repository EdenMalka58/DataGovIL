import { clean, vehicleTitle } from "../../lib/format";
import { formatPlate } from "../../lib/plate";
import { classifyOwnership, classifyVehicleKind, ownershipTone } from "../../lib/viewModel";
import { he } from "../../strings.he";
import type { VehicleRecord } from "../../types/vehicle";
import { ManufacturerLogo } from "../ManufacturerLogo";
import { Icon, type IconName } from "../Icon";
import { CopyButton, Ltr, Plate, useToast, copyText } from "../ui";
import { colorSwatch, ownershipIcon, vehicleKindIcon } from "./visuals";

type VehicleHeroProps = {
  vehicle: VehicleRecord;
  inComparison: boolean;
  onCompare: () => void;
  onPrint: () => void;
};

function fuelIcon(fuel: string): IconName {
  return /חשמל|היבריד|electric|hybrid/i.test(fuel) ? "bolt" : "fuel";
}

export function VehicleHero({ vehicle, inComparison, onCompare, onPrint }: VehicleHeroProps) {
  const toast = useToast();
  const plate = clean(vehicle.registrationNumber);
  const year = clean(vehicle.manufactureYear);
  const fuel = clean(vehicle.fuelType) ?? clean(vehicle.manufacturerModel?.fuelName);
  const driveTechnology = clean(vehicle.manufacturerModel?.driveTechnologyName);
  const color = clean(vehicle.color);
  const kind = classifyVehicleKind(vehicle);
  const kindLabel = he.vehicleKind[kind];
  const vehicleType = clean(vehicle.vehicleTypeName);
  const typeDetail = vehicleType && vehicleType !== kindLabel ? vehicleType : null;
  const euCategory = clean(vehicle.euVehicleTypeCode) ?? clean(vehicle.manufacturerModel?.euTypeApproval);
  const sourceLabel = vehicle.source ? he.source[vehicle.source] : null;
  const ownership = clean(vehicle.ownershipType);
  const ownershipKind = classifyOwnership(ownership);
  const swatch = color ? colorSwatch(color) : null;
  const subtitle = [clean(vehicle.commercialName) ? clean(vehicle.modelName) : null, clean(vehicle.modelType)]
    .filter(Boolean)
    .join(" · ");
  const brand = clean(vehicle.manufacturerName) ?? clean(vehicle.manufacturerModel?.manufacturerName);

  async function copyLink() {
    if (!plate) return;
    const url = new URL(window.location.href);
    url.search = "";
    url.hash = "";
    url.searchParams.set("plate", plate);
    const ok = await copyText(url.toString());
    toast(ok ? he.toast.linkCopied : he.toast.copyFailed);
  }

  return (
    <section className="vehicle-hero card card--elevated" aria-labelledby="vehicle-title">
      <div className="vehicle-hero__bg" aria-hidden="true" />
      <div className="vehicle-hero__main">
        <div className={`vehicle-kind vehicle-kind--${kind}`}>
          <span className="vehicle-kind__icon" aria-hidden="true">
            <Icon name={vehicleKindIcon(kind)} size={30} />
          </span>
          <div className="vehicle-kind__text">
            <p className="vehicle-kind__type">
              {kindLabel}
              {typeDetail && <span className="vehicle-kind__detail">{typeDetail}</span>}
              {euCategory && (
                <span className="vehicle-kind__eu" title={he.hero.euCategory}>
                  <Ltr>{euCategory}</Ltr>
                </span>
              )}
            </p>
            {sourceLabel && (
              <p className="vehicle-kind__source">
                <Icon name="database" size={14} />
                {he.hero.foundIn} {sourceLabel}
              </p>
            )}
          </div>
        </div>
        <h1 id="vehicle-title" className="vehicle-hero__title">
          {vehicleTitle(vehicle)}
          {year && (
            <span className="vehicle-hero__year">
              <Ltr>{year}</Ltr>
            </span>
          )}
        </h1>
        {subtitle && <p className="vehicle-hero__subtitle">{subtitle}</p>}

        <ul className="chips" aria-label={he.accordion.groups.identity}>
          {fuel && (
            <li className="chip">
              <Icon name={fuelIcon(fuel)} size={16} />
              {fuel}
            </li>
          )}
          {driveTechnology && driveTechnology !== fuel && (
            <li className="chip" title={he.compare.rows.driveTechnology}>
              <Icon name={fuelIcon(driveTechnology)} size={16} />
              {driveTechnology}
            </li>
          )}
          {color && (
            <li className="chip">
              {swatch ? (
                <span className="chip__swatch" style={{ background: swatch }} aria-hidden="true" />
              ) : (
                <Icon name="droplet" size={16} />
              )}
              {color}
            </li>
          )}
          {ownership && (
            <li className={`chip chip--owner-${ownershipTone(ownershipKind)}`}>
              <Icon name={ownershipIcon(ownershipKind)} size={16} />
              {ownership}
            </li>
          )}
        </ul>
      </div>

      <div className="vehicle-hero__side">
        <ManufacturerLogo
          logoSlug={vehicle.manufacturerModel?.logoSlug}
          name={brand}
          className="vehicle-hero__logo"
        />
        <div className="vehicle-hero__plate">
          <Plate number={plate} />
          {plate && (
            <CopyButton
              text={plate}
              label={he.hero.copyPlate}
              toastText={`${he.toast.copied}: ${formatPlate(plate)}`}
            />
          )}
        </div>
        <div className="vehicle-hero__actions no-print">
          <button type="button" className="btn btn--ghost" onClick={copyLink} disabled={!plate}>
            <Icon name="link" size={18} />
            {he.hero.copyLink}
          </button>
          <button type="button" className="btn btn--ghost" onClick={onPrint}>
            <Icon name="print" size={18} />
            {he.hero.print}
          </button>
          <button
            type="button"
            className={`btn ${inComparison ? "btn--soft" : "btn--primary"}`}
            onClick={onCompare}
          >
            <Icon name={inComparison ? "check" : "compare"} size={18} />
            {inComparison ? he.hero.inCompare : he.hero.compare}
          </button>
        </div>
      </div>
    </section>
  );
}
