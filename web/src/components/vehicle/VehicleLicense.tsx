import { useState } from "react";
import israelEmblemUrl from "../../assets/logo-license/logo-Israel.svg.webp";
import motLogoUrl from "../../assets/logo-license/logo-MOT.svg.webp";
import { clean, formatDateShort, needsLtr, parseGovDateDetailed, toBool } from "../../lib/format";
import { he } from "../../strings.he";
import type { VehicleRecord } from "../../types/vehicle";
import { Icon } from "../Icon";
import { Ltr, Modal } from "../ui";

const S = he.license;

/** MM/YYYY, as printed on the license for month-precision dates. */
function licenseMonth(raw: unknown): string | null {
  const parsed = parseGovDateDetailed(raw);
  if (!parsed) return clean(raw);
  return `${String(parsed.date.getMonth() + 1).padStart(2, "0")}/${parsed.date.getFullYear()}`;
}

/** DD/MM/YYYY, or MM/YYYY when the source only has a month. */
function licenseDate(raw: unknown): string | null {
  const parsed = parseGovDateDetailed(raw);
  if (!parsed) return clean(raw);
  return parsed.monthOnly ? licenseMonth(raw) : formatDateShort(parsed.date);
}

function yesNo(raw: unknown): string | null {
  const b = toBool(raw);
  if (b == null) return clean(raw);
  return b ? he.common.yes : he.common.no;
}

/** `sug_degem`: P = private passenger, M = commercial. */
function licenseType(vehicle: VehicleRecord): string | null {
  const modelType = (clean(vehicle.modelType) ?? clean(vehicle.manufacturerModel?.modelType))?.toUpperCase();
  if (modelType === "P") return S.typePrivate;
  if (modelType === "M") return S.typeCommercial;
  return clean(vehicle.vehicleTypeName) ?? clean(vehicle.vehicleTypeCode) ?? modelType ?? null;
}

function Value({ value }: { value: string | null }) {
  if (!value) return null;
  return needsLtr(value) ? <Ltr>{value}</Ltr> : <>{value}</>;
}

function Field({
  label,
  value,
  span,
  rowStart,
}: {
  label: string;
  value: string | null;
  span: 2 | 4 | 8;
  rowStart?: boolean;
}) {
  return (
    <div className={`vlicense__f vlicense__s${span} ${rowStart ? "vlicense__row-start" : ""}`}>
      <span className="vlicense__l">{label}</span>
      <span className="vlicense__v">
        <Value value={value} />
      </span>
    </div>
  );
}

export function VehicleLicenseCard({ vehicle }: { vehicle: VehicleRecord }) {
  const mm = vehicle.manufacturerModel;
  const manufacturerCode = clean(vehicle.manufacturerCode) ?? clean(mm?.manufacturerCode);
  const modelCode = clean(vehicle.modelCode) ?? clean(mm?.modelCode);

  const strip: { label: string; value: string | null }[] = [
    { label: S.horsepower, value: clean(mm?.horsepower) },
    { label: S.stability, value: yesNo(mm?.stabilityControlIndicator) },
    { label: S.pollution, value: clean(vehicle.pollutionGroup) ?? clean(mm?.pollutionGroup) },
    { label: S.airbags, value: clean(mm?.airbagCount) },
    { label: S.abs, value: yesNo(mm?.absIndicator) },
    { label: S.sunroof, value: yesNo(mm?.powerSunroofIndicator) },
    { label: S.registrationOrder, value: clean(vehicle.registrationOrder) },
    {
      label: S.code,
      value: manufacturerCode && modelCode ? `${manufacturerCode}-${modelCode}` : (manufacturerCode ?? modelCode),
    },
  ];

  return (
    <article className="vlicense" aria-label={S.title}>
      <header className="vlicense__top">
        <img className="vlicense__ministry" src={motLogoUrl} alt={S.ministryAlt} />
        <h3 className="vlicense__title">{S.title}</h3>
        <img className="vlicense__emblem" src={israelEmblemUrl} alt={S.emblemAlt} />
      </header>

      <section className="vlicense__grid">
        <div className="vlicense__band" aria-hidden="true">
          <div>{S.plate}</div>
          <div>{S.type}</div>
          <div>{S.validUntil}</div>
        </div>
        <dl className="vlicense__band-values">
          <div>
            <dt className="sr-only">{S.plate}</dt>
            <dd>
              <Value value={clean(vehicle.registrationNumber)} />
            </dd>
          </div>
          <div>
            <dt className="sr-only">{S.type}</dt>
            <dd>
              <Value value={licenseType(vehicle)} />
            </dd>
          </div>
          <div>
            <dt className="sr-only">{S.validUntil}</dt>
            <dd>
              <Value value={licenseDate(vehicle.testValidUntil)} />
            </dd>
          </div>
        </dl>

        <Field span={8} label={S.firstRegistration} value={licenseDate(vehicle.history?.technical?.firstRegistrationDate)} />

        <Field span={4} rowStart label={S.roadEntry} value={licenseMonth(vehicle.roadEntryDate)} />
        <Field span={4} label={S.chassis} value={clean(vehicle.chassisNumber)} />

        <Field span={2} rowStart label={S.fuel} value={clean(vehicle.fuelType) ?? clean(mm?.fuelName)} />
        <Field span={2} label={S.displacement} value={clean(mm?.engineDisplacement)} />
        <Field span={4} label={S.engineModel} value={clean(vehicle.engineModel)} />

        <Field span={4} rowStart label={S.frontTire} value={clean(vehicle.frontTire)} />
        <Field
          span={4}
          label={S.engineNumber}
          value={clean(vehicle.engineNumber) ?? clean(vehicle.history?.technical?.engineNumber)}
        />

        <Field span={4} rowStart label={S.rearTire} value={clean(vehicle.rearTire)} />
        <Field span={4} label={S.color} value={clean(vehicle.color)} />

        <Field span={2} rowStart label={S.totalWeight} value={clean(vehicle.totalWeight) ?? clean(mm?.totalWeight)} />
        <Field span={2} label={S.curbWeight} value={clean(mm?.curbWeight)} />
        <Field span={4} label={S.drive} value={clean(mm?.driveName) ?? clean(mm?.driveCode)} />

        <Field span={8} label={S.towHook} value={clean(vehicle.towHitch)} />
        <Field
          span={8}
          label={S.safetyLevel}
          value={clean(vehicle.safetyEquipmentLevel) ?? clean(mm?.safetyEquipmentLevel)}
        />

        <Field span={2} rowStart label={S.manufacturer} value={clean(vehicle.manufacturerName) ?? clean(mm?.manufacturerName)} />
        <Field span={2} label={S.model} value={clean(vehicle.modelName) ?? clean(mm?.modelName)} />
        <Field span={4} label={S.body} value={clean(mm?.bodyType)} />

        <Field span={2} rowStart label={S.commercialName} value={clean(vehicle.commercialName) ?? clean(mm?.commercialName)} />
        <Field span={2} label={S.trimLevel} value={clean(vehicle.trimLevel) ?? clean(mm?.trimLevel)} />
        <Field span={4} label={S.feeGroup} value={clean(mm?.feeGroupCode)} />

        <dl className="vlicense__strip">
          {strip.map((s) => (
            <div key={s.label}>
              <dt className="vlicense__l">{s.label}</dt>
              <dd className="vlicense__v">
                <Value value={s.value} />
              </dd>
            </div>
          ))}
        </dl>
      </section>

      <p className="vlicense__footnote">{S.footnote}</p>
    </article>
  );
}

export function VehicleLicenseButton({ vehicle }: { vehicle: VehicleRecord }) {
  const [open, setOpen] = useState(false);
  return (
    <>
      <button type="button" className="btn btn--soft btn--sm" onClick={() => setOpen(true)}>
        <Icon name="document" size={18} />
        {S.button}
      </button>
      <Modal open={open} onClose={() => setOpen(false)} title={S.modalTitle} icon="document" className="modal--license">
        <VehicleLicenseCard vehicle={vehicle} />
      </Modal>
    </>
  );
}
