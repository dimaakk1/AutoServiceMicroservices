import { AlertCircle, Loader2 } from "lucide-react";
import { Button } from "./ui/button";

export default function VehicleLoadState({ loading, error, onRetry }: {
  loading: boolean; error: string | null; onRetry: () => void;
}) {
  return (
    <div className="rounded-2xl border bg-card p-8 sm:p-12 text-center">
      {loading ? (
        <p role="status" className="flex items-center justify-center gap-2 text-muted-foreground">
          <Loader2 className="h-5 w-5 animate-spin" /> Завантаження автомобілів…
        </p>
      ) : (
        <>
          <AlertCircle className="mx-auto mb-3 h-8 w-8 text-destructive" />
          <p role="alert" className="mb-5">{error}</p>
          <Button variant="outline" onClick={onRetry}>Спробувати ще раз</Button>
        </>
      )}
    </div>
  );
}
