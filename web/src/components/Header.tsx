import { Logo } from "./Logo";

type HeaderProps = {
  comparisonCount: number;
};

export function Header({ comparisonCount }: HeaderProps) {
  return (
    <header className="sticky top-0 z-30 border-b border-line/80 bg-paper/80 backdrop-blur-md">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6">
        <Logo />
        <nav className="flex items-center gap-2 sm:gap-3">
          <a
            href="#compare"
            className="inline-flex items-center gap-2 rounded-full border border-line bg-card px-3 py-1.5 text-sm font-semibold text-ink-soft no-underline transition hover:border-brand/40 hover:text-brand"
          >
            <span>ההשוואה שלי</span>
            <span className="grid min-w-6 place-items-center rounded-full bg-ink px-1.5 text-[0.7rem] text-paper">
              {comparisonCount}
            </span>
          </a>
        </nav>
      </div>
    </header>
  );
}
