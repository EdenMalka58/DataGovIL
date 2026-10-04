import { useEffect, useState } from "react";
import { he } from "../strings.he";

const MOTOMARKS_KEY = import.meta.env.MOTOMARKS_KEY ?? "";

/** Motomarks CDN URL, or null when the maker has no slug or the publishable key is unset. */
export function manufacturerLogoSrc(logoSlug: string | null | undefined): string | null {
  const slug = logoSlug?.trim();
  if (!slug || !MOTOMARKS_KEY) return null;
  return `https://motomarks.io/img/${encodeURIComponent(slug)}?token=${encodeURIComponent(MOTOMARKS_KEY)}`;
}

type ManufacturerLogoProps = {
  logoSlug?: string | null;
  name?: string | null;
  className?: string;
};

export function ManufacturerLogo({ logoSlug, name, className }: ManufacturerLogoProps) {
  const src = manufacturerLogoSrc(logoSlug);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    setFailed(false);
  }, [src]);

  if (!src || failed) return null;

  const label = name?.trim();
  return (
    <img
      className={className ? `brand-logo ${className}` : "brand-logo"}
      src={src}
      alt={label ? he.hero.logoAlt(label) : ""}
      onError={() => setFailed(true)}
    />
  );
}
