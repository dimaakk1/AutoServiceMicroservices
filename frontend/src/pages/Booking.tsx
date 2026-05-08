import { useState, useMemo, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "../lib/auth-context";

import { Button } from "../components/ui/button";
import { Calendar } from "../components/ui/calendar";

import { createOrder, addOrderItem } from "../api/order";
import { getServices } from "../api/service";

import { Check, ChevronRight } from "lucide-react";
import { toast } from "sonner";
import { cn } from "../lib/utils";

type Service = {
  serviceId: number;
  name: string;
  price: number;
  categoryName: string;
};

const steps = ["Послуга", "Дата", "Підтвердження"];

const timeSlots = [
  "09:00","10:00","11:00",
  "12:00","14:00","15:00",
  "16:00","17:00"
];

export default function Booking() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const preselected = searchParams.get("service");

  const [step, setStep] = useState(preselected ? 1 : 0);

  const [services, setServices] = useState<Service[]>([]);
  const [loading, setLoading] = useState(true);

  const [selectedServiceId, setSelectedServiceId] = useState<number | null>(
    preselected ? Number(preselected) : null
  );

  const [selectedDate, setSelectedDate] = useState<Date | undefined>();
  const [selectedTime, setSelectedTime] = useState("");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    getServices()
      .then(res => setServices(res.data))
      .catch(() => toast.error("Помилка завантаження"))
      .finally(() => setLoading(false));
  }, []);

  const selectedService = useMemo(() => {
    return services.find(s => s.serviceId === selectedServiceId);
  }, [selectedServiceId, services]);

  if (!user) {
    return (
      <div className="container py-20 text-center">
        <h2 className="text-2xl font-bold text-orange-600 mb-4">
          Увійдіть для запису
        </h2>
        <Button onClick={() => navigate("/auth")} className="bg-orange-500">
          Увійти
        </Button>
      </div>
    );
  }

  const handleConfirm = async () => {
    if (!selectedService || !selectedDate || !selectedTime) return;

    setSubmitting(true);

    try {
      const dateTime = new Date(
        selectedDate.toDateString() + " " + selectedTime
      ).toISOString();

      const orderRes = await createOrder({
        orderDate: dateTime,
        status: "Pending",
      });

      const orderId = orderRes.data?.orderId;

      await addOrderItem({
        orderId,
        productId: selectedService.serviceId,
        quantity: 1,
      });

      toast.success("Запис створено");
      navigate("/my-bookings");
    } catch {
      toast.error("Помилка створення");
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return (
      <div className="container py-20 text-center text-orange-500">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="container py-10 max-w-4xl">

      {/* HEADER */}
      <h1 className="text-3xl font-bold text-orange-600 mb-8">
        Запис на сервіс
      </h1>

      {/* STEPPER */}
      <div className="flex items-center gap-3 mb-10">
        {steps.map((s, i) => (
          <div key={s} className="flex items-center">

            <div
              className={cn(
                "w-9 h-9 rounded-full flex items-center justify-center text-sm font-bold transition",
                i < step
                  ? "bg-green-500 text-white"
                  : i === step
                  ? "bg-orange-500 text-white"
                  : "bg-gray-200"
              )}
            >
              {i < step ? <Check size={14} /> : i + 1}
            </div>

            <span className="ml-2 text-sm font-medium">
              {s}
            </span>

            {i < steps.length - 1 && (
              <ChevronRight className="mx-3 text-gray-300" />
            )}
          </div>
        ))}
      </div>

      {/* STEP 1 */}
      {step === 0 && (
        <div>
          <h2 className="text-xl font-semibold mb-4">
            Оберіть послугу
          </h2>

          <div className="grid sm:grid-cols-2 gap-4">
            {services.map(s => (
              <button
                key={s.serviceId}
                onClick={() => {
                  setSelectedServiceId(s.serviceId);
                  setStep(1);
                }}
                className={cn(
                  "border rounded-xl p-5 text-left transition shadow-sm hover:shadow-md hover:border-orange-400",
                  selectedServiceId === s.serviceId &&
                    "border-orange-500 bg-orange-50"
                )}
              >
                <h3 className="font-semibold text-lg">
                  {s.name}
                </h3>

                <p className="text-sm text-orange-600 mt-1">
                  {s.categoryName}
                </p>

                <p className="mt-3 font-bold">
                  {s.price} ₴
                </p>
              </button>
            ))}
          </div>
        </div>
      )}

      {/* STEP 2 */}
      {step === 1 && (
        <div>
          <h2 className="text-xl font-semibold mb-4">
            Оберіть дату і час
          </h2>

          <div className="grid md:grid-cols-2 gap-6">

            {/* DATE */}
            <div className="border rounded-xl p-4 shadow-sm bg-white">
              <p className="text-orange-600 font-medium mb-3">
                Дата
              </p>

              <Calendar
                mode="single"
                selected={selectedDate}
                onSelect={setSelectedDate}
                weekStartsOn={1}
              />
            </div>

            {/* TIME */}
            <div className="border rounded-xl p-4 shadow-sm bg-white">
              <p className="text-orange-600 font-medium mb-3">
                Час
              </p>

              <div className="grid grid-cols-2 gap-2">
                {timeSlots.map(t => (
                  <button
                    key={t}
                    onClick={() => setSelectedTime(t)}
                    className={cn(
                      "border rounded-lg p-2 text-sm transition",
                      selectedTime === t
                        ? "bg-orange-500 text-white border-orange-500"
                        : "hover:border-orange-400"
                    )}
                  >
                    {t}
                  </button>
                ))}
              </div>
            </div>

          </div>

          <Button
            className="mt-6 bg-orange-500 hover:bg-orange-600"
            disabled={!selectedDate || !selectedTime}
            onClick={() => setStep(2)}
          >
            Далі
          </Button>
        </div>
      )}

      {/* STEP 3 */}
      {step === 2 && (
        <div>
          <h2 className="text-xl font-semibold mb-4">
            Підтвердження
          </h2>

          <div className="border rounded-xl p-5 bg-orange-50 space-y-2">
            <p><b>Послуга:</b> {selectedService?.name}</p>
            <p><b>Категорія:</b> {selectedService?.categoryName}</p>
            <p><b>Дата:</b> {selectedDate?.toLocaleDateString("uk-UA")}</p>
            <p><b>Час:</b> {selectedTime}</p>
            <p><b>Ціна:</b> {selectedService?.price} ₴</p>
          </div>

          <div className="flex gap-3 mt-6">
            <Button variant="outline" onClick={() => setStep(1)}>
              Назад
            </Button>

            <Button
              className="bg-orange-500 hover:bg-orange-600"
              onClick={handleConfirm}
              disabled={submitting}
            >
              {submitting ? "Створення..." : "Підтвердити"}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}