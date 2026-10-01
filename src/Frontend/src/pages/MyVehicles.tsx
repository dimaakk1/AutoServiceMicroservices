import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { ArrowUpRight, Car, Gauge, Plus } from "lucide-react";
import { getVehicles, vehicleError } from "../api/vehicle";
import type { Vehicle } from "../api/vehicle";
import { Button } from "../components/ui/button";
import DeleteVehicleDialog from "../components/DeleteVehicleDialog";
import VehicleLoadState from "../components/VehicleLoadState";
import { PageHeader } from "../components/PageHeader";

export default function MyVehicles() {
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    getVehicles(controller.signal)
      .then(({ data }) => { if (!controller.signal.aborted) setVehicles(data); })
      .catch((err: unknown) => { if (!controller.signal.aborted) setError(vehicleError(err)); })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [attempt]);

  return (
    <div className="min-h-[70vh] bg-muted/20">
      <PageHeader
        eyebrow="Особистий гараж"
        title="Мої автомобілі"
        description="Зберігайте VIN, характеристики й актуальний пробіг та обирайте автомобіль під час запису."
      >
          <Button asChild variant="accent" className="shrink-0">
            <Link to="/my-vehicles/new"><Plus /> Додати автомобіль</Link>
          </Button>
      </PageHeader>
      <div className="container max-w-5xl py-10 md:py-14">

        {loading || error ? (
          <VehicleLoadState loading={loading} error={error} onRetry={() => { setLoading(true); setError(null); setAttempt((n) => n + 1); }} />
        ) : vehicles.length === 0 ? (
          <div className="border border-border bg-card px-6 py-16 text-center shadow-card">
            <div className="mx-auto mb-5 flex h-16 w-16 items-center justify-center border border-accent/30 bg-accent/10 text-accent"><Car className="h-8 w-8" /></div>
            <h2 className="mb-3 text-2xl font-semibold">Ваш гараж поки порожній</h2>
            <p className="mx-auto mb-7 max-w-md text-muted-foreground">Додайте перше авто: вкажіть VIN, марку, модель і рік. Пробіг та інші деталі можна доповнити пізніше.</p>
            <Button asChild variant="accent"><Link to="/my-vehicles/new"><Plus /> Додати перше авто</Link></Button>
          </div>
        ) : (
          <>
            <p className="mb-4 text-sm text-muted-foreground">Автомобілів у списку: {vehicles.length}</p>
            <div className="grid gap-5 md:grid-cols-2">
              {vehicles.map((vehicle) => (
                <article key={vehicle.id} className="flex min-w-0 flex-col border border-border bg-card shadow-card transition-colors hover:border-accent/40">
                  <div className="flex items-start gap-4 border-b p-6">
                    <div className="flex h-12 w-12 shrink-0 items-center justify-center border border-accent/30 bg-accent/10 text-accent"><Car className="h-6 w-6" /></div>
                    <div className="min-w-0">
                      <h2 className="break-words text-xl font-semibold"><Link className="hover:text-accent" to={`/my-vehicles/${vehicle.id}`}>{vehicle.make} {vehicle.model}</Link></h2>
                      <p className="mt-1 text-sm text-muted-foreground">{vehicle.year} рік{vehicle.color ? ` · ${vehicle.color}` : ""}</p>
                    </div>
                  </div>
                  <div className="flex-1 space-y-5 p-6">
                    <div><p className="mb-1 text-xs uppercase tracking-wider text-muted-foreground">VIN-код</p><p className="break-all font-mono text-sm tracking-wide">{vehicle.vin}</p></div>
                    <div className="flex flex-wrap items-center justify-between gap-3">
                      <span className="flex items-center gap-2 text-sm"><Gauge className="h-4 w-4 text-muted-foreground" />{vehicle.mileageKm === null ? "Пробіг не вказано" : `${vehicle.mileageKm.toLocaleString("uk-UA")} км`}</span>
                      {vehicle.licensePlate && <span className="rounded-md border bg-muted/40 px-3 py-1 text-sm font-semibold break-all">{vehicle.licensePlate}</span>}
                    </div>
                  </div>
                  <div className="flex flex-wrap gap-2 border-t p-4 sm:px-6">
                    <Button asChild variant="outline"><Link to={`/my-vehicles/${vehicle.id}`}>Детальніше <ArrowUpRight /></Link></Button>
                    <DeleteVehicleDialog vehicle={vehicle} onDeleted={(id) => setVehicles((items) => items.filter((item) => item.id !== id))} />
                  </div>
                </article>
              ))}
            </div>
          </>
        )}
      </div>
    </div>
  );
}
