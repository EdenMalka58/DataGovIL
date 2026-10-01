import { useEffect, useRef, useState } from "react";
import lottie from "lottie-web/build/player/lottie_light";
import animationData from "../assets/logo-package/logo-animation.json";
import stillUrl from "../assets/logo-package/logo-lottie.png";
import { prefersReducedMotion } from "../hooks/useMotion";

type LogoLottieProps = {
  size?: "page" | "inline";
};

/** Looping logo mark shown while a search or load is in progress. */
export function LogoLottie({ size = "page" }: LogoLottieProps) {
  const host = useRef<HTMLDivElement>(null);
  const [reduce] = useState(prefersReducedMotion);

  useEffect(() => {
    const node = host.current;
    if (!node || reduce) return;
    const anim = lottie.loadAnimation({
      container: node,
      renderer: "svg",
      loop: true,
      autoplay: true,
      animationData,
      rendererSettings: { preserveAspectRatio: "xMidYMid meet" },
    });
    return () => anim.destroy();
  }, [reduce]);

  return (
    <div className={`logo-lottie logo-lottie--${size}`} aria-hidden="true">
      {reduce ? (
        <img className="logo-lottie__still" src={stillUrl} alt="" />
      ) : (
        <div ref={host} className="logo-lottie__stage" />
      )}
    </div>
  );
}
