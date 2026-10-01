import type { ApiError } from "../api/client";
import { he } from "../strings.he";
import type { VehicleRecord } from "../types/vehicle";
import { Icon } from "./Icon";
import { LogoLottie } from "./LogoLottie";
import { Ltr, Plate } from "./ui";
import { formatDate } from "../lib/format";

export function LoadingSkeleton() {
  return (
    <div className="container result-layout" aria-busy="true">
      <div className="load-status">
        <LogoLottie />
        <p className="load-status__text" role="status" aria-live="polite">
          {he.states.loading}
        </p>
      </div>
      <div className="skeleton-card skeleton-hero" aria-hidden="true">
        <div className="sk sk--line sk--w40" />
        <div className="sk sk--title sk--w70" />
        <div className="sk sk--plate" />
        <div className="sk-row">
          <div className="sk sk--chip" />
          <div className="sk sk--chip" />
          <div className="sk sk--chip" />
        </div>
      </div>
      <div className="skeleton-card skeleton-banner sk" aria-hidden="true" />
      <div className="result-grid" aria-hidden="true">
        <div className="result-grid__aside">
          <div className="skeleton-card">
            <div className="sk sk--title sk--w50" />
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="sk sk--line" />
            ))}
          </div>
        </div>
        <div className="result-grid__main">
          <div className="skeleton-card">
            <div className="sk sk--title sk--w40" />
            <div className="sk sk--chart" />
          </div>
          <div className="skeleton-card">
            <div className="sk sk--title sk--w30" />
            <div className="sk-grid">
              {Array.from({ length: 6 }).map((_, i) => (
                <div key={i} className="sk sk--cell" />
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function NotFoundIllustration() {
  return (
    <svg className="state__art" viewBox="0 0 220 150" aria-hidden="true" focusable="false">
      <defs>
        <linearGradient id="nf-g" x1="0" x2="1" y1="0" y2="1">
          <stop offset="0" stopColor="var(--primary)" stopOpacity=".18" />
          <stop offset="1" stopColor="var(--primary)" stopOpacity=".04" />
        </linearGradient>
      </defs>
      <ellipse cx="110" cy="132" rx="84" ry="9" fill="var(--ink-3)" opacity=".12" />
      <rect x="30" y="38" width="140" height="62" rx="12" fill="url(#nf-g)" stroke="var(--border-strong)" strokeWidth="2" />
      <rect x="30" y="38" width="26" height="62" rx="10" fill="var(--il-blue)" opacity=".85" />
      <path d="M72 69h78" stroke="var(--ink-3)" strokeWidth="6" strokeLinecap="round" strokeDasharray="10 12" opacity=".5" />
      <circle cx="156" cy="92" r="26" fill="var(--surface)" stroke="var(--primary)" strokeWidth="5" />
      <path d="m175 111 18 18" stroke="var(--primary)" strokeWidth="7" strokeLinecap="round" />
      <path d="M148 86a8 8 0 1 1 11 7.4c-2 .8-3 2-3 4" stroke="var(--primary)" strokeWidth="4" strokeLinecap="round" fill="none" />
      <circle cx="156" cy="104" r="2.6" fill="var(--primary)" />
    </svg>
  );
}

export function NotFoundState({ plate, onReset }: { plate: string; onReset: () => void }) {
  return (
    <section className="container state" aria-labelledby="state-title">
      <div className="state__card">
        <NotFoundIllustration />
        <h1 id="state-title" className="state__title">
          {he.states.notFoundTitle("")}
          <Plate number={plate} small className="state__plate" />
        </h1>
        <p className="state__text">{he.states.notFoundText}</p>
        <ul className="state__tips">
          {he.states.notFoundTips.map((tip) => (
            <li key={tip}>
              <Icon name="check" size={18} />
              {tip}
            </li>
          ))}
        </ul>
        <button type="button" className="btn btn--primary" onClick={onReset}>
          <Icon name="search" />
          {he.states.tryAnother}
        </button>
      </div>
    </section>
  );
}

export function ErrorState({
  error,
  onRetry,
  onNewSearch,
}: {
  error: ApiError | null;
  onRetry: () => void;
  onNewSearch: () => void;
}) {
  const kind = error?.kind ?? "network";
  const title =
    kind === "upstream"
      ? he.states.upstreamTitle
      : kind === "network"
        ? he.states.networkTitle
        : he.states.genericTitle;
  const text =
    kind === "upstream" ? he.states.upstreamText : kind === "network" ? he.states.networkText : he.states.genericText;
  const detail = error?.problem?.detail ?? error?.problem?.title;

  return (
    <section className="container state" aria-labelledby="state-title">
      <div className="state__card state__card--error" role="alert">
        <span className="state__icon" aria-hidden="true">
          <Icon name={kind === "network" ? "link" : "warning"} size={40} />
        </span>
        <h1 id="state-title" className="state__title">
          {title}
        </h1>
        <p className="state__text">{text}</p>
        <div className="state__actions">
          <button type="button" className="btn btn--primary" onClick={onRetry}>
            <Icon name="history" />
            {he.states.retry}
          </button>
          <button type="button" className="btn btn--ghost" onClick={onNewSearch}>
            <Icon name="search" />
            {he.search.newSearch}
          </button>
        </div>
        {(detail || !!error?.status) && (
          <details className="state__details">
            <summary>{he.states.technicalDetails}</summary>
            <p dir="ltr">
              {error?.status ? `HTTP ${error.status}` : null}
              {detail ? ` — ${detail}` : null}
            </p>
          </details>
        )}
      </div>
    </section>
  );
}

export function PartialResult({ vehicle, onNewSearch }: { vehicle: VehicleRecord; onNewSearch: () => void }) {
  const updated = formatDate(vehicle.updatedDate);
  return (
    <section className="container state" aria-labelledby="state-title">
      <div className="state__card">
        <Plate number={vehicle.registrationNumber} />
        <h1 id="state-title" className="state__title">
          {he.states.partialTitle}
        </h1>
        <p className="state__text">
          <strong>{he.summary.flags.safetyDiscount}</strong>
        </p>
        <p className="state__text">{he.states.partialText}</p>
        {updated && (
          <p className="state__meta">
            <Icon name="calendar" size={18} />
            {he.labels.updatedDate}: <Ltr>{updated}</Ltr>
          </p>
        )}
        <button type="button" className="btn btn--primary" onClick={onNewSearch}>
          <Icon name="search" />
          {he.search.newSearch}
        </button>
      </div>
    </section>
  );
}
