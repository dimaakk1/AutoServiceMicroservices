import { useState } from "react";
import { Loader2, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { deleteVehicle, vehicleError } from "../api/vehicle";
import type { Vehicle } from "../api/vehicle";
import { Button } from "./ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle, DialogTrigger } from "./ui/dialog";

export default function DeleteVehicleDialog({ vehicle, onDeleted }: {
  vehicle: Vehicle; onDeleted: (id: string) => void;
}) {
  const [open, setOpen] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleDelete = async () => {
    if (deleting) return;
    setDeleting(true);
    setError(null);
    try {
      await deleteVehicle(vehicle.id);
      setOpen(false);
      toast.success("Автомобіль видалено зі списку");
      onDeleted(vehicle.id);
    } catch (err) {
      setError(vehicleError(err));
    } finally {
      setDeleting(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={(value) => { if (!deleting) { setOpen(value); setError(null); } }}>
      <DialogTrigger asChild>
        <Button variant="outline" className="text-destructive" aria-label={`Видалити ${vehicle.make} ${vehicle.model}`}>
          <Trash2 /> Видалити
        </Button>
      </DialogTrigger>
      <DialogContent className="max-w-[calc(100%-2rem)] sm:max-w-lg rounded-2xl">
        <DialogHeader>
          <DialogTitle>Видалити автомобіль?</DialogTitle>
          <DialogDescription>
            {vehicle.make} {vehicle.model}, {vehicle.year} — VIN {vehicle.vin}.
            Автомобіль буде видалено з вашого списку. Цю дію неможливо скасувати.
          </DialogDescription>
        </DialogHeader>
        {error && <p role="alert" className="text-sm text-destructive">{error}</p>}
        <DialogFooter className="gap-2">
          <Button variant="outline" disabled={deleting} onClick={() => setOpen(false)}>Залишити авто</Button>
          <Button variant="destructive" disabled={deleting} onClick={handleDelete}>
            {deleting && <Loader2 className="animate-spin" />} {deleting ? "Видалення…" : "Видалити автомобіль"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
