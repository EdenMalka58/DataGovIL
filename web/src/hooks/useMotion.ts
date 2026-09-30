import { useEffect, useRef, useState, type RefObject } from "react";

export function prefersReducedMotion(): boolean {
  return typeof window !== "undefined" && window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}

/** Becomes true once the element scrolls into view (and stays true). */
export function useInView<T extends Element>(options?: IntersectionObserverInit): [RefObject<T | null>, boolean] {
  const ref = useRef<T | null>(null);
  const [inView, setInView] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el || inView) return;
    if (typeof IntersectionObserver === "undefined") {
      setInView(true);
      return;
    }
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries.some((e) => e.isIntersecting)) {
          setInView(true);
          observer.disconnect();
        }
      },
      // Edge-based (threshold 0): a ratio threshold never fires for elements taller than the viewport.
      { rootMargin: "0px 0px -12% 0px", threshold: 0, ...options },
    );
    observer.observe(el);
    return () => observer.disconnect();
  }, [inView, options]);

  return [ref, inView];
}

/** Short ease-out count-up from 0 to `target` once `start` is true. */
export function useCountUp(target: number | null | undefined, start: boolean, duration = 800): number | null {
  const [value, setValue] = useState<number | null>(() =>
    target == null ? null : prefersReducedMotion() ? target : 0,
  );

  useEffect(() => {
    if (target == null) return;
    const showFinal = () => setValue(target);
    window.addEventListener("beforeprint", showFinal);
    return () => window.removeEventListener("beforeprint", showFinal);
  }, [target]);

  useEffect(() => {
    if (target == null) {
      setValue(null);
      return;
    }
    if (!start) return;
    if (prefersReducedMotion()) {
      setValue(target);
      return;
    }
    let frame = 0;
    const began = performance.now();
    const tick = (now: number) => {
      const p = Math.min(1, (now - began) / duration);
      const eased = 1 - Math.pow(1 - p, 3);
      setValue(target * eased);
      if (p < 1) frame = requestAnimationFrame(tick);
    };
    frame = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(frame);
  }, [target, start, duration]);

  return value;
}
