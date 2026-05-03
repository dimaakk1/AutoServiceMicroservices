import { useState, useMemo, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../lib/auth-context";

import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Calendar } from "../components/ui/calendar";

import { createOrder, addOrderItem } from "../api/order";
import { getServices } from "../api/service";

import { Check, ChevronRight } from "lucide-react";
import { toast } from "sonner";
import { cn } from "../lib/utils";

const timeSlots = [
  "09:00", "10:00", "11:00",
  "12:00", "14:00", "15:00",
  "16:00", "17:00"
];

const steps = ["Послуга", "Дата і час", "Підтвердження"];

export default function Booking() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const preselectedId = searchParams.get("service");

  const [step, setStep] = useState(preselectedId ? 1 : 0);

  const [services, setServices] = useState<any[]>([]);
  const [loadingServices, setLoadingServices] = useState(true);

  const [selectedServiceId, setSelectedServiceId] = useState<string>(
    preselectedId || ""
  );

  const [selectedDate, setSelectedDate] = useState<Date | undefined>();
  const [selectedTime, setSelectedTime] = useState("");
  const [vehicleInfo, setVehicleInfo] = useState("");

  const [loading, setLoading] = useState(false);

  // ======================
  // LOAD SERVICES FROM API
  // ======================
  useEffect(() => {
    const load = async () => {
      try {
        const res = await getServices();
        setServices(res.data);
      } catch (err) {
        console.error("Services error:", err);
        toast.error("Не вдалося завантажити послуги");
      } finally {
        setLoadingServices(false);
      }
    };

    load();
  }, []);

  // ======================
  // SELECTED SERVICE
  // ======================
  const selectedService = useMemo(() => {
    return services.find(
      (s) => String(s.serviceId) === String(selectedServiceId)
    );
  }, [selectedServiceId, services]);

  // ======================
  // AUTH CHECK
  // ======================
  if (!user) {
    return (
      <div className="container py-20 text-center">
        <h2 className="text-2xl font-bold mb-4">
          Для запису потрібно увійти
        </h2>
        <Button onClick={() => navigate("/auth")}>
          Увійти
        </Button>
      </div>
    );
  }

  // ======================
  // CREATE ORDER
  // ======================
  const handleConfirm = async () => {
    if (!selectedService || !selectedDate || !selectedTime) return;

    if (loading) return;
    setLoading(true);

    try {
      const dateTime = new Date(
        selectedDate.toDateString() + " " + selectedTime
      ).toISOString();

      // 1. CREATE ORDER
      const orderRes = await createOrder({
        orderDate: dateTime,
        status: "Pending",
      });

      const orderId = orderRes.data?.orderId;

      if (!orderId) {
        toast.error("Order не створено");
        return;
      }

      // 2. ADD ITEM
      await addOrderItem({
        orderId,
        productId: selectedService.serviceId, // 🔥 ВАЖЛИВО
        quantity: 1,
      });

      toast.success("Запис створено!");
      navigate("/my-bookings");

    } catch (err: any) {
      console.error(err);

      toast.error(
        err.response?.data?.message || "Помилка створення запису"
      );
    } finally {
      setLoading(false);
    }
  };

  // ======================
  // LOADING SERVICES
  // ======================
  if (loadingServices) {
    return (
      <div className="container py-20 text-center">
        Завантаження послуг...
      </div>
    );
  }

  return (
    <div className="container py-12 max-w-3xl">
      <h1 className="text-3xl font-bold mb-8">
        Запис на сервіс
      </h1>

      {/* STEPPER */}
      <div className="flex items-center mb-10">
        {steps.map((s, i) => (
          <div key={s} className="flex items-center">
            <div
              className={cn(
                "w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold",
                i < step
                  ? "bg-green-500 text-white"
                  : i === step
                  ? "bg-accent text-accent-foreground"
                  : "bg-muted text-muted-foreground"
              )}
            >
              {i < step ? <Check className="h-4 w-4" /> : i + 1}
            </div>

            <span className="ml-2 text-sm hidden sm:inline">
              {s}
            </span>

            {i < steps.length - 1 && (
              <ChevronRight className="mx-3 h-4 w-4 text-muted-foreground" />
            )}
          </div>
        ))}
      </div>

      {/* STEP 1 */}
      {step === 0 && (
        <div>
          <h2 className="text-xl mb-4">Оберіть послугу</h2>

          <div className="grid sm:grid-cols-2 gap-3">
            {services.map((service) => (
              <button
                key={service.serviceId}
                onClick={() => {
                  setSelectedServiceId(service.serviceId);
                  setStep(1);
                }}
                className={cn(
                  "border rounded-lg p-4 text-left",
                  selectedServiceId == service.serviceId &&
                    "border-accent bg-accent/10"
                )}
              >
                <p className="font-medium">{service.name}</p>
                <p className="text-sm text-muted-foreground">
                  {service.price} грн
                </p>
              </button>
            ))}
          </div>
        </div>
      )}

      {/* STEP 2 */}
      {step === 1 && (
        <div>
          <h2 className="text-xl mb-4">Дата і час</h2>

          <Calendar
            mode="single"
            selected={selectedDate}
            onSelect={setSelectedDate}
          />

          <div className="grid grid-cols-3 gap-2 mt-4">
            {timeSlots.map((t) => (
              <button
                key={t}
                onClick={() => setSelectedTime(t)}
                className={cn(
                  "border rounded p-2",
                  selectedTime === t && "bg-accent text-white"
                )}
              >
                {t}
              </button>
            ))}
          </div>

          <Button
            className="mt-4"
            onClick={() => setStep(2)}
            disabled={!selectedDate || !selectedTime}
          >
            Далі
          </Button>
        </div>
      )}

      {/* STEP 3 */}
      {step === 2 && (
        <div>
          <h2 className="text-xl mb-4">Підтвердження</h2>

          <div className="space-y-2 mb-4">
            <p><b>Послуга:</b> {selectedService?.name}</p>
            <p><b>Дата:</b> {selectedDate?.toLocaleDateString()}</p>
            <p><b>Час:</b> {selectedTime}</p>
            <p><b>Клієнт:</b> {user.name}</p>
          </div>

          <Label>Авто</Label>
          <Input
            value={vehicleInfo}
            onChange={(e) => setVehicleInfo(e.target.value)}
          />

          <div className="flex gap-3 mt-4">
            <Button variant="outline" onClick={() => setStep(1)}>
              Назад
            </Button>

            <Button onClick={handleConfirm} disabled={loading}>
              {loading ? "Створення..." : "Підтвердити"}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}