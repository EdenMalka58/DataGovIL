import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
  type CSSProperties,
  type ReactNode,
} from "react";
import { formatPlate } from "../lib/plate";
import { he } from "../strings.he";
import { useInView } from "../hooks/useMotion";
import { Icon, type IconName } from "./Icon";

// ─── LTR isolation ────────────────────────────────────────────────────────

/** Numbers, plates, VINs and codes inside Hebrew text. */
export function Ltr({ children, className }: { children: ReactNode; className?: string }) {
  return (
    <bdi dir="ltr" className={className}>
      {children}
    </bdi>
  );
}

// ─── Toast ────────────────────────────────────────────────────────────────

export type ToastAction = { label: string; onClick: () => void };
type ToastState = { id: number; text: string; action?: ToastAction } | null;
const ToastContext = createContext<(text: string, action?: ToastAction) => void>(() => {});

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toast, setToast] = useState<ToastState>(null);
  const timer = useRef<number | undefined>(undefined);

  const show = useCallback((text: string, action?: ToastAction) => {
    window.clearTimeout(timer.current);
    setToast({ id: Date.now(), text, action });
    timer.current = window.setTimeout(() => setToast(null), action ? 5000 : 2600);
  }, []);

  return (
    <ToastContext.Provider value={show}>
      {children}
      <div className="toast-region" role="status" aria-live="polite">
        {toast && (
          <div key={toast.id} className={`toast ${toast.action ? "toast--action" : ""}`}>
            <Icon name="check" size={18} />
            <span>{toast.text}</span>
            {toast.action && (
              <button
                type="button"
                className="toast__action"
                onClick={() => {
                  window.clearTimeout(timer.current);
                  setToast(null);
                  toast.action!.onClick();
                }}
              >
                {toast.action.label}
              </button>
            )}
          </div>
        )}
      </div>
    </ToastContext.Provider>
  );
}

export const useToast = () => useContext(ToastContext);

export async function copyText(text: string): Promise<boolean> {
  try {
    await navigator.clipboard.writeText(text);
    return true;
  } catch {
    try {
      const area = document.createElement("textarea");
      area.value = text;
      area.setAttribute("readonly", "");
      area.style.position = "fixed";
      area.style.opacity = "0";
      document.body.appendChild(area);
      area.select();
      const ok = document.execCommand("copy");
      area.remove();
      return ok;
    } catch {
      return false;
    }
  }
}

export function CopyButton({
  text,
  label,
  className,
  toastText,
}: {
  text: string;
  label: string;
  className?: string;
  toastText?: string;
}) {
  const toast = useToast();
  const [done, setDone] = useState(false);
  return (
    <button
      type="button"
      className={`icon-btn ${className ?? ""}`}
      aria-label={label}
      title={label}
      onClick={async () => {
        const ok = await copyText(text);
        toast(ok ? (toastText ?? he.toast.copied) : he.toast.copyFailed);
        if (ok) {
          setDone(true);
          window.setTimeout(() => setDone(false), 1600);
        }
      }}
    >
      <Icon name={done ? "check" : "copy"} size={18} />
    </button>
  );
}

// ─── Modal ────────────────────────────────────────────────────────────────

export function Modal({
  open,
  onClose,
  title,
  children,
  wide,
  icon,
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  wide?: boolean;
  icon?: IconName;
}) {
  const ref = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open]);

  return (
    <dialog
      ref={ref}
      className={`modal ${wide ? "modal--wide" : ""}`}
      aria-labelledby="modal-title"
      onClose={onClose}
      onCancel={(e) => {
        e.preventDefault();
        onClose();
      }}
      onClick={(e) => {
        if (e.target === ref.current) onClose();
      }}
    >
      {open && (
        <div className="modal__panel">
          <header className="modal__header">
            <h2 id="modal-title" className="modal__title">
              {icon && <Icon name={icon} size={22} />}
              {title}
            </h2>
            <button type="button" className="icon-btn" onClick={onClose} aria-label={he.modal.close}>
              <Icon name="close" />
            </button>
          </header>
          <div className="modal__body">{children}</div>
        </div>
      )}
    </dialog>
  );
}

// ─── Scroll reveal ────────────────────────────────────────────────────────

export function Reveal({
  children,
  index = 0,
  as: Tag = "div",
  className,
  id,
  style,
}: {
  children: ReactNode;
  index?: number;
  as?: "div" | "section" | "article" | "aside" | "li";
  className?: string;
  id?: string;
  style?: CSSProperties;
}) {
  const [ref, inView] = useInView<HTMLElement>();
  return (
    <Tag
      ref={ref as never}
      id={id}
      className={`reveal ${inView ? "is-visible" : ""} ${className ?? ""}`}
      style={{ ...style, ["--reveal-delay" as string]: `${index * 60}ms` }}
    >
      {children}
    </Tag>
  );
}

// ─── License plate ────────────────────────────────────────────────────────

export function Plate({ number, small, className }: { number?: string | null; small?: boolean; className?: string }) {
  const formatted = number ? formatPlate(number) : "—";
  return (
    <span
      className={["vehicle-code", small && "sm", className].filter(Boolean).join(" ")}
      role="img"
      aria-label={he.hero.plateAria(formatted)}
    >
      {formatted}
    </span>
  );
}

// ─── Misc ─────────────────────────────────────────────────────────────────

export function SectionHeader({
  icon,
  title,
  subtitle,
  id,
  actions,
}: {
  icon: IconName;
  title: string;
  subtitle?: string;
  id?: string;
  actions?: ReactNode;
}) {
  return (
    <header className="section-header">
      <span className="section-header__icon" aria-hidden="true">
        <Icon name={icon} size={22} />
      </span>
      <div className="section-header__text">
        <h2 id={id} className="section-header__title">
          {title}
        </h2>
        {subtitle && <p className="section-header__subtitle">{subtitle}</p>}
      </div>
      {actions && <div className="section-header__actions">{actions}</div>}
    </header>
  );
}

export function ToneIcon({ tone, size = 20 }: { tone: "ok" | "warn" | "bad" | "info"; size?: number }) {
  const name: IconName =
    tone === "ok" ? "checkCircle" : tone === "warn" ? "warning" : tone === "bad" ? "xCircle" : "info";
  const label =
    tone === "ok"
      ? he.summary.levelOk
      : tone === "warn"
        ? he.summary.levelWarn
        : tone === "bad"
          ? he.summary.levelBad
          : he.summary.levelInfo;
  return <Icon name={name} size={size} label={label} className={`tone-icon tone-icon--${tone}`} />;
}
