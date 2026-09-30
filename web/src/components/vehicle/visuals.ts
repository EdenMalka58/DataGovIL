import type { OwnershipKind } from "../../lib/viewModel";
import type { IconName } from "../Icon";

/** Basic Hebrew color-name → swatch map. Unknown colors fall back to a droplet icon. */
const COLOR_MAP: [RegExp, string][] = [
  [/לבן|שנהב|פנינה/, "#f4f4f2"],
  [/שחור/, "#16181d"],
  [/כסף|כסוף|מטאלי/, "#b8bec7"],
  [/אפור|גרפיט|עופרת|טיטניום/, "#6b7280"],
  [/אדום|בורדו|יין/, "#c62828"],
  [/כחול|תכלת|טורקיז/, "#1e5bb8"],
  [/ירוק|זית/, "#2e7d32"],
  [/צהוב/, "#f2c200"],
  [/כתום/, "#ef7d1a"],
  [/חום|שוקולד|קפה|ברונזה|נחושת/, "#7b4f2c"],
  [/בז'|בז|שמפניה|קרם|חול/, "#d8c3a0"],
  [/זהב/, "#c9a227"],
  [/סגול|לילך/, "#6b3fa0"],
  [/ורוד/, "#e57fa5"],
];

export function colorSwatch(name: string): string | null {
  for (const [pattern, hex] of COLOR_MAP) if (pattern.test(name)) return hex;
  return null;
}

export function ownershipIcon(kind: OwnershipKind | null): IconName {
  switch (kind) {
    case "private":
      return "user";
    case "commercial":
      return "building";
    case "rental":
      return "key";
    case "taxi":
      return "taxi";
    case "government":
    case "public":
      return "landmark";
    case "kibbutz":
      return "home";
    default:
      return "user";
  }
}
