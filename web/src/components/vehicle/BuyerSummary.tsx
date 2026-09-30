import { formatCurrency, formatPercent } from "../../lib/format";
import type { SummaryFlag, Verdict } from "../../lib/viewModel";
import { he } from "../../strings.he";
import type { VehicleDepreciationRecord } from "../../types/vehicle";
import { Icon, type IconName } from "../Icon";
import { ToneIcon } from "../ui";
import { scrollToAnchor } from "./StatusBanner";

const VERDICT: Record<Verdict, { title: string; text: string; icon: IconName }> = {
  ok: { title: he.summary.verdictOk, text: he.summary.verdictOkText, icon: "checkCircle" },
  warn: { title: he.summary.verdictWarn, text: he.summary.verdictWarnText, icon: "warning" },
  bad: { title: he.summary.verdictBad, text: he.summary.verdictBadText, icon: "xCircle" },
};

type BuyerSummaryProps = {
  verdict: Verdict;
  flags: SummaryFlag[];
  depreciation?: VehicleDepreciationRecord | null;
  valueDisabled: boolean;
};

export function BuyerSummary({ verdict, flags, depreciation, valueDisabled }: BuyerSummaryProps) {
  const v = VERDICT[verdict];
  const estimated = depreciation?.estimatedValue;
  const percent = depreciation?.depreciationPercent;

  return (
    <section id="summary" className="card summary" aria-labelledby="summary-title">
      <header className="summary__header">
        <h2 id="summary-title" className="summary__title">
          <Icon name="sparkle" size={20} />
          {he.summary.title}
        </h2>
        <p className="summary__subtitle">{he.summary.subtitle}</p>
      </header>

      <div className={`verdict verdict--${verdict}`}>
        <span className="verdict__icon" aria-hidden="true">
          <Icon name={v.icon} size={30} />
        </span>
        <div>
          <p className="verdict__title">{v.title}</p>
          <p className="verdict__text">{v.text}</p>
        </div>
      </div>

      <ul className="flag-list">
        {flags.map((flag) => (
          <li key={flag.id} className={`flag flag--${flag.tone}`}>
            <ToneIcon tone={flag.tone} size={20} />
            {flag.anchor ? (
              <button type="button" className="flag__link" onClick={() => scrollToAnchor(flag.anchor!)}>
                {flag.text}
              </button>
            ) : (
              <span>{flag.text}</span>
            )}
          </li>
        ))}
      </ul>

      {depreciation && !valueDisabled && (
        <button type="button" className="mini-value" onClick={() => scrollToAnchor("value")}>
          <span className="mini-value__label">{he.summary.miniValueTitle}</span>
          <span className="mini-value__row">
            {estimated != null ? (
              <strong className="mini-value__amount">
                {formatCurrency(estimated)}
              </strong>
            ) : null}
            {percent != null && (
              <span className={`delta ${percent > 0 ? "delta--up" : percent < 0 ? "delta--down" : ""}`}>
                <Icon name={percent >= 0 ? "arrowUp" : "arrowDown"} size={14} />
                {formatPercent(percent)}
              </span>
            )}
          </span>
          <span className="mini-value__link">
            {he.summary.miniValueLink}
            <Icon name="chevronStart" size={16} className="flip-rtl" />
          </span>
        </button>
      )}

      <p className="disclaimer">
        <Icon name="info" size={16} />
        {he.summary.disclaimer}
      </p>
    </section>
  );
}
