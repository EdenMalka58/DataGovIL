import type { Theme } from "../lib/storage";
import { he } from "../strings.he";
import { Icon } from "./Icon";
import { Logo } from "./Logo";

type HeaderProps = {
  compact: boolean;
  onHome: () => void;
  theme: Theme;
  onToggleTheme: () => void;
  comparisonCount: number;
  comparisonActive: boolean;
  onOpenCompare: () => void;
};

export function Header({
  compact,
  onHome,
  theme,
  onToggleTheme,
  comparisonCount,
  comparisonActive,
  onOpenCompare,
}: HeaderProps) {
  const compareEmpty = comparisonCount === 0;

  return (
    <header className={`topbar ${compact ? "topbar--compact" : ""}`}>
      <div className="container topbar__inner">
        <Logo theme={theme} onClick={onHome} />

        <div className="topbar__actions">
          <button
            type="button"
            className={`pill-btn ${compareEmpty ? "is-empty" : ""} ${comparisonActive ? "is-active" : ""}`}
            onClick={onOpenCompare}
            aria-label={`${he.compare.openCompare} (${comparisonCount})`}
            aria-current={comparisonActive ? "page" : undefined}
            title={compareEmpty ? he.compare.headerEmptyHint : he.compare.openCompare}
          >
            <Icon name="compare" size={18} />
            <span className="pill-btn__label">{he.compare.headerCount}</span>
            <span className="pill-btn__label-short">{he.compare.headerShort}</span>
            <span className="pill-btn__count">{comparisonCount}</span>
          </button>
          <button
            type="button"
            className="icon-btn"
            onClick={onToggleTheme}
            aria-label={theme === "dark" ? he.theme.toLight : he.theme.toDark}
            title={theme === "dark" ? he.theme.toLight : he.theme.toDark}
          >
            <Icon name={theme === "dark" ? "sun" : "moon"} />
          </button>
        </div>
      </div>
    </header>
  );
}
