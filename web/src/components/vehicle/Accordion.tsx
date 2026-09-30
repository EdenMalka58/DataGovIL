import { useEffect, useRef, useState, type MouseEvent, type ReactNode } from "react";
import type { DataCell } from "../../lib/fields";
import { he } from "../../strings.he";
import { Icon, type IconName } from "../Icon";
import { CopyButton, Ltr } from "../ui";

type AccordionProps = {
  id: string;
  icon: IconName;
  title: string;
  count?: number;
  open: boolean;
  onToggle: (id: string) => void;
  tone?: "default" | "alert";
  children: () => ReactNode;
};

/** Content renders on first open only (and stays mounted afterwards). */
export function Accordion({ id, icon, title, count, open, onToggle, tone = "default", children }: AccordionProps) {
  const [mounted, setMounted] = useState(open);
  const buttonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (open) setMounted(true);
  }, [open]);

  function ripple(e: MouseEvent<HTMLButtonElement>) {
    const btn = buttonRef.current;
    if (!btn) return;
    const rect = btn.getBoundingClientRect();
    const rtl = getComputedStyle(btn).direction === "rtl";
    btn.style.setProperty("--rx", `${rtl ? rect.right - e.clientX : e.clientX - rect.left}px`);
    btn.style.setProperty("--ry", `${e.clientY - rect.top}px`);
    btn.classList.remove("is-rippling");
    void btn.offsetWidth;
    btn.classList.add("is-rippling");
  }

  return (
    <section id={id} className={`accordion accordion--${tone} ${open ? "is-open" : ""}`}>
      <h3 className="accordion__heading">
        <button
          ref={buttonRef}
          type="button"
          className="accordion__trigger"
          aria-expanded={open}
          aria-controls={`${id}-panel`}
          onClick={(e) => {
            ripple(e);
            onToggle(id);
          }}
        >
          <span className="accordion__icon" aria-hidden="true">
            <Icon name={icon} size={20} />
          </span>
          <span className="accordion__title">{title}</span>
          {count != null && count > 0 && <span className="accordion__count">{he.accordion.count(count)}</span>}
          <span className="accordion__chevron" aria-hidden="true">
            <Icon name="chevron" size={20} />
          </span>
        </button>
      </h3>
      <div id={`${id}-panel`} className="accordion__panel" role="region" aria-label={title} hidden={!open && !mounted}>
        <div className="accordion__panel-inner">
          <div className="accordion__content">{mounted || open ? children() : null}</div>
        </div>
      </div>
    </section>
  );
}

// ─── Data grid ────────────────────────────────────────────────────────────

const LONG_VALUE = 90;

function CellValue({ cell }: { cell: DataCell }) {
  const [expanded, setExpanded] = useState(false);

  if (cell.bool != null) {
    return (
      <span className={`bool-chip ${cell.bool ? "bool-chip--yes" : "bool-chip--no"}`}>
        <Icon name={cell.bool ? "check" : "x"} size={14} />
        {cell.value}
      </span>
    );
  }

  const long = cell.value.length > LONG_VALUE;
  const text = long && !expanded ? `${cell.value.slice(0, LONG_VALUE).trimEnd()}…` : cell.value;
  const body = cell.ltr ? <Ltr className={cell.mono ? "mono" : undefined}>{text}</Ltr> : text;

  return (
    <>
      <span className="cell__text">{body}</span>
      {long && (
        <button type="button" className="link-btn cell__more" onClick={() => setExpanded((x) => !x)} aria-expanded={expanded}>
          {expanded ? he.accordion.less : he.accordion.more}
        </button>
      )}
    </>
  );
}

export function DataGrid({ cells }: { cells: DataCell[] }) {
  if (cells.length === 0) return null;
  return (
    <dl className="data-grid">
      {cells.map((cell) => (
        <div key={cell.key} className="cell">
          <dt className="cell__label">{cell.label}</dt>
          <dd className="cell__value">
            <CellValue cell={cell} />
            {cell.copyable && (
              <CopyButton text={cell.value.replace(/-/g, "")} label={he.accordion.copyAria(cell.label)} className="icon-btn--sm" />
            )}
            {cell.hint && <span className={`cell__hint cell__hint--${cell.hintTone ?? "muted"}`}>{cell.hint}</span>}
          </dd>
        </div>
      ))}
    </dl>
  );
}
