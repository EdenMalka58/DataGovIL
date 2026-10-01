import type { ReactNode } from "react";

/** One consistent sprite: 24×24, 1.75px rounded stroke. */
const ICONS = {
  car: (
    <>
      <path d="M5 17h14M3.5 13.5 5.3 8.1A2 2 0 0 1 7.2 6.7h9.6a2 2 0 0 1 1.9 1.4l1.8 5.4" />
      <rect x="2.5" y="12.5" width="19" height="5.5" rx="1.8" />
      <path d="M6 18v1.5M18 18v1.5" />
      <circle cx="7" cy="15.2" r=".9" />
      <circle cx="17" cy="15.2" r=".9" />
    </>
  ),
  plate: (
    <>
      <rect x="2.5" y="7" width="19" height="10" rx="2" />
      <path d="M6 7v10M9 12h9" />
    </>
  ),
  check: <path d="m5 12.5 4.5 4.5L19 7.5" />,
  checkCircle: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="m8 12.3 2.8 2.8L16.2 9.5" className="draw" />
    </>
  ),
  x: <path d="M6 6l12 12M18 6 6 18" />,
  xCircle: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="m9 9 6 6m0-6-6 6" className="draw" />
    </>
  ),
  ban: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="m5.6 5.6 12.8 12.8" className="draw" />
    </>
  ),
  pause: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M10 8.5v7M14 8.5v7" className="draw" />
    </>
  ),
  warning: (
    <>
      <path d="M10.3 4.1 2.8 17.2A2 2 0 0 0 4.5 20h15a2 2 0 0 0 1.7-2.8L13.7 4.1a2 2 0 0 0-3.4 0Z" />
      <path d="M12 9.5v4.2M12 16.8h.01" className="draw" />
    </>
  ),
  info: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 11v5.5M12 7.8h.01" />
    </>
  ),
  calendar: (
    <>
      <rect x="3.5" y="5" width="17" height="15.5" rx="2.2" />
      <path d="M3.5 10h17M8 3v4M16 3v4" />
      <path d="m9 15 2 2 4-4" className="draw" />
    </>
  ),
  engine: (
    <>
      <path d="M4 10h2V8h3V6h6v2h2.5l1.5 2h1.5v6H19l-1.5 2H9l-2-2H4Z" />
      <path d="M2 11v4M12 6V4M10 4h4" />
    </>
  ),
  fuel: (
    <>
      <path d="M4 20V5.5A1.5 1.5 0 0 1 5.5 4h7A1.5 1.5 0 0 1 14 5.5V20M3 20h12M4 10h10" />
      <path d="M14 8h1.5A1.5 1.5 0 0 1 17 9.5v6a1.5 1.5 0 0 0 3 0V8.5L17.5 6" />
    </>
  ),
  bolt: <path d="M13 2.5 4.5 13.5H12l-1 8 8.5-11H12Z" />,
  weight: (
    <>
      <path d="M6.5 8h11l2.5 12H4Z" />
      <circle cx="12" cy="5.5" r="2.5" />
    </>
  ),
  droplet: <path d="M12 3.2s6.5 7 6.5 11.3A6.5 6.5 0 0 1 5.5 14.5C5.5 10.2 12 3.2 12 3.2Z" />,
  tire: (
    <>
      <circle cx="12" cy="12" r="9" />
      <circle cx="12" cy="12" r="3.5" />
      <path d="M12 3v5.5M12 15.5V21M3 12h5.5M15.5 12H21" />
    </>
  ),
  chassis: (
    <>
      <rect x="3" y="6" width="18" height="12" rx="2" />
      <path d="M7 10h1M7 14h1M11 10h6M11 14h4" />
    </>
  ),
  user: (
    <>
      <circle cx="12" cy="8" r="4" />
      <path d="M4.5 20.5a7.5 7.5 0 0 1 15 0" />
    </>
  ),
  users: (
    <>
      <circle cx="9" cy="8" r="3.5" />
      <path d="M2.5 20a6.5 6.5 0 0 1 13 0M16 4.8a3.5 3.5 0 0 1 0 6.4M18 14.2a6.5 6.5 0 0 1 3.5 5.8" />
    </>
  ),
  wrench: (
    <path d="M14.7 6.3a4 4 0 0 0-5.4 5.1L3.5 17.2a1.8 1.8 0 0 0 2.6 2.6l5.8-5.8a4 4 0 0 0 5.1-5.4l-2.6 2.6-2.3-.3-.3-2.3Z" />
  ),
  shield: (
    <>
      <path d="M12 3 4.5 6v5.5c0 4.6 3.1 8.2 7.5 9.5 4.4-1.3 7.5-4.9 7.5-9.5V6Z" />
      <path d="m9 12 2.2 2.2L15.5 10" />
    </>
  ),
  leaf: (
    <>
      <path d="M5 19c0-8 5-13.5 15-14-.4 10-6 15-14 15" />
      <path d="M5 19c3-4 6-6.5 9.5-8.5" />
    </>
  ),
  recall: (
    <>
      <path d="M10.3 4.1 2.8 17.2A2 2 0 0 0 4.5 20h15a2 2 0 0 0 1.7-2.8L13.7 4.1a2 2 0 0 0-3.4 0Z" />
      <path d="M12 9v5M12 17h.01" />
    </>
  ),
  clock: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 7v5.2l3.3 2" />
    </>
  ),
  history: (
    <>
      <path d="M3.5 12a8.5 8.5 0 1 0 2.5-6" />
      <path d="M3.5 4v4h4M12 8v4.3l3 1.8" />
    </>
  ),
  structure: (
    <>
      <path d="M4 20V9l8-5 8 5v11" />
      <path d="M4 20h16M9 20v-6h6v6M8 11h.01M16 11h.01" />
    </>
  ),
  odometer: (
    <>
      <path d="M4.2 17.5a9 9 0 1 1 15.6 0" />
      <path d="m12 13.5 4-5M12 13.5h.01M7 12h.01M17.5 12h.01M12 7h.01" />
    </>
  ),
  currency: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M9 16V8h3a2.5 2.5 0 0 1 2.5 2.5V13M15 8v8h-3a2.5 2.5 0 0 1-2.5-2.5V12" />
    </>
  ),
  trendUp: (
    <>
      <path d="m3 17 6-6 4 4 8-8" />
      <path d="M15 7h6v6" />
    </>
  ),
  trendDown: (
    <>
      <path d="m3 7 6 6 4-4 8 8" />
      <path d="M15 17h6v-6" />
    </>
  ),
  arrowUp: <path d="M12 19V5M6 11l6-6 6 6" />,
  arrowDown: <path d="M12 5v14M6 13l6 6 6-6" />,
  flat: <path d="M5 12h14" />,
  document: (
    <>
      <path d="M14 3H7a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V8Z" />
      <path d="M14 3v5h5M9 13h6M9 17h4" />
    </>
  ),
  copy: (
    <>
      <rect x="8.5" y="8.5" width="12" height="12" rx="2.2" />
      <path d="M15.5 8.5V5.7a2.2 2.2 0 0 0-2.2-2.2H5.7a2.2 2.2 0 0 0-2.2 2.2v7.6a2.2 2.2 0 0 0 2.2 2.2h2.8" />
    </>
  ),
  link: (
    <>
      <path d="M10 14a4.5 4.5 0 0 0 6.4 0l3-3a4.5 4.5 0 0 0-6.4-6.4l-1 1" />
      <path d="M14 10a4.5 4.5 0 0 0-6.4 0l-3 3a4.5 4.5 0 0 0 6.4 6.4l1-1" />
    </>
  ),
  print: (
    <>
      <path d="M7 9V3.5h10V9M7 17.5H5a2 2 0 0 1-2-2V11a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2v4.5a2 2 0 0 1-2 2h-2" />
      <rect x="7" y="14" width="10" height="6.5" rx="1" />
    </>
  ),
  compare: (
    <>
      <rect x="3" y="4" width="7.5" height="16" rx="1.8" />
      <rect x="13.5" y="4" width="7.5" height="16" rx="1.8" />
      <path d="M6.7 9h.01M17.2 9h.01M5.5 13h2.5M16 13h2.5" />
    </>
  ),
  search: (
    <>
      <circle cx="11" cy="11" r="7" />
      <path d="m20.5 20.5-4.5-4.5" />
    </>
  ),
  chevron: <path d="m6 9 6 6 6-6" />,
  chevronStart: <path d="m9 6 6 6-6 6" />,
  close: <path d="M6 6l12 12M18 6 6 18" />,
  sun: (
    <>
      <circle cx="12" cy="12" r="4" />
      <path d="M12 2.5v2M12 19.5v2M2.5 12h2M19.5 12h2M5.3 5.3l1.4 1.4M17.3 17.3l1.4 1.4M5.3 18.7l1.4-1.4M17.3 6.7l1.4-1.4" />
    </>
  ),
  moon: <path d="M20 14.5A8.5 8.5 0 0 1 9.5 4a8.5 8.5 0 1 0 10.5 10.5Z" />,
  building: (
    <>
      <rect x="4" y="3" width="11" height="18" rx="1.5" />
      <path d="M15 9h4a1 1 0 0 1 1 1v11M8 7h3M8 11h3M8 15h3M3 21h18" />
    </>
  ),
  taxi: (
    <>
      <path d="M9.5 4.5h5l.8 2.2M8.7 6.7l.8-2.2" />
      <path d="M5 17h14M3.5 13.5 5.3 8.1A2 2 0 0 1 7.2 6.7h9.6a2 2 0 0 1 1.9 1.4l1.8 5.4" />
      <rect x="2.5" y="12.5" width="19" height="5.5" rx="1.8" />
    </>
  ),
  motorcycle: (
    <>
      <circle cx="5.5" cy="16.5" r="3" />
      <circle cx="18.5" cy="16.5" r="3" />
      <path d="M5.5 16.5 9 11h6M11 16.5h3.5L15 11M18.5 16.5 16 7h-2.5" />
    </>
  ),
  scooter: (
    <>
      <circle cx="6" cy="17.5" r="2.5" />
      <circle cx="18" cy="17.5" r="2.5" />
      <path d="M3.5 17.5V14a2 2 0 0 1 2-2h5l2 5.5h3.5M18 17.5 16.5 6H14M6 12v-1.5h4.5" />
    </>
  ),
  bus: (
    <>
      <rect x="3.5" y="3.5" width="17" height="14" rx="2.2" />
      <path d="M3.5 10h17M12 3.5V10M7 17.5V20M17 17.5V20" />
      <circle cx="7.5" cy="14" r=".9" />
      <circle cx="16.5" cy="14" r=".9" />
    </>
  ),
  van: (
    <>
      <path d="M2.5 16.5V7.5A1.5 1.5 0 0 1 4 6h11l4.5 5 2 .8v4.7Z" />
      <path d="M2.5 11h17M9 6v5" />
      <circle cx="6.5" cy="16.5" r="1.8" />
      <circle cx="17" cy="16.5" r="1.8" />
    </>
  ),
  pickup: (
    <>
      <path d="M2.5 16.5V12h9V7h5l3.5 5h1.5v4.5Z" />
      <path d="M11.5 12h8.5" />
      <circle cx="6.5" cy="16.5" r="1.8" />
      <circle cx="17" cy="16.5" r="1.8" />
    </>
  ),
  truck: (
    <>
      <rect x="2" y="5" width="12" height="11" rx="1" />
      <path d="M14 9h4l3 3.5V16h-7" />
      <circle cx="6" cy="17.5" r="2" />
      <circle cx="17.5" cy="17.5" r="2" />
    </>
  ),
  tractor: (
    <>
      <circle cx="7.5" cy="15.5" r="4.5" />
      <circle cx="7.5" cy="15.5" r="1.2" />
      <circle cx="18" cy="17" r="2.5" />
      <path d="M5 11V5h5l1.5 6H19a1.5 1.5 0 0 1 1.5 1.5V15M12 15.5h3.5M16 11V7.5" />
    </>
  ),
  trailer: (
    <>
      <rect x="3" y="6" width="15" height="9" rx="1" />
      <circle cx="10.5" cy="17.5" r="2" />
      <path d="M18 13h3.5M3 15v2" />
    </>
  ),
  database: (
    <>
      <ellipse cx="12" cy="5.5" rx="7.5" ry="2.5" />
      <path d="M4.5 5.5v13c0 1.4 3.4 2.5 7.5 2.5s7.5-1.1 7.5-2.5v-13M4.5 12c0 1.4 3.4 2.5 7.5 2.5s7.5-1.1 7.5-2.5" />
    </>
  ),
  key: (
    <>
      <circle cx="8" cy="15" r="4.5" />
      <path d="m11.2 11.8 8.3-8.3M16.5 6.5l2.5 2.5M14 9l2 2" />
    </>
  ),
  landmark: (
    <>
      <path d="M3 21h18M4 10h16M12 3l8.5 5H3.5Z" />
      <path d="M6 10v8M10 10v8M14 10v8M18 10v8" />
    </>
  ),
  home: (
    <>
      <path d="M3.5 11 12 4l8.5 7" />
      <path d="M5.5 9.5V20h13V9.5M10 20v-5.5h4V20" />
    </>
  ),
  sparkle: (
    <path d="M12 3.5 13.8 9l5.7 1.8-5.7 1.9L12 18.5l-1.8-5.8-5.7-1.9L10.2 9ZM19 3v3M17.5 4.5h3M5 17v3M3.5 18.5h3" />
  ),
  plus: <path d="M12 5v14M5 12h14" />,
  trash: (
    <>
      <path d="M4 7h16M10 11v6M14 11v6M9 7V4.5h6V7" />
      <path d="M6 7l1 13h10l1-13" />
    </>
  ),
  layers: (
    <>
      <path d="m12 3 9 5-9 5-9-5Z" />
      <path d="m3 13 9 5 9-5" />
    </>
  ),
  ruler: (
    <>
      <path d="M3.5 16.5 16.5 3.5l4 4-13 13Z" />
      <path d="m7.5 12.5 2 2M10.5 9.5l2 2M13.5 6.5l2 2" />
    </>
  ),
  seat: (
    <>
      <path d="M7 3.5h4l1 9H7.5Z" />
      <path d="M6 13h9.5a2 2 0 0 1 2 2v1.5H6.5a1.5 1.5 0 0 1-1.5-1.5v-.5A1 1 0 0 1 6 13ZM8 16.5V21M15.5 16.5V21" />
    </>
  ),
  list: (
    <>
      <path d="M9 6h11M9 12h11M9 18h11" />
      <path d="M4.5 6h.01M4.5 12h.01M4.5 18h.01" />
    </>
  ),
  tag: (
    <>
      <path d="M3.5 12.2V4.5a1 1 0 0 1 1-1h7.7l8.3 8.3a1.5 1.5 0 0 1 0 2.1l-6.2 6.2a1.5 1.5 0 0 1-2.1 0Z" />
      <circle cx="8" cy="8" r="1.4" />
    </>
  ),
  palette: (
    <>
      <path d="M12 3a9 9 0 1 0 0 18c1.2 0 1.8-.8 1.8-1.7 0-1.2-1-1.5-1-2.6 0-.9.7-1.7 1.7-1.7h2.2A4.3 4.3 0 0 0 21 10.7C21 6.4 17 3 12 3Z" />
      <circle cx="7.5" cy="11" r="1" />
      <circle cx="10" cy="7" r="1" />
      <circle cx="15" cy="7" r="1" />
    </>
  ),
  gauge: (
    <>
      <path d="M4.2 17.5a9 9 0 1 1 15.6 0" />
      <path d="m12 13.5 3.5-3.5" />
      <circle cx="12" cy="13.5" r="1.2" />
    </>
  ),
  star: <path d="m12 3.5 2.6 5.3 5.9.9-4.3 4.1 1 5.8L12 16.9l-5.2 2.7 1-5.8-4.3-4.1 5.9-.9Z" />,
  filter: <path d="M3.5 5h17l-6.5 8v6l-4 2v-8Z" />,
  help: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M9.5 9.3a2.6 2.6 0 0 1 5 .9c0 1.8-2.5 2.2-2.5 3.8M12 17h.01" />
    </>
  ),
  mapPin: (
    <>
      <path d="M12 21s7-6.2 7-11.5a7 7 0 0 0-14 0C5 14.8 12 21 12 21Z" />
      <circle cx="12" cy="9.5" r="2.5" />
    </>
  ),
  trophy: (
    <>
      <path d="M8 4h8v5a4 4 0 0 1-8 0Z" />
      <path d="M8 6H5a3 3 0 0 0 3 4M16 6h3a3 3 0 0 1-3 4M12 13v4M8.5 20.5h7M9.5 17h5v3.5h-5Z" />
    </>
  ),
  chart: (
    <>
      <path d="M4 20h16" />
      <path d="M7 20V12M12 20V5M17 20v-7" />
    </>
  ),
} satisfies Record<string, ReactNode>;

export type IconName = keyof typeof ICONS;

/** Rendered once at the app root. */
export function IconSprite() {
  return (
    <svg width="0" height="0" style={{ position: "absolute" }} aria-hidden="true" focusable="false">
      <defs>
        {Object.entries(ICONS).map(([name, body]) => (
          <symbol
            key={name}
            id={`i-${name}`}
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.75"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            {body}
          </symbol>
        ))}
      </defs>
    </svg>
  );
}

type IconProps = {
  name: IconName;
  size?: number;
  className?: string;
  /** When set the icon carries meaning and gets screen-reader text; otherwise it's decorative. */
  label?: string;
};

export function Icon({ name, size = 20, className, label }: IconProps) {
  return (
    <>
      <svg
        className={`icon ${className ?? ""}`}
        width={size}
        height={size}
        aria-hidden="true"
        focusable="false"
      >
        <use href={`#i-${name}`} />
      </svg>
      {label ? <span className="sr-only">{label}</span> : null}
    </>
  );
}

/** Inline (non-sprite) icon whose paths can be stroke-animated. */
export function DrawIcon({ name, size = 40, className }: { name: IconName; size?: number; className?: string }) {
  return (
    <svg
      className={`icon icon-draw ${className ?? ""}`}
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      {ICONS[name]}
    </svg>
  );
}
