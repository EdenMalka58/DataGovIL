import type { StatusItem, StatusKind, SummaryFlag } from "../../lib/viewModel";
import { he } from "../../strings.he";
import { DrawIcon, type IconName } from "../Icon";
import { ToneIcon } from "../ui";

const STATUS_ICON: Record<StatusKind, IconName> = {
  cancelled: "ban",
  inactive: "pause",
  testExpired: "xCircle",
  testSoon: "calendar",
  recall: "warning",
  ok: "checkCircle",
};

export function scrollToAnchor(id: string) {
  const el = document.getElementById(id);
  if (!el) return;
  el.scrollIntoView({ behavior: "smooth", block: "start" });
  window.setTimeout(() => {
    const focusTarget = el.matches("button, a, [tabindex]")
      ? el
      : (el.querySelector<HTMLElement>("h2, h3, button") ?? el);
    if (!focusTarget.hasAttribute("tabindex") && !focusTarget.matches("button, a")) {
      focusTarget.setAttribute("tabindex", "-1");
    }
    focusTarget.focus({ preventScroll: true });
  }, 450);
}

export function CancelledRibbon() {
  return (
    <div className="ribbon" role="note">
      <span>{he.status.ribbon}</span>
    </div>
  );
}

type StatusBannerProps = {
  statuses: StatusItem[];
  alerts: SummaryFlag[];
};

export function StatusBanner({ statuses, alerts }: StatusBannerProps) {
  const [main, ...rest] = statuses;
  const clickable = !!main.anchor;
  const restKinds = new Set(rest.map((r) => r.anchor));
  const extraAlerts = alerts.filter(
    (a) => a.anchor && a.anchor !== "status" && !restKinds.has(a.anchor) && a.anchor !== main.anchor,
  );

  const content = (
    <>
      <span className="status-banner__icon" aria-hidden="true">
        <span className="status-banner__ring" />
        <DrawIcon name={STATUS_ICON[main.kind]} size={40} />
      </span>
      <span className="status-banner__text">
        <strong className="status-banner__title">{main.title}</strong>
        <span className="status-banner__sub">{main.text}</span>
      </span>
    </>
  );

  return (
    <section id="status" className="status-wrap" aria-label={main.title}>
      {clickable ? (
        <button
          type="button"
          className={`status-banner status-banner--${main.tone} status-banner--clickable`}
          onClick={() => scrollToAnchor(main.anchor!)}
        >
          {content}
        </button>
      ) : (
        <div className={`status-banner status-banner--${main.tone}`} role="status">
          {content}
        </div>
      )}

      {(rest.length > 0 || extraAlerts.length > 0) && (
        <ul className="alerts" aria-label={he.status.alertsLabel}>
          {rest.map((item) => (
            <li key={item.kind}>
              <AlertChip tone={item.tone} text={item.title} anchor={item.anchor} />
            </li>
          ))}
          {extraAlerts.map((flag) => (
            <li key={flag.id}>
              <AlertChip tone={flag.tone} text={flag.text} anchor={flag.anchor} />
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

function AlertChip({ tone, text, anchor }: { tone: SummaryFlag["tone"]; text: string; anchor?: string }) {
  const body = (
    <>
      <ToneIcon tone={tone} size={16} />
      <span>{text}</span>
    </>
  );
  if (!anchor) return <span className={`alert-chip alert-chip--${tone}`}>{body}</span>;
  return (
    <button type="button" className={`alert-chip alert-chip--${tone}`} onClick={() => scrollToAnchor(anchor)}>
      {body}
    </button>
  );
}
