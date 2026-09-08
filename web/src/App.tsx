import { useState } from "react";
import { lookupVehicle, VehicleLookupError } from "./api/vehicles";
import { ComparisonShelf } from "./components/ComparisonShelf";
import { Header } from "./components/Header";
import { HeroSearch } from "./components/HeroSearch";
import { VehicleResultCard } from "./components/VehicleResultCard";
import { useComparison } from "./hooks/useComparison";
import { isLikelyPlate } from "./lib/plate";
import type { VehicleRecord } from "./types/vehicle";

export default function App() {
  const [plate, setPlate] = useState("");
  const [vehicle, setVehicle] = useState<VehicleRecord | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const comparison = useComparison();

  async function handleSearch() {
    if (!isLikelyPlate(plate)) {
      setError("יש להזין מספר רישוי תקין, בין 6 ל-8 ספרות.");
      setVehicle(null);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const result = await lookupVehicle(plate);
      setVehicle(result);
      requestAnimationFrame(() => {
        document.getElementById("result")?.scrollIntoView({ behavior: "smooth", block: "start" });
      });
    } catch (cause) {
      setVehicle(null);
      if (cause instanceof VehicleLookupError) {
        setError(cause.message);
        return;
      }
      setError("לא הצלחנו להתחבר לשרת. ודאו שה-API רץ ונסו שוב.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div id="top" className="app-shell relative">
      <Header comparisonCount={comparison.items.length} />
      <main>
        <HeroSearch
          plate={plate}
          onPlateChange={(value) => {
            setPlate(value);
            if (error) setError(null);
          }}
          onSubmit={handleSearch}
          loading={loading}
          error={error}
        />
        {vehicle && (
          <VehicleResultCard
            vehicle={vehicle}
            inComparison={comparison.has(vehicle)}
            comparisonFull={comparison.isFull}
            onAdd={() => comparison.add(vehicle)}
          />
        )}
        <ComparisonShelf
          items={comparison.items}
          onRemove={comparison.remove}
          onClear={comparison.clear}
        />
      </main>
      <footer className="border-t border-line/80 px-4 py-6 text-center text-sm text-slate-body">
        הנתונים מגיעים ממאגרי data.gov.il. אוטוסקופ הוא שם עבודה זמני למערכת.
      </footer>
    </div>
  );
}
