import { useEffect, useState } from "react";
import { useAuth } from "../lib/auth-context";
import { Button } from "../components/ui/button";
import { useNavigate } from "react-router-dom";
import { CalendarDays, XCircle } from "lucide-react";
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
          o.orderId === orderId ? { ...o, status: "Cancelled" } : o
        )
      );

      toast.success("Запис скасовано");
    } catch (err) {
      console.error(err);
      toast.error("Помилка скасування");
    }
  };

  if (!user) {
    return (
      <div className="container py-20 text-center">
        <h2 className="text-2xl mb-4">Увійдіть для перегляду записів</h2>
        <Button
          onClick={() => navigate("/auth")}
          className="bg-orange-500 hover:bg-orange-600 text-white"
        >
          Увійти
        </Button>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="container py-20 text-center text-orange-500">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="container py-12 max-w-4xl">
      {/* HEADER */}
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold text-orange-600">
          Мої записи
        </h1>

        <Button
          onClick={() => navigate("/booking")}
          className="bg-orange-500 hover:bg-orange-600 text-white"
        >
          Новий запис
        </Button>
      </div>

      {/* EMPTY */}
      {orders.length === 0 ? (
        <div className="bg-card border rounded-lg p-12 text-center">
          <p className="text-muted-foreground mb-4">
            У вас ще немає записів
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
              const total = order.items?.reduce(
                (sum: number, i: any) => sum + i.totalPrice,
                0
              );

              return (
                <div
                  key={order.orderId}
                  className="border rounded-lg p-5 bg-white shadow-sm hover:shadow-md transition"
                >
                  {/* HEADER */}
                  <div className="flex justify-between mb-3">
                    <div>
                      <h3 className="font-semibold text-orange-600">
                        Замовлення #{order.orderId}
                      </h3>

                      <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <CalendarDays className="h-4 w-4" />
                        {new Date(order.orderDate).toLocaleString()}
                      </div>
                    </div>

                    {/* STATUS BADGE */}
                    <span
                      className={`text-xs px-3 py-1 rounded-full font-medium ${
                        order.status === "Pending"
                          ? "bg-yellow-100 text-yellow-700"
                          : order.status === "Confirmed"
                          ? "bg-blue-100 text-blue-700"
                          : order.status === "Completed"
                          ? "bg-green-100 text-green-700"
                          : order.status === "Cancelled"
                          ? "bg-red-100 text-red-700"
                          : "bg-gray-100 text-gray-700"
                      }`}
                    >
                      {order.status}
                    </span>
                  </div>

                  {/* ITEMS */}
                  <div className="space-y-2 mb-4">
                    {order.items?.map((item: any) => (
                      <div
                        key={item.orderItemId}
                        className="flex justify-between text-sm border-b pb-2"
                      >
                        <div>
                          <p className="font-medium">
                            {item.productName}
                          </p>
                          <p className="text-xs text-muted-foreground">
                            x{item.quantity}
                          </p>
                        </div>

                        <div className="text-right font-medium text-orange-600">
                          {item.totalPrice} ₴
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* TOTAL + ACTION */}
                  <div className="flex justify-between items-center">
                    <p className="font-semibold text-orange-600">
                      Разом: {total} ₴
                    </p>

                    {(order.status === "Pending" ||
                      order.status === "Confirmed") && (
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleCancel(order.orderId)}
                        className="text-red-500 border-red-200 hover:bg-red-50"
                      >
                        <XCircle className="h-4 w-4 mr-1" />
                        Скасувати
                      </Button>
                    )}
                  </div>
                </div>
              );
            })}
        </div>
      )}
    </div>
  );
}