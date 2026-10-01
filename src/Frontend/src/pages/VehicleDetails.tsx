import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Car, Pencil } from "lucide-react";
import { getVehicle, vehicleError } from "../api/vehicle";
import type { Vehicle } from "../api/vehicle";
import { Button } from "../components/ui/button";
import VehicleLoadState from "../components/VehicleLoadState";
import DeleteVehicleDialog from "../components/DeleteVehicleDialog";
import { PageHeader } from "../components/PageHeader";

export default function VehicleDetails() {
  const { id = "" } = useParams();
  return <Details key={id} id={id} />;
}

function Details({ id }: { id: string }) {
  const navigate = useNavigate();
  const [vehicle, setVehicle] = useState<Vehicle | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    const controller = new AbortController();
    getVehicle(id, controller.signal)
      .then(({ data }) => { if (!controller.signal.aborted) setVehicle(data); })
      .catch((err: unknown) => { if (!controller.signal.aborted) setError(vehicleError(err)); })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [id, attempt]);

  return (
    <div className="min-h-[70vh] bg-muted/20">
      <PageHeader
        eyebrow="Особистий гараж"
        title={vehicle ? `${vehicle.make} ${vehicle.model}` : "Картка автомобіля"}
        description={vehicle ? `${vehicle.year} рік · збережені характеристики та актуальний пробіг` : "Детальна інформація про збережений автомобіль."}
      >
        <Button asChild variant="outline"><Link to="/my-vehicles"><ArrowLeft /> Мої автомобілі</Link></Button>
      </PageHeader>
      <div className="container max-w-4xl py-10 md:py-14">
      {loading || error ? <VehicleLoadState loading={loading} error={error} onRetry={() => { setLoading(true); setError(null); setAttempt((n) => n + 1); }} /> : vehicle && (
        <article className="overflow-hidden border border-border bg-card shadow-card">
          <div className="flex flex-col gap-5 bg-primary p-6 text-primary-foreground sm:flex-row sm:items-center sm:p-8">
            <Car className="h-12 w-12 shrink-0 text-accent" />
            <div className="min-w-0"><p className="technical-label mb-2 opacity-70">Мій автомобіль · {vehicle.year}</p><h2 className="break-words text-3xl">{vehicle.make} {vehicle.model}</h2></div>
          </div>
          <dl className="grid gap-6 p-6 sm:grid-cols-2 sm:p-8">
            {[
              ["VIN-код", vehicle.vin], ["Рік випуску", vehicle.year],
              ["Пробіг", vehicle.mileageKm === null ? "Не вказано" : `${vehicle.mileageKm.toLocaleString("uk-UA")} км`],
              ["Номерний знак", vehicle.licensePlate || "Не вказано"], ["Колір", vehicle.color || "Не вказано"],
              ["Додано", new Date(vehicle.createdAt).toLocaleDateString("uk-UA")],
            ].map(([label, value]) => <div key={label}><dt className="mb-1 text-sm text-muted-foreground">{label}</dt><dd className="break-words font-medium">{value}</dd></div>)}
          </dl>
          <p className="mx-6 mb-6 border border-border bg-muted/50 p-4 text-sm text-muted-foreground sm:mx-8">Характеристики та пробіг введені вами. Це картка збереженого авто, а не звіт про його історію.</p>
          <div className="flex flex-wrap gap-3 border-t p-6 sm:px-8">
            <Button asChild variant="accent"><Link to={`/my-vehicles/${id}/edit`}><Pencil /> Редагувати</Link></Button>
            <DeleteVehicleDialog vehicle={vehicle} onDeleted={() => navigate("/my-vehicles", { replace: true })} />
          </div>
        </article>
      )}
      </div>
    </div>
  );
}
