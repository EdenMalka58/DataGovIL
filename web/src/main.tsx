import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "@fontsource/heebo/400.css";
import "@fontsource/heebo/500.css";
import "@fontsource/heebo/700.css";
import "@fontsource/heebo/800.css";
import "./styles/tokens.css";
import "./styles/base.css";
import "./styles/components.css";
import "./styles/animations.css";
import "./styles/print.css";
import App from "./App";

// Vite reloads the page once its dropped HMR socket reconnects (e.g. after the tab was in the
// background); a listener that never settles blocks that reload so the current view survives.
if (import.meta.hot) {
  import.meta.hot.on("vite:ws:disconnect", () => new Promise<never>(() => {}));
}

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
