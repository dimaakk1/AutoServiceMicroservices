import { useState, useMemo, useEffect } from "react";
import axios from "axios";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../lib/auth-context";

import { Button } from "../components/ui/button";
import { Calendar } from "../components/ui/calendar";
import { Card, CardContent } from "../components/ui/card";
import { Badge } from "../components/ui/badge";

import {
  createOrder,
  addOrderItem,
  getTakenSlots,
} from "../api/order";

import { getServices } from "../api/service";
import { getVehicles, vehicleError } from "../api/vehicle";
import type { Vehicle } from "../api/vehicle";

import {
  Check,
  ChevronRight,
  CalendarDays,
  Clock3,
  Wrench,
  Car,
  Plus,
} from "lucide-react";

import { toast } from "sonner";
import { cn } from "../lib/utils";

/* ================= TYPES ================= */

type Service = {
  serviceId: number;
  name: string;
  price: number;
  categoryName: string;
};

const steps = ["Послуга", "Автомобіль і час", "Підтвердження"];

const timeSlots = [
  "09:00",
  "10:00",
  "11:00",
  "12:00",
  "14:00",
  "15:00",
  "16:00",
  "17:00",
];


export default function Booking() {
  const { user } = useAuth();

  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const preselected = searchParams.get("service");

  const [step, setStep] = useState(preselected ? 1 : 0);

  const [services, setServices] = useState<Service[]>([]);
  const [loading, setLoading] = useState(true);
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [vehiclesLoading, setVehiclesLoading] = useState(true);
  const [vehiclesError, setVehiclesError] = useState<string | null>(null);
  const [selectedVehicleId, setSelectedVehicleId] = useState("");

   // ✅ MULTI SELECT (замість одного)
  const [selectedServiceIds, setSelectedServiceIds] = useState<number[]>(
    preselected ? [Number(preselected)] : []
  );

  const [selectedDate, setSelectedDate] = useState<Date | undefined>();
  const [selectedTime, setSelectedTime] = useState("");

  const [takenSlots, setTakenSlots] = useState<string[]>([]);

  const [submitting, setSubmitting] = useState(false);


  useEffect(() => {
    getServices()
      .then((res) => setServices(res.data))
      .catch(() => toast.error("Помилка завантаження"))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    getVehicles(controller.signal)
      .then((res) => { if (!controller.signal.aborted) setVehicles(res.data); })
      .catch((error: unknown) => { if (!controller.signal.aborted) setVehiclesError(vehicleError(error)); })
      .finally(() => { if (!controller.signal.aborted) setVehiclesLoading(false); });
    return () => controller.abort();
  }, []);


useEffect(() => {
  if (!selectedDate) return;

  const year = selectedDate.getFullYear();
  const month = String(selectedDate.getMonth() + 1).padStart(2, "0");
  const day = String(selectedDate.getDate()).padStart(2, "0");

  const localDate = `${year}-${month}-${day}`;

  getTakenSlots(localDate)
    .then((res) => setTakenSlots(res.data))
    .catch(() => {
      toast.error("Не вдалося завантажити зайняті години");
    });

  setSelectedTime("");
}, [selectedDate]);


  // ✅ multiple services
  const selectedServices = useMemo(() => {
    return services.filter((s) => selectedServiceIds.includes(s.serviceId));
  }, [selectedServiceIds, services]);

  const selectedVehicle = useMemo(
    () => vehicles.find((vehicle) => vehicle.id === selectedVehicleId) ?? null,
    [selectedVehicleId, vehicles]
  );


  const isSlotTaken = (time: string) => {
    return takenSlots.includes(time);
  };


  if (!user) {
    return (
      <div className="container py-20 text-center">
        <h2 className="text-3xl font-bold mb-4">
          Увійдіть для запису
        </h2>

        <p className="text-muted-foreground mb-6">
          Авторизуйтесь щоб створити запис
        </p>

        <Button
          onClick={() => navigate("/auth")}
          className="bg-accent text-accent-foreground hover:bg-accent/90"
        >
          Увійти
        </Button>
      </div>
    );
  }


const handleConfirm = async () => {
    if (!selectedServices.length || !selectedDate || !selectedTime) return;

    setSubmitting(true);
  try {
    const [hours, minutes] = selectedTime.split(":").map(Number);

    const dateTime = new Date(Date.UTC(
      selectedDate.getFullYear(),
      selectedDate.getMonth(),
      selectedDate.getDate(),
      hours,
      minutes,
      0,
      0
    )).toISOString();

    const orderRes = await createOrder({
      orderDate: dateTime,
      status: "Pending",
      vehicleId: selectedVehicleId || null,
    });

    const orderId = orderRes.data?.orderId;

    for (const service of selectedServices) {
        await addOrderItem({
          orderId,
          productId: service.serviceId,
          quantity: 1,
        });
      }

    toast.success("Запис успішно створено");
    navigate("/my-bookings");

  } catch (error: unknown) {
    const responseData = axios.isAxiosError(error) ? error.response?.data : null;
    const message = responseData && typeof responseData === "object" && "message" in responseData
      && typeof responseData.message === "string"
      ? responseData.message
      : "Помилка створення запису";
    toast.error(message);
  } finally {
    setSubmitting(false);
  }
};


  if (loading) {
    return (
      <div className="container py-20 text-center">
        Завантаження...
      </div>
    );
  }


  return (
    <div className="min-h-screen bg-muted/30">
      <div className="container py-12 max-w-5xl">

        {/* HEADER */}
        <div className="mb-10">
          <Badge className="mb-4 border-0 bg-accent/10 text-accent">
            Онлайн запис
          </Badge>

          <h1 className="text-4xl font-bold mb-3">
            Запис на сервіс
          </h1>

          <p className="text-muted-foreground max-w-2xl">
            Оберіть послугу, дату та час для запису вашого автомобіля.
          </p>
        </div>

        <div className="flex items-center gap-3 mb-10 flex-wrap">
          {steps.map((s, i) => (
            <div key={s} className="flex items-center">

              <div
                className={cn(
                  "w-10 h-10 rounded-full flex items-center justify-center text-sm font-bold transition",
                  i < step
                    ? "bg-green-500 text-white"
                    : i === step
                    ? "bg-accent text-accent-foreground"
                    : "bg-muted text-muted-foreground"
                )}
              >
                {i < step ? <Check size={16} /> : i + 1}
              </div>

              <span className="ml-3 font-medium">
                {s}
              </span>

              {i < steps.length - 1 && (
                <ChevronRight className="mx-4 text-muted-foreground/40" />
              )}
            </div>
          ))}
        </div>

        {step === 0 && (
          <div>

            <div className="flex items-center justify-between mb-6">
              <div>
                <h2 className="text-2xl font-bold mb-1">
                  Оберіть послугу
                </h2>

                <p className="text-muted-foreground">
                  Доступні послуги сервісу
                </p>
              </div>

              <Badge variant="secondary">
                {services.length} послуг
              </Badge>
            </div>

            <div className="grid md:grid-cols-2 gap-5">
              {services.map((s) => {
                const selected = selectedServiceIds.includes(s.serviceId);

                return (
                  <Card
                    key={s.serviceId}
                    onClick={() => {
                      setSelectedServiceIds((prev) =>
                        prev.includes(s.serviceId)
                          ? prev.filter((id) => id !== s.serviceId)
                          : [...prev, s.serviceId]
                      );
                    }}
                    className={cn(
                      "cursor-pointer border transition-all hover:border-accent/40 hover:shadow-md",
                      selected && "border-accent bg-accent/10"
                    )}
                  >
                    <CardContent className="p-6">
                      <div className="flex items-start justify-between mb-4">
                        <div className="flex h-12 w-12 items-center justify-center border border-accent/30 bg-accent/10">
                          <Wrench className="h-6 w-6 text-accent" />
                        </div>

                        <Badge className="border-0 bg-accent text-accent-foreground">
                          {s.categoryName}
                        </Badge>
                      </div>

                      <h3 className="text-lg font-semibold mb-2">
                        {s.name}
                      </h3>

                      <div className="flex justify-between items-center mt-5">
                        <p className="text-xl font-bold">
                          {s.price} ₴
                        </p>

                        <span className="text-sm font-medium text-accent">
                          Обрати
                        </span>
                      </div>
                    </CardContent>
                  </Card>
                );
              })}
            </div>

            <div className="mt-6 flex justify-end">
              <Button
                onClick={() => setStep(1)}
                disabled={!selectedServiceIds.length}
                className="bg-accent text-accent-foreground hover:bg-accent/90"
              >
                Далі
              </Button>
            </div>

          </div>
        )}

        {step === 1 && (
          <div className="grid lg:grid-cols-[1fr_320px] gap-6">

            <div className="space-y-6">

              <Card className="border-accent/20 shadow-card">
                <CardContent className="p-6">
                  <div className="flex items-center gap-3 mb-5">
                    <div className="flex h-10 w-10 items-center justify-center border border-accent/30 bg-accent/10">
                      <Car className="h-5 w-5 text-accent" />
                    </div>
                    <div>
                      <h2 className="text-xl font-semibold">Оберіть автомобіль</h2>
                      <p className="text-sm text-muted-foreground">Необов’язково — запис можна створити без автомобіля</p>
                    </div>
                  </div>

                  {vehiclesLoading ? (
                    <p className="text-sm text-muted-foreground">Завантаження автомобілів…</p>
                  ) : vehiclesError ? (
                    <div className="rounded-xl border border-destructive/20 bg-destructive/5 p-4">
                      <p className="text-sm text-destructive">{vehiclesError}</p>
                      <p className="mt-2 text-xs text-muted-foreground">Ви все одно можете продовжити запис без автомобіля.</p>
                    </div>
                  ) : (
                    <div className="space-y-3">
                      <select
                        value={selectedVehicleId}
                        onChange={(event) => setSelectedVehicleId(event.target.value)}
                        className="h-12 w-full rounded-md border border-input bg-background px-3 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
                        aria-label="Автомобіль для запису"
                      >
                        <option value="">Без автомобіля</option>
                        {vehicles.map((vehicle) => (
                          <option key={vehicle.id} value={vehicle.id}>
                            {vehicle.make} {vehicle.model} ({vehicle.year}){vehicle.licensePlate ? ` · ${vehicle.licensePlate}` : ""}
                          </option>
                        ))}
                      </select>
                      {vehicles.length === 0 && <p className="text-sm text-muted-foreground">У вашому гаражі ще немає автомобілів.</p>}
                    </div>
                  )}

                  <Button type="button" variant="outline" className="mt-4" onClick={() => navigate("/my-vehicles/new")}>
                    <Plus className="h-4 w-4" /> Додати автомобіль
                  </Button>
                </CardContent>
              </Card>

              <Card className="border-accent/20 shadow-card">
                <CardContent className="p-6">

                  <div className="flex items-center gap-3 mb-5">
                    <div className="flex h-10 w-10 items-center justify-center border border-accent/30 bg-accent/10">
                      <CalendarDays className="h-5 w-5 text-accent" />
                    </div>

                    <div>
                      <h2 className="text-xl font-semibold">
                        Оберіть дату
                      </h2>

                      <p className="text-sm text-muted-foreground">
                        Доступні дні для запису
                      </p>
                    </div>
                  </div>

                  <Calendar
                    mode="single"
                    selected={selectedDate}
                    onSelect={setSelectedDate}
                    weekStartsOn={1}
                    disabled={(date) =>
                      date < new Date(new Date().setHours(0, 0, 0, 0))
                    }
                  />

                </CardContent>
              </Card>

              <Card className="border-accent/20 shadow-card">
                <CardContent className="p-6">

                  <div className="flex items-center gap-3 mb-5">
                    <div className="flex h-10 w-10 items-center justify-center border border-accent/30 bg-accent/10">
                      <Clock3 className="h-5 w-5 text-accent" />
                    </div>

                    <div>
                      <h2 className="text-xl font-semibold">
                        Оберіть час
                      </h2>

                      <p className="text-sm text-muted-foreground">
                        Вільні часові слоти
                      </p>
                    </div>
                  </div>

                  <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
                    {timeSlots.map((t) => {
                      const taken = isSlotTaken(t);

                      return (
                        <button
                          key={t}
                          disabled={taken}
                          onClick={() => setSelectedTime(t)}
                          className={cn(
                            "h-11 rounded-lg border transition font-medium",

                            taken
                              ? "bg-gray-100 text-gray-400 cursor-not-allowed border-gray-200"
                              : selectedTime === t
                              ? "border-accent bg-accent text-accent-foreground"
                              : "hover:border-accent/40 hover:bg-accent/10"
                          )}
                        >
                          {taken ? `${t} • Зайнято` : t}
                        </button>
                      );
                    })}
                  </div>

                </CardContent>
              </Card>

            </div>

            <div>
              <Card className="sticky top-24 border-accent/20 shadow-card">
                <CardContent className="p-6">

                  <h3 className="text-xl font-bold mb-5">
                    Ваш запис
                  </h3>

                  <div className="space-y-4">

                    <div>
                      <p className="text-sm text-muted-foreground mb-1">
                        Послуга
                      </p>

                      <p className="font-semibold">
                        {selectedServices.map((s) => s.name).join(", ")}
                      </p>
                    </div>

                    <div>
                      <p className="text-sm text-muted-foreground mb-1">
                        Категорія
                      </p>

                      <Badge className="border-0 bg-accent/10 text-accent">
                        {selectedServices.map((s) => s.categoryName).join(", ")}
                      </Badge>
                    </div>

                    <div>
                      <p className="text-sm text-muted-foreground mb-1">Автомобіль</p>
                      <p className="font-semibold">
                        {selectedVehicle ? `${selectedVehicle.make} ${selectedVehicle.model}${selectedVehicle.licensePlate ? ` · ${selectedVehicle.licensePlate}` : ""}` : "Без автомобіля"}
                      </p>
                    </div>

                    <div>
                      <p className="text-sm text-muted-foreground mb-1">
                        Дата
                      </p>

                      <p>
                        {selectedDate
                          ? selectedDate.toLocaleDateString("uk-UA")
                          : "Не обрано"}
                      </p>
                    </div>

                    <div>
                      <p className="text-sm text-muted-foreground mb-1">
                        Час
                      </p>

                      <p>
                        {selectedTime || "Не обрано"}
                      </p>
                    </div>

                    <div className="border-t pt-4 flex justify-between items-center">
                      <span className="text-muted-foreground">
                        Вартість
                      </span>

                      <span className="text-2xl font-bold">
                        {selectedServices.reduce((sum, s) => sum + s.price, 0)} ₴
                      </span>
                    </div>

                  </div>

                  <Button
                    className="mt-6 w-full bg-accent text-accent-foreground hover:bg-accent/90"
                    disabled={!selectedDate || !selectedTime}
                    onClick={() => setStep(2)}
                  >
                    Далі
                  </Button>

                </CardContent>
              </Card>
            </div>

          </div>
        )}

        {/* STEP 3 */}
        {step === 2 && (
          <Card className="border-accent/20 shadow-card">
            <CardContent className="p-8">

              <div className="flex items-center gap-4 mb-8">

                <div className="w-14 h-14 rounded-xl bg-green-100 flex items-center justify-center">
                  <Check className="h-7 w-7 text-green-600" />
                </div>

                <div>
                  <h2 className="text-3xl font-bold">
                    Підтвердження
                  </h2>

                  <p className="text-muted-foreground">
                    Перевірте інформацію перед створенням запису
                  </p>
                </div>

              </div>

              <div className="grid md:grid-cols-2 gap-5 mb-8">

                <div className="rounded-xl bg-muted/40 p-5">
                  <p className="text-sm text-muted-foreground mb-2">
                    Послуга
                  </p>

                  <p className="font-semibold">
                    {selectedServices.map((s) => s.name).join(", ")}
                  </p>
                </div>

                <div className="rounded-xl bg-muted/40 p-5">
                  <p className="text-sm text-muted-foreground mb-2">
                    Категорія
                  </p>

                  <p className="font-semibold">
                    {selectedServices.map((s) => s.categoryName).join(", ")}
                  </p>
                </div>

                <div className="rounded-xl bg-muted/40 p-5">
                  <p className="text-sm text-muted-foreground mb-2">Автомобіль</p>
                  <p className="font-semibold">
                    {selectedVehicle ? `${selectedVehicle.make} ${selectedVehicle.model}${selectedVehicle.licensePlate ? ` · ${selectedVehicle.licensePlate}` : ""}` : "Без автомобіля"}
                  </p>
                </div>

                <div className="rounded-xl bg-muted/40 p-5">
                  <p className="text-sm text-muted-foreground mb-2">
                    Дата
                  </p>

                  <p className="font-semibold">
                    {selectedDate?.toLocaleDateString("uk-UA")}
                  </p>
                </div>

                <div className="rounded-xl bg-muted/40 p-5">
                  <p className="text-sm text-muted-foreground mb-2">
                    Час
                  </p>

                  <p className="font-semibold">
                    {selectedTime}
                  </p>
                </div>

              </div>

              <div className="border-t pt-5 flex justify-between items-center mb-8">
                <span className="text-muted-foreground">
                  Загальна вартість
                </span>

                <span className="text-3xl font-bold">
                  {selectedServices.reduce((sum, s) => sum + s.price, 0)} ₴
                </span>
              </div>

              <div className="flex gap-3">

                <Button
                  variant="outline"
                  onClick={() => setStep(1)}
                >
                  Назад
                </Button>

                <Button
                  className="bg-accent text-accent-foreground hover:bg-accent/90"
                  onClick={handleConfirm}
                  disabled={submitting}
                >
                  {submitting
                    ? "Створення..."
                    : "Підтвердити"}
                </Button>

              </div>

            </CardContent>
          </Card>
        )}

      </div>
    </div>
  );
}
