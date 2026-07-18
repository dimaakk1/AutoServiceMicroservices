import { useEffect, useState } from "react";
import { useAuth } from "../lib/auth-context";
import { Button } from "../components/ui/button";
import { useNavigate } from "react-router-dom";
import { createCheckout } from "../api/payment";
import {
  CalendarDays,
  XCircle,
  Clock3,
  CheckCircle2,
  AlertCircle,
  Wrench,
} from "lucide-react";

import { toast } from "sonner";

import { getOrdersWithItems, updateOrder } from "../api/order";

export default function MyBookings() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [orders, setOrders] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);

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

  const handlePayment = async (order: any) => {
  try {
    const total =
      order.items?.reduce(
        (sum: number, item: any) => sum + item.totalPrice,
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
            "bg-yellow-100 text-yellow-700 border-yellow-200",
          icon: Clock3,
        };

      case "Confirmed":
        return {
          label: "Підтверджено",
          className:
            "bg-blue-100 text-blue-700 border-blue-200",
          icon: CheckCircle2,
        };

      case "Completed":
        return {
          label: "Завершено",
          className:
            "bg-green-100 text-green-700 border-green-200",
          icon: CheckCircle2,
        };

      case "Cancelled":
        return {
          label: "Скасовано",
          className:
            "bg-red-100 text-red-700 border-red-200",
          icon: AlertCircle,
        };

      default:
        return {
          label: status,
          className:
            "bg-gray-100 text-gray-700 border-gray-200",
          icon: Clock3,
        };
    }
  };

  if (!user) {
    return (
      <div className="container py-24 text-center">
        <div className="max-w-md mx-auto border rounded-2xl p-10 shadow-sm bg-card">
          <h2 className="text-3xl font-bold mb-4">
            Потрібен вхід
          </h2>

          <p className="text-muted-foreground mb-6">
            Увійдіть у свій акаунт для перегляду записів
          </p>

          <Button
            onClick={() => navigate("/auth")}
            className="bg-orange-500 hover:bg-orange-600 text-white"
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
    <div className="min-h-screen bg-muted/30">
      <div className="container py-12 max-w-5xl">

        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-5 mb-10">

          <div>
            <h1 className="text-4xl font-bold tracking-tight mb-2">
              Мої записи
            </h1>

            <p className="text-muted-foreground text-lg">
              Переглядайте статус ваших замовлень та керуйте ними
            </p>
          </div>

          <Button
            onClick={() => navigate("/booking")}
            className="bg-orange-500 hover:bg-orange-600 text-white rounded-xl px-6"
          >
            Новий запис
          </Button>
        </div>

        {orders.length === 0 ? (
          <div className="border rounded-2xl bg-card p-14 text-center shadow-sm">

            <div className="w-16 h-16 rounded-full bg-orange-100 flex items-center justify-center mx-auto mb-5">
              <CalendarDays className="h-8 w-8 text-orange-500" />
            </div>

            <h2 className="text-2xl font-semibold mb-3">
              У вас ще немає записів
            </h2>

            <p className="text-muted-foreground mb-8 max-w-md mx-auto">
              Створіть свій перший запис на сервіс і керуйте ним онлайн
            </p>

            <Button
              onClick={() => navigate("/booking")}
              className="bg-orange-500 hover:bg-orange-600 text-white"
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
                    (sum: number, i: any) =>
                      sum + i.totalPrice,
                    0
                  ) || 0;

                const status = getStatusStyles(order.status);
                const StatusIcon = status.icon;

                return (
                  <div
                    key={order.orderId}
                    className="rounded-2xl border bg-card shadow-sm hover:shadow-md transition-all"
                  >
                    {/* TOP */}
                    <div className="p-6 border-b">

                      <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-5">

                        <div>

                          <div className="flex items-center gap-3 mb-3">

                            <div className="w-12 h-12 rounded-xl bg-orange-100 flex items-center justify-center">
                              <Wrench className="h-6 w-6 text-orange-500" />
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

                      <div className="space-y-4">
                        {order.items?.map((item: any) => (
                          <div
                            key={item.orderItemId}
                            className="flex items-center justify-between rounded-xl border p-4 bg-muted/30"
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
      className="bg-green-600 hover:bg-green-700 text-white rounded-xl"
    >
      Оплатити
    </Button>
  )}

  {(order.status === "Pending" ||
    order.status === "Confirmed") && (
    <Button
      variant="outline"
      onClick={() => handleCancel(order.orderId)}
      className="border-red-200 text-red-600 hover:bg-red-50 rounded-xl"
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