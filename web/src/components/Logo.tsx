import logoDarkUrl from "../assets/logo-header-dark.png";
import logoUrl from "../assets/logo-header.png";
import type { Theme } from "../lib/storage";
import { he } from "../strings.he";

export function Logo({ theme, onClick }: { theme: Theme; onClick?: () => void }) {
  const dark = theme === "dark";
  return (
    <a
      href="./"
      className="logo"
      aria-label={he.app.home}
      title={he.app.home}
      onClick={(e) => {
        if (!onClick) return;
        e.preventDefault();
        onClick();
      }}
    >
      <img
        className="logo__img"
        src={dark ? logoDarkUrl : logoUrl}
        alt={he.app.name}
        width={dark ? 339 : 323}
        height={132}
      />
      <span className="logo__tagline">{he.app.tagline}</span>
    </a>
  );
}
