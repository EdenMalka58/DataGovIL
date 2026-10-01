import type { FormEvent } from "react";
import { vehicleTitle } from "../lib/format";
import { formatPlate } from "../lib/plate";
import type { RecentSearch } from "../lib/storage";
import { he } from "../strings.he";
import type { VehicleRecord } from "../types/vehicle";
import { Icon, type IconName } from "./Icon";
import { LicensePlateField } from "./LicensePlateField";
import { LogoLottie } from "./LogoLottie";
import { Ltr, Plate } from "./ui";

type HeroSearchProps = {
  plate: string;
  onPlateChange: (value: string) => void;
  onSubmit: () => void;
  onPick: (plate: string) => void;
  loading: boolean;
  validationError: string | null;
  recent: RecentSearch[];
  onClearRecent: () => void;
  comparisonItems: VehicleRecord[];
  comparisonLimit: number;
  onOpenCompare: () => void;
};

const EXAMPLE_PLATE = "1234567";
const FEATURE_ICONS: IconName[] = ["shield", "warning", "currency"];

export function HeroSearch({
  plate,
  onPlateChange,
  onSubmit,
  onPick,
  loading,
  validationError,
  recent,
  onClearRecent,
  comparisonItems,
  comparisonLimit,
  onOpenCompare,
}: HeroSearchProps) {
  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    onSubmit();
  }

  return (
    <section className="hero-search" aria-labelledby="hero-title">
      <div className="hero-search__glow" aria-hidden="true" />
      <div className="container hero-search__inner">
        <p className="eyebrow">
          <span className="eyebrow__dot" aria-hidden="true" />
          {he.search.eyebrow}
        </p>
        <h1 id="hero-title" className="hero-search__title">
          {he.search.titleTop}
          <span className="hero-search__title-accent">{he.search.titleBottom}</span>
        </h1>
        <p className="hero-search__subtitle">{he.search.subtitle}</p>

        <form className="search-card" onSubmit={handleSubmit} noValidate role="search">
          <label htmlFor="plate" className="search-card__label">
            {he.search.label}
          </label>
          <LicensePlateField
            value={plate}
            onChange={onPlateChange}
            disabled={loading}
            errorId={validationError ? "plate-error" : undefined}
            invalid={!!validationError}
          />
          <p id="plate-error" className="field-error" role="alert">
            {validationError}
          </p>
          <button type="submit" className="btn btn--primary btn--lg btn--block" disabled={loading}>
            {loading ? (
              <>
                <LogoLottie size="inline" />
                {he.search.submitting}
              </>
            ) : (
              <>
                <Icon name="search" />
                {he.search.submit}
              </>
            )}
          </button>
          <p className="search-card__example">
            {he.search.example}{" "}
            <button type="button" className="link-btn" onClick={() => onPlateChange(EXAMPLE_PLATE)}>
              <Ltr>{formatPlate(EXAMPLE_PLATE)}</Ltr>
            </button>
            <span className="search-card__shortcut">{he.search.shortcutHint}</span>
          </p>
        </form>

        {comparisonItems.length > 0 ? (
          <div className="my-compare">
            <div className="my-compare__header">
              <h2 className="my-compare__title">
                <Icon name="compare" size={20} />
                {he.compare.myTitle}
                <span className="badge badge--primary">
                  {he.compare.myCount(comparisonItems.length, comparisonLimit)}
                </span>
              </h2>
            </div>
            <ul className="my-compare__list">
              {comparisonItems.map((v, i) => (
                <li key={String(v.registrationNumber ?? i)} className="my-compare__item">
                  <Plate number={v.registrationNumber} small />
                  <span className="my-compare__name">{vehicleTitle(v)}</span>
                </li>
              ))}
            </ul>
            {comparisonItems.length === 1 && <p className="my-compare__hint">{he.compare.myHintOne}</p>}
            <button type="button" className="btn btn--primary my-compare__open" onClick={onOpenCompare}>
              <Icon name="compare" size={18} />
              {he.compare.openCompare}
            </button>
          </div>
        ) : (
          <p className="my-compare-hint">
            <Icon name="compare" size={18} />
            {he.compare.myHintEmpty}
          </p>
        )}

        {recent.length > 0 && (
          <div className="recent">
            <div className="recent__header">
              <h2 className="recent__title">
                <Icon name="history" size={20} />
                {he.search.recentTitle}
                <span className="badge badge--primary">{recent.length}</span>
              </h2>
              <button
                type="button"
                className="link-btn recent__clear"
                onClick={onClearRecent}
                aria-label={he.search.recentClearAria}
              >
                <Icon name="trash" size={16} />
                {he.search.recentClear}
              </button>
            </div>
            <ul className="recent__list">
              {recent.map((item) => (
                <li key={item.plate}>
                  <button type="button" className="recent__item" onClick={() => onPick(item.plate)}>
                    <Plate number={item.plate} small />
                    <span className="recent__name">{item.title}</span>
                    <Icon name="chevronStart" size={16} className="recent__go flip-rtl" />
                  </button>
                </li>
              ))}
            </ul>
            <p className="recent__note">{he.search.recentNote}</p>
          </div>
        )}

        <ul className="features">
          {he.search.features.map((feature, i) => (
            <li key={feature.title} className="feature">
              <span className="feature__icon" aria-hidden="true">
                <Icon name={FEATURE_ICONS[i]} size={22} />
              </span>
              <span>
                <strong className="feature__title">{feature.title}</strong>
                <span className="feature__text">{feature.text}</span>
              </span>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
