export function Logo({ compact = false }: { compact?: boolean }) {
  return (
    <a href="#top" className="inline-flex items-center gap-2.5 no-underline text-ink">
      <span className="relative grid h-9 w-9 place-items-center rounded-xl bg-ink text-paper shadow-sm">
        <svg viewBox="0 0 32 32" className="h-5 w-5" aria-hidden="true">
          <circle cx="15" cy="15" r="7.5" fill="none" stroke="currentColor" strokeWidth="2" />
          <circle cx="15" cy="15" r="2.4" fill="currentColor" />
          <path d="M20.8 20.8 27 27" stroke="currentColor" strokeWidth="2.4" strokeLinecap="round" />
        </svg>
      </span>
      {!compact && (
        <span className="leading-tight">
          <span className="block text-[1.05rem] font-extrabold tracking-tight">אוטוסקופ</span>
          <span className="block text-[0.7rem] font-medium text-slate-body">שם זמני · בחירת רכב חכמה</span>
        </span>
      )}
    </a>
  );
}
