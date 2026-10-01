import { useCallback, useEffect, useRef, useState } from "react";
import { flushSync } from "react-dom";
import { ApiError } from "./api/client";
import { lookupVehicle } from "./api/vehicles";
import { CompareView } from "./components/CompareView";
import { Footer } from "./components/Footer";
import { Header } from "./components/Header";
import { HeroSearch } from "./components/HeroSearch";
import { IconSprite } from "./components/Icon";
import { ErrorState, LoadingSkeleton, NotFoundState, PartialResult } from "./components/States";
import { ToastProvider, useToast } from "./components/ui";
import { VehicleView } from "./components/vehicle/VehicleView";
import { useComparison } from "./hooks/useComparison";
import { formatDate, vehicleTitle } from "./lib/format";
import { isValidPlate, normalizePlate } from "./lib/plate";
import {
  clearRecent,
  pushRecent,
  readRecent,
  readTheme,
  recallResult,
  rememberResult,
  writeTheme,
  type RecentSearch,
  type Theme,
} from "./lib/storage";
import { isPartialRecord } from "./lib/viewModel";
import { he } from "./strings.he";
import type { VehicleRecord } from "./types/vehicle";

type ViewState =
  | { status: "idle" }
  | { status: "loading"; plate: string }
  | { status: "found"; plate: string; vehicle: VehicleRecord; cached: boolean }
  | { status: "notFound"; plate: string }
  | { status: "error"; plate: string; error: ApiError | null };

function systemTheme(): Theme {
  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

type Page = "main" | "compare";

function plateFromUrl(): string | null {
  const raw = new URLSearchParams(window.location.search).get("plate");
  if (!raw) return null;
  const digits = normalizePlate(raw);
  return isValidPlate(digits) ? digits : null;
}

function isCompareUrl(): boolean {
  return new URLSearchParams(window.location.search).get("view") === "compare";
}

/** URLs: `/` home, `?plate=…` vehicle, `?view=compare` comparison page. */
function setUrl(target: { plate?: string | null; compare?: boolean }, mode: "push" | "replace") {
  const url = new URL(window.location.href);
  url.hash = "";
  url.searchParams.delete("plate");
  url.searchParams.delete("view");
  if (target.plate) url.searchParams.set("plate", target.plate);
  if (target.compare) url.searchParams.set("view", "compare");
  if (url.toString() === window.location.href) return;
  const state = { plate: target.plate ?? null, compare: !!target.compare };
  if (mode === "push") window.history.pushState(state, "", url);
  else window.history.replaceState(state, "", url);
}

function AppContent() {
  const toast = useToast();
  const [input, setInput] = useState("");
  const [validationError, setValidationError] = useState<string | null>(null);
  const [view, setView] = useState<ViewState>({ status: "idle" });
  const [recent, setRecent] = useState<RecentSearch[]>(readRecent);
  const [theme, setTheme] = useState<Theme>(() => readTheme() ?? systemTheme());
  const [page, setPage] = useState<Page>(() => (isCompareUrl() ? "compare" : "main"));
  const [printMode, setPrintMode] = useState(false);
  const comparison = useComparison();
  const abortRef = useRef<AbortController | null>(null);
  const viewRef = useRef(view);
  viewRef.current = view;
  /** True when the comparison page was reached in-app, so "back" can pop history instead of pushing home. */
  const compareHasBackRef = useRef(false);

  // Theme: explicit choice wins; otherwise follow the OS.
  useEffect(() => {
    const stored = readTheme();
    if (stored) document.documentElement.dataset.theme = stored;
    else delete document.documentElement.dataset.theme;
    if (stored) return;
    const mq = window.matchMedia("(prefers-color-scheme: dark)");
    const onChange = () => setTheme(mq.matches ? "dark" : "light");
    mq.addEventListener("change", onChange);
    return () => mq.removeEventListener("change", onChange);
  }, []);

  const toggleTheme = useCallback(() => {
    setTheme((current) => {
      const next: Theme = current === "dark" ? "light" : "dark";
      writeTheme(next);
      document.documentElement.dataset.theme = next;
      return next;
    });
  }, []);

  const search = useCallback(
    async (rawPlate: string, urlMode: "push" | "replace" | "none" = "push") => {
      const plate = normalizePlate(rawPlate);
      if (!plate) {
        setValidationError(he.search.empty);
        return;
      }
      if (!isValidPlate(plate)) {
        setValidationError(he.search.invalid);
        return;
      }
      setValidationError(null);
      setInput(plate);
      abortRef.current?.abort();
      const controller = new AbortController();
      abortRef.current = controller;

      if (urlMode !== "none") setUrl({ plate }, urlMode);
      setPage("main");
      compareHasBackRef.current = false;
      setView({ status: "loading", plate });
      window.scrollTo({ top: 0, behavior: "smooth" });

      try {
        const vehicle = await lookupVehicle(plate, controller.signal);
        if (controller.signal.aborted) return;
        rememberResult(plate, vehicle);
        setRecent(pushRecent({ plate, title: vehicleTitle(vehicle), at: Date.now() }));
        setView({ status: "found", plate, vehicle, cached: false });
        setInput("");
      } catch (cause) {
        if (controller.signal.aborted) return;
        const error = cause instanceof ApiError ? cause : null;
        if (error?.kind === "notFound") {
          setView({ status: "notFound", plate });
          return;
        }
        const cached = recallResult(plate);
        if (cached && (error?.kind === "network" || error?.kind === "upstream")) {
          setView({ status: "found", plate, vehicle: cached, cached: true });
          setInput("");
          return;
        }
        setView({ status: "error", plate, error });
      }
    },
    [],
  );

  const goHome = useCallback(() => {
    abortRef.current?.abort();
    setPage("main");
    compareHasBackRef.current = false;
    setView({ status: "idle" });
    setInput("");
    setValidationError(null);
    setUrl({}, "push");
    window.scrollTo({ top: 0, behavior: "smooth" });
    window.setTimeout(() => document.getElementById("plate")?.focus(), 50);
  }, []);

  const openCompare = useCallback(() => {
    if (isCompareUrl()) return;
    setPage("compare");
    setValidationError(null);
    compareHasBackRef.current = true;
    setUrl({ compare: true }, "push");
    window.scrollTo({ top: 0 });
  }, []);

  const leaveCompare = useCallback(() => {
    if (compareHasBackRef.current) window.history.back();
    else goHome();
  }, [goHome]);

  // Deep links (?plate=, ?view=compare) on load, and back/forward navigation.
  useEffect(() => {
    const initial = plateFromUrl();
    if (initial && !isCompareUrl()) void search(initial, "replace");
    const onPop = () => {
      if (isCompareUrl()) {
        setPage("compare");
        compareHasBackRef.current = true;
        window.scrollTo({ top: 0 });
        return;
      }
      setPage("main");
      compareHasBackRef.current = false;
      const plate = plateFromUrl();
      const current = viewRef.current;
      if (plate) {
        if (current.status === "found" && current.plate === plate) return;
        void search(plate, "none");
      } else {
        abortRef.current?.abort();
        setView({ status: "idle" });
        setInput("");
      }
    };
    window.addEventListener("popstate", onPop);
    return () => window.removeEventListener("popstate", onPop);
  }, [search]);

  // "/" focuses the search field.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key !== "/" || e.ctrlKey || e.metaKey || e.altKey) return;
      const target = e.target as HTMLElement | null;
      if (target && (target.isContentEditable || /^(input|textarea|select)$/i.test(target.tagName))) return;
      if (document.querySelector("dialog[open]")) return;
      const field = document.querySelector<HTMLInputElement>("[data-search-input]");
      if (!field) return;
      e.preventDefault();
      field.focus();
      field.select();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, []);

  // Print: open every accordion synchronously so closed groups make it onto paper.
  useEffect(() => {
    const before = () => flushSync(() => setPrintMode(true));
    const after = () => setPrintMode(false);
    window.addEventListener("beforeprint", before);
    window.addEventListener("afterprint", after);
    return () => {
      window.removeEventListener("beforeprint", before);
      window.removeEventListener("afterprint", after);
    };
  }, []);

  const handlePrint = useCallback(() => {
    flushSync(() => setPrintMode(true));
    window.print();
  }, []);

  const handleCompare = useCallback(() => {
    if (view.status !== "found") return;
    const result = comparison.add(view.vehicle);
    const action = { label: he.toast.viewCompare, onClick: openCompare };
    if (result === "added") toast(he.toast.compareAdded(comparison.items.length + 1, comparison.limit), action);
    else if (result === "full") toast(he.toast.compareFull, action);
    else if (result === "duplicate") openCompare();
  }, [comparison, openCompare, toast, view]);

  useEffect(() => {
    const titleBase = `${he.app.name} — ${he.app.tagline}`;
    document.title =
      page === "compare"
        ? `${he.compare.title} · ${titleBase}`
        : view.status === "found"
          ? `${vehicleTitle(view.vehicle)} · ${titleBase}`
          : titleBase;
  }, [page, view]);

  const compact = page === "compare" || view.status !== "idle";
  const loading = view.status === "loading";
  const vehicle = view.status === "found" ? view.vehicle : null;

  return (
    <div className="app">
      <IconSprite />
      <a className="skip-link" href="#main">
        {he.app.skipToContent}
      </a>
      <Header
        compact={compact}
        onHome={goHome}
        theme={theme}
        onToggleTheme={toggleTheme}
        comparisonCount={comparison.items.length}
        comparisonActive={page === "compare"}
        onOpenCompare={openCompare}
        back={
          page === "compare"
            ? { label: he.compare.back, onClick: leaveCompare }
            : view.status !== "idle"
              ? { label: he.app.home, onClick: goHome }
              : null
        }
      />

      <main id="main" className="main" tabIndex={-1}>
        {page === "compare" && (
          <CompareView comparison={comparison} onOpenVehicle={(plate) => void search(plate)} />
        )}

        {page === "main" && view.status === "idle" && (
          <HeroSearch
            plate={input}
            onPlateChange={(value) => {
              setInput(value);
              if (validationError) setValidationError(null);
            }}
            onSubmit={() => void search(input)}
            onPick={(plate) => void search(plate)}
            loading={loading}
            validationError={validationError}
            recent={recent}
            onClearRecent={() => setRecent(clearRecent())}
            comparisonItems={comparison.items}
            comparisonLimit={comparison.limit}
            onOpenCompare={openCompare}
          />
        )}

        {page === "main" && view.status === "loading" && <LoadingSkeleton />}

        {page === "main" && view.status === "notFound" && <NotFoundState plate={view.plate} onReset={goHome} />}

        {page === "main" && view.status === "error" && (
          <ErrorState error={view.error} onRetry={() => void search(view.plate, "none")} onNewSearch={goHome} />
        )}

        {page === "main" &&
          view.status === "found" &&
          (isPartialRecord(view.vehicle) ? (
            <PartialResult vehicle={view.vehicle} onNewSearch={goHome} />
          ) : (
            <VehicleView
              key={view.plate}
              vehicle={view.vehicle}
              cached={view.cached}
              inComparison={comparison.has(view.plate)}
              onCompare={handleCompare}
              onPrint={handlePrint}
              onSearch={(plate) => void search(plate)}
              printMode={printMode}
            />
          ))}
      </main>

      <Footer updatedDate={page === "main" && vehicle ? formatDate(vehicle.updatedDate) : null} />
    </div>
  );
}

export default function App() {
  return (
    <ToastProvider>
      <AppContent />
    </ToastProvider>
  );
}
