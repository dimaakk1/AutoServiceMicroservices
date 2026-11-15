import { useEffect, useState } from "react";
import type { ReactNode } from "react";
import { useAuth } from "../lib/auth-context";
import { Button } from "../components/ui/button";
import { Link, useNavigate } from "react-router-dom";
import { createCheckout } from "../api/payment";
import {
  CalendarDays,
  XCircle,
  Clock3,
  CheckCircle2,
  AlertCircle,
  Wrench,
  Car,
} from "lucide-react";

import { toast } from "sonner";

import { getOrdersWithItems, updateOrder } from "../api/order";
import { getVehicles } from "../api/vehicle";
import type { Vehicle } from "../api/vehicle";
import { PageHeader } from "../components/PageHeader";
import { OrderForecast } from "../components/OrderForecast";

type BookingItem = {
  orderItemId: number;
  productName: string;
  quantity: number;
  totalPrice: number;
};

type BookingOrder = {
  orderId: number;
  orderDate: string;
  status: string;
  vehicleId?: string | null;
  mechanicId?: string | null;
  vehicleDisplayName?: string | null;
  vehicleLicensePlate?: string | null;
  items: BookingItem[];
};

type GarageCheck = { ownerId: string; vehicles: Vehicle[] | null };

function getDurationBlockers(order: BookingOrder, garage: GarageCheck | null): ReactNode[] {
  if (order.status === "Completed") return ["Ремонт уже завершено. Прогноз потрібен лише для активних записів."];
  if (order.status === "Cancelled") return ["Запис скасовано."];

  const reasons: ReactNode[] = [];
  if (!order.mechanicId?.trim()) reasons.push("Майстра ще не призначено.");
  if (!order.items?.length) reasons.push("До запису не додано жодної послуги.");
  if (!order.vehicleId) {
    reasons.push("До запису не прив’язано автомобіль.");
  } else if (!garage) {
    reasons.push("Перевіряємо дані автомобіля…");
  } else if (!garage.vehicles) {
    reasons.push("Не вдалося перевірити дані авто. Оновіть сторінку й спробуйте ще раз.");
  } else {
    const vehicle = garage.vehicles.find((item) => item.id.toLowerCase() === order.vehicleId?.toLowerCase());
    if (!vehicle) {
      reasons.push("Автомобіля з цього запису більше немає у вашому гаражі.");
    } else if (typeof vehicle.mileageKm !== "number" || !Number.isInteger(vehicle.mileageKm) || vehicle.mileageKm < 0) {
      reasons.push(<>У профілі авто не вказано пробіг. <Link className="underline" to={`/my-vehicles/${vehicle.id}/edit`}>Вказати пробіг</Link></>);
    }
  }
  return reasons;
}

export default function MyBookings() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [orders, setOrders] = useState<BookingOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [garageCheck, setGarageCheck] = useState<GarageCheck | null>(null);
  const ownerId = user?.id;

  useEffect(() => {
    if (!ownerId) return;
    const controller = new AbortController();
    getVehicles(AbortSignal.any([controller.signal, AbortSignal.timeout(8000)]))
      .then(({ data }) => {
        if (!controller.signal.aborted) setGarageCheck({ ownerId, vehicles: data });
      })
      .catch(() => {
        if (!controller.signal.aborted) setGarageCheck({ ownerId, vehicles: null });
      });
    return () => controller.abort();
  }, [ownerId]);

  useEffect(() => {
    if (!user) return;

    getOrdersWithItems()
      .then((res) => setOrders(res.data))
      .catch((err) => {
        console.error(err);
        toast.error("Не вдалося завантажити записи");
      })
      .finally(() => setLoading(false));
  }, [user]);

  const handleCancel = async (orderId: number) => {
    try {
      await updateOrder({
        orderId,
        status: "Cancelled",
        orderDate: new Date().toISOString(),
      });

      setOrders((prev) =>
        prev.map((o) =>
          o.orderId === orderId
            ? { ...o, status: "Cancelled" }
            : o
        )
      );

      toast.success("Запис скасовано");
    } catch (err) {
      console.error(err);
      toast.error("Помилка скасування");
    }
  };

  const handlePayment = async (order: BookingOrder) => {
  try {
    const total =
      order.items?.reduce(
        (sum: number, item: BookingItem) => sum + item.totalPrice,
        0
      ) || 100;

    const response = await createCheckout({
      orderId: order.orderId,
      amount: total,
      description: `Оплата замовлення #${order.orderId}`,
    });

    const { data, signature } = response.data;

    const form = document.createElement("form");
    form.method = "POST";
    form.action = "https://www.liqpay.ua/api/3/checkout";
    form.acceptCharset = "utf-8";

    const dataInput = document.createElement("input");
    dataInput.type = "hidden";
    dataInput.name = "data";
    dataInput.value = data;

    const signatureInput = document.createElement("input");
    signatureInput.type = "hidden";
    signatureInput.name = "signature";
    signatureInput.value = signature;

    form.appendChild(dataInput);
    form.appendChild(signatureInput);

    document.body.appendChild(form);
    form.submit();
  } catch (error) {
    console.error(error);
    toast.error("Помилка створення платежу");
  }
};

  const getStatusStyles = (status: string) => {
    switch (status) {
      case "Pending":
        return {
          label: "Очікує",
          className:
            "border-amber-500/30 bg-amber-500/10 text-amber-400",
          icon: Clock3,
        };

      case "Confirmed":
        return {
          label: "Підтверджено",
          className:
            "border-blue-500/30 bg-blue-500/10 text-blue-400",
          icon: CheckCircle2,
        };

      case "Completed":
        return {
          label: "Завершено",
          className:
            "border-emerald-500/30 bg-emerald-500/10 text-emerald-400",
          icon: CheckCircle2,
        };

      case "Cancelled":
        return {
          label: "Скасовано",
          className:
            "border-red-500/30 bg-red-500/10 text-red-400",
          icon: AlertCircle,
        };

      default:
        return {
          label: status,
          className:
            "border-border bg-muted/60 text-muted-foreground",
          icon: Clock3,
        };
    }
  };

  if (!user) {
    return (
      <div className="container py-24 text-center">
        <div className="mx-auto max-w-md border border-border bg-card p-10 shadow-card">
          <h2 className="text-3xl font-bold mb-4">
            Потрібен вхід
          </h2>

          <p className="text-muted-foreground mb-6">
            Увійдіть у свій акаунт для перегляду записів
          </p>

          <Button
            onClick={() => navigate("/auth")}
            className="bg-accent text-accent-foreground hover:bg-accent/90"
          >
            Увійти
          </Button>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="container py-24 text-center">
        <div className="text-lg font-medium">
          Завантаження записів...
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-muted/20">
      <PageHeader
        eyebrow="Особистий кабінет"
        title="Мої записи"
        description="Статуси замовлень, вибрані послуги, оплата та керування візитами в одному місці."
      >
          <Button
            onClick={() => navigate("/booking")}
            className="px-6"
          >
            Новий запис
          </Button>
      </PageHeader>
      <div className="container max-w-5xl py-10 md:py-14">

        {orders.length === 0 ? (
          <div className="border border-border bg-card p-14 text-center shadow-card">

            <div className="mx-auto mb-5 flex h-16 w-16 items-center justify-center border border-accent/30 bg-accent/10">
              <CalendarDays className="h-8 w-8 text-accent" />
            </div>

            <h2 className="text-2xl font-semibold mb-3">
              У вас ще немає записів
            </h2>

            <p className="text-muted-foreground mb-8 max-w-md mx-auto">
              Створіть свій перший запис на сервіс і керуйте ним онлайн
            </p>

            <Button
              onClick={() => navigate("/booking")}
              className="bg-accent text-accent-foreground hover:bg-accent/90"
            >
              Записатися
            </Button>
          </div>
        ) : (
          <div className="space-y-6">

            {orders
              .slice()
              .reverse()
              .map((order) => {
                const total =
                  order.items?.reduce(
                    (sum: number, i: BookingItem) =>
                      sum + i.totalPrice,
                    0
                  ) || 0;

                const status = getStatusStyles(order.status);
                const StatusIcon = status.icon;

                return (
                  <div
                    key={order.orderId}
                    className="border border-border bg-card shadow-card transition-colors hover:border-accent/40"
                  >
                    {/* TOP */}
                    <div className="p-6 border-b">

                      <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-5">

                        <div>

                          <div className="flex items-center gap-3 mb-3">

                            <div className="flex h-12 w-12 items-center justify-center border border-accent/30 bg-accent/10">
                              <Wrench className="h-6 w-6 text-accent" />
                            </div>

                            <div>
                              <h3 className="text-xl font-semibold">
                                Замовлення #{order.orderId}
                              </h3>

                              <div className="flex items-center gap-2 text-sm text-muted-foreground mt-1">
                                <CalendarDays className="h-4 w-4" />

                                {new Date(
                                  order.orderDate
                                ).toLocaleString("uk-UA")}
                              </div>
                            </div>
                          </div>
                        </div>

                        <div
                          className={`inline-flex items-center gap-2 px-4 py-2 rounded-full border text-sm font-medium ${status.className}`}
                        >
                          <StatusIcon className="h-4 w-4" />
                          {status.label}
                        </div>
                      </div>
                    </div>

                    <div className="p-6">

                      <div className="mb-5 flex items-start gap-3 border border-border bg-muted/30 p-4">
                        <Car className="mt-0.5 h-5 w-5 shrink-0 text-accent" />
                        <div>
                          <p className="text-xs uppercase tracking-wider text-muted-foreground">Автомобіль</p>
                          <p className="mt-1 font-semibold">{order.vehicleDisplayName || "Не вказано"}</p>
                          {order.vehicleLicensePlate && <p className="mt-1 text-sm text-muted-foreground">Номер: {order.vehicleLicensePlate}</p>}
                        </div>
                      </div>

                      <div className="mb-5 border border-border bg-muted/30 p-4">
                        <OrderForecast
                          key={JSON.stringify([order.orderId, order.vehicleId, order.mechanicId, order.orderDate, order.status])}
                          orderId={order.orderId}
                          kind="duration"
                          blockedBy={getDurationBlockers(order, garageCheck?.ownerId === ownerId ? garageCheck : null)}
                        />
                      </div>

                      <div className="space-y-4">
                        {order.items?.map((item: BookingItem) => (
                          <div
                            key={item.orderItemId}
                            className="flex items-center justify-between border border-border bg-muted/30 p-4"
                          >
                            <div>
                              <p className="font-semibold">
                                {item.productName}
                              </p>

                              <p className="text-sm text-muted-foreground mt-1">
                                Кількість: {item.quantity}
                              </p>
                            </div>

                            <div className="text-right">
                              <p className="font-bold text-lg">
                                {item.totalPrice} ₴
                              </p>
                            </div>
                          </div>
                        ))}
                      </div>

                      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4 mt-8 pt-5 border-t">

                        <div>
                          <p className="text-sm text-muted-foreground mb-1">
                            Загальна сума
                          </p>

                          <p className="text-2xl font-bold">
                            {total.toFixed(2)} ₴
                          </p>
                        </div>

                        <div className="flex gap-3">
  {order.status === "Pending" && (
    <Button
      onClick={() => handlePayment(order)}
      className="bg-green-600 text-white hover:bg-green-700"
    >
      Оплатити
    </Button>
  )}

  {(order.status === "Pending" ||
    order.status === "Confirmed") && (
    <Button
      variant="outline"
      onClick={() => handleCancel(order.orderId)}
      className="border-red-500/30 text-red-400 hover:bg-red-500/10"
    >
      <XCircle className="h-4 w-4 mr-2" />
      Скасувати запис
    </Button>
  )}
</div>
                      </div>
                    </div>
                  </div>
                );
              })}
          </div>
        )}
      </div>
    </div>
  );
}
