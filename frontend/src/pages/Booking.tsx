import { useState, useMemo, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../lib/auth-context";
import { createOrder, createOrderItem } from "../api/order";
import  api  from "../api/api";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Calendar } from "../components/ui/calendar";
import { Check, ChevronRight } from "lucide-react";
import { toast } from "sonner";
import { cn } from "../lib/utils";
const steps = ["Послуга", "Дата і час", "Підтвердження"];

export default function Booking() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const preselectedId = searchParams.get("service");

  const [services, setServices] = useState<any[]>([]);
  const [step, setStep] = useState(preselectedId ? 1 : 0);

  const [selectedServiceId, setSelectedServiceId] = useState<string>(
    preselectedId || ""
  );

  const [selectedDate, setSelectedDate] = useState<Date | undefined>();
  const [selectedTime, setSelectedTime] = useState("");
  const [vehicleInfo, setVehicleInfo] = useState("");

  // 🔥 LOAD SERVICES FROM BACKEND
  useEffect(() => {
    api
      .get("/Catalog/Service")
      .then((res) => setServices(res.data))
      .catch((err) => {
        console.error(err);
        toast.error("Не вдалося завантажити послуги");
      });
  }, []);

  // 🔥 FIND SELECTED SERVICE
  const selectedService = useMemo(() => {
    return services.find(
      (s) => s.serviceId === Number(selectedServiceId)
    );
  }, [services, selectedServiceId]);

  if (!user) {
    return (
      <div className="container py-20 text-center">
        <h2 className="text-2xl font-bold mb-4">Потрібно увійти</h2>
        <Button onClick={() => navigate("/auth")}>Увійти</Button>
      </div>
    );
  }

  // 🔥 CREATE ORDER + ORDER ITEM
  const handleConfirm = async () => {
    if (!selectedService || !selectedDate || !selectedTime) {
      toast.error("Заповніть всі поля");
      return;
    }

    try {
      // 1. Order
      const orderRes = await createOrder({
        orderId: 0,
        orderDate: new Date().toISOString(),
        status: "Pending",
      });

      const orderId =
        orderRes.data.orderId ||
        orderRes.data.id ||
        orderRes.data;

      // 2. OrderItem
      await createOrderItem({
        orderItemId: 0,
        orderId: orderId,
        productId: selectedService.serviceId,
        quantity: 1,
      });

      toast.success("Запис створено!");
      navigate("/");
    } catch (err: any) {
      console.error(err);
      toast.error(
        err.response?.data?.message || "Помилка при створенні запису"
      );
    }
  };

  return (
    <div className="container py-12 max-w-3xl">
      <h1 className="text-3xl font-bold mb-8">Запис на сервіс</h1>

      {/* STEP INDICATOR */}
      <div className="flex items-center mb-10">
        {steps.map((s, i) => (
          <div key={s} className="flex items-center">
            <div
              className={cn(
                "w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold",
                i < step
                  ? "bg-green-500 text-white"
                  : i === step
                  ? "bg-blue-500 text-white"
                  : "bg-gray-300"
              )}
            >
              {i < step ? <Check className="h-4 w-4" /> : i + 1}
            </div>

            <span className="ml-2 text-sm hidden sm:inline">{s}</span>

            {i < steps.length - 1 && (
              <ChevronRight className="mx-3 h-4 w-4" />
            )}
          </div>
        ))}
      </div>

      {/* STEP 0 - SERVICES */}
      {step === 0 && (
        <div>
          <h2 className="text-xl mb-4">Оберіть послугу</h2>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
            {services.map((service) => (
              <button
                key={service.serviceId}
                onClick={() => {
                  setSelectedServiceId(service.serviceId);
                  setStep(1);
                }}
                className="border p-4 rounded hover:shadow"
              >
                <p className="font-medium">{service.name}</p>
                <p className="text-sm text-gray-500">
                  {service.price} грн
                </p>
              </button>
            ))}
          </div>
        </div>
      )}

      {/* STEP 1 - DATE */}
      {step === 1 && (
        <div>
          <h2 className="text-xl mb-4">
            Дата і час ({selectedService?.name})
          </h2>

          <Calendar
            mode="single"
            selected={selectedDate}
            onSelect={setSelectedDate}
          />

          <div className="grid grid-cols-3 gap-2 mt-4">
            {["09:00", "11:00", "13:00", "15:00", "17:00"].map(
              (time) => (
                <button
                  key={time}
                  onClick={() => setSelectedTime(time)}
                  className={cn(
                    "border p-2 rounded",
                    selectedTime === time && "bg-blue-500 text-white"
                  )}
                >
                  {time}
                </button>
              )
            )}
          </div>

          <div className="mt-6 flex gap-3">
            <Button onClick={() => setStep(0)}>Назад</Button>
            <Button
              onClick={() => setStep(2)}
              disabled={!selectedDate || !selectedTime}
            >
              Далі
            </Button>
          </div>
        </div>
      )}

      {/* STEP 2 - CONFIRM */}
      {step === 2 && (
        <div>
          <h2 className="text-xl mb-4">Підтвердження</h2>

          <div className="border p-4 rounded mb-4">
            <p>Послуга: {selectedService?.name}</p>
            <p>Ціна: {selectedService?.price} грн</p>
            <p>
              Дата: {selectedDate?.toLocaleDateString("uk-UA")}
            </p>
            <p>Час: {selectedTime}</p>
            <p>Клієнт: {user.name}</p>
          </div>

          <div className="mb-4">
            <Label>Авто</Label>
            <Input
              value={vehicleInfo}
              onChange={(e) => setVehicleInfo(e.target.value)}
              placeholder="Toyota Camry"
            />
          </div>

          <div className="flex gap-3">
            <Button onClick={() => setStep(1)}>Назад</Button>
            <Button onClick={handleConfirm}>
              Підтвердити
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}