import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../lib/auth-context";

import { ArrowLeft } from "lucide-react";
import { Badge } from "../../components/ui/badge";
import { Calendar } from "../../components/ui/calendar";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "../../components/ui/table";

import { toast } from "sonner";
import api from "../../api/api";


type Review = {
  _id: string;
  rating: number;
  comment: string;
  createdAt: string;
};

type OrderItem = {
  productId: number;
  productName: string;
  quantity: number;
  price: number;
};

type Order = {
  orderId: number;
  userId: string;
  username: string;
  email: string;
  orderDate: string;
  status: string;

  paymentStatus?: string | null;
  paymentId?: number;

  items: OrderItem[];
  review: Review | null;
};


export default function AdminOrders() {
  const { user } = useAuth();

  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  const [selectedDate, setSelectedDate] = useState<Date | undefined>();

  const [filters, setFilters] = useState({
    status: "",
    fromDate: "",
    toDate: "",
  });

  const statuses = [
    "Pending",
    "Confirmed",
    "InProgress",
    "Completed",
    "Cancelled",
  ];


  const loadOrders = async () => {
  try {
    setLoading(true);

    const [ordersRes, paymentsRes] = await Promise.all([
      api.get("/aggregation/orders", {
        params: {
          status: filters.status || undefined,
          fromDate: filters.fromDate || undefined,
          toDate: filters.toDate || undefined,
        },
      }),
      api.get("/Orders/payments"),
    ]);

    const payments = paymentsRes.data;

    const ordersWithPayments = ordersRes.data.map((order: any) => {
      const payment = payments.find(
        (p: any) => p.orderId === order.orderId
      );

      return {
        ...order,
        paymentStatus: payment?.status ?? null,
        paymentId: payment?.paymentId,
      };
    });

    setOrders(ordersWithPayments);
  } catch {
    toast.error("Не вдалося завантажити замовлення");
  } finally {
    setLoading(false);
  }
};

  useEffect(() => {
    if (user?.role === "Admin") loadOrders();
  }, [user, filters]);


  const updateStatus = async (orderId: number, status: string) => {
    try {
      await api.put("/Orders/Order", {
        orderId,
        status,
        orderDate: new Date().toISOString(),
      });

      setOrders((prev) =>
        prev.map((o) =>
          o.orderId === orderId ? { ...o, status } : o
        )
      );

      toast.success("Статус оновлено");
    } catch {
      toast.error("Помилка оновлення статусу");
    }
  };


  const totalPrice = (order: Order) =>
    order.items.reduce((sum, i) => sum + i.price * i.quantity, 0);

  const filteredByDate = selectedDate
    ? orders.filter((o) => {
        const d = new Date(o.orderDate);
        return d.toDateString() === selectedDate.toDateString();
      })
    : [];


  if (!user || user.role !== "Admin") {
    return (
      <div className="container py-10 text-center">
        Доступ заборонено
      </div>
    );
  }

  if (loading) {
    return (
      <div className="container py-10 text-center text-orange-500">
        Завантаження...
      </div>
    );
  }


  return (
    <div className="container py-8 max-w-6xl">

      <div className="flex items-center gap-4 mb-6">
        <Link
          to="/admin"
          className="text-muted-foreground hover:text-orange-500 transition"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>

        <h1 className="text-3xl font-bold">
          Замовлення
        </h1>

        <Badge className="ml-auto bg-orange-500 text-white">
          {orders.length}
        </Badge>
      </div>

      <div className="bg-card border rounded-xl p-4 mb-6 flex flex-wrap gap-3">

        <select
          className="border rounded-lg px-3 py-2 text-sm"
          value={filters.status}
          onChange={(e) =>
            setFilters({ ...filters, status: e.target.value })
          }
        >
          <option value="">Всі статуси</option>
          {statuses.map((s) => (
            <option key={s} value={s}>
              {s}
            </option>
          ))}
        </select>

        <input
          type="date"
          className="border rounded-lg px-3 py-2 text-sm"
          value={filters.fromDate}
          onChange={(e) =>
            setFilters({ ...filters, fromDate: e.target.value })
          }
        />

        <input
          type="date"
          className="border rounded-lg px-3 py-2 text-sm"
          value={filters.toDate}
          onChange={(e) =>
            setFilters({ ...filters, toDate: e.target.value })
          }
        />
      </div>

      <div className="grid lg:grid-cols-[320px_1fr] gap-6 mb-6">

        <div className="border rounded-xl p-4 bg-card">
          <h2 className="font-semibold mb-3">
            Календар записів
          </h2>

          <Calendar
            mode="single"
            selected={selectedDate}
            onSelect={setSelectedDate}
            className="rounded-md border"
          />
        </div>

<div className="border rounded-xl p-4 bg-card">
  <h2 className="font-semibold mb-3">
    Записи на обрану дату
  </h2>

  {!selectedDate ? (
    <p className="text-muted-foreground">
      Оберіть дату
    </p>
  ) : filteredByDate.length === 0 ? (
    <p className="text-muted-foreground">
      Немає записів
    </p>
  ) : (
    <div className="space-y-3">
      {filteredByDate.map((o) => (
        <div
          key={o.orderId}
          className="border rounded-lg p-3 flex justify-between items-center"
        >
          <div>
  <div className="font-medium">
    #{o.orderId} — {o.username}
  </div>

  <div className="text-sm text-muted-foreground">
    {o.items.map(i => i.productName).join(", ")}
  </div>

  <div className="mt-2">
  {o.paymentStatus?.toLowerCase() === "success" ? (
    <Badge className="bg-green-500 text-white">
      Оплачено
    </Badge>
  ) : o.paymentStatus?.toLowerCase() === "pending" ? (
    <Badge className="bg-yellow-500 text-white">
      Очікує оплату
    </Badge>
  ) : o.paymentStatus ? (
    <Badge className="bg-red-500 text-white">
      {o.paymentStatus}
    </Badge>
  ) : (
    <Badge variant="outline">
      Без платежу
    </Badge>
  )}
</div>
</div>

          <div className="flex items-center gap-4">

            <div className="text-right">
              <div className="font-semibold">
                {new Date(o.orderDate).toLocaleTimeString([], {
                  hour: "2-digit",
                  minute: "2-digit",
                })}
              </div>
            </div>

            <Select
              value={o.status}
              onValueChange={(v) =>
                updateStatus(o.orderId, v)
              }
            >
              <SelectTrigger className="w-[140px]">
                <SelectValue />
              </SelectTrigger>

              <SelectContent>
                {statuses.map((s) => (
                  <SelectItem key={s} value={s}>
                    {s}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

          </div>
        </div>
      ))}
    </div>
  )}
</div>
      </div>

      <div className="border rounded-xl overflow-hidden shadow-sm">

        <Table>
          <TableHeader className="bg-orange-500">
            <TableRow>
              <TableHead className="text-white">ID</TableHead>
              <TableHead className="text-white">Користувач</TableHead>
              <TableHead className="text-white">Дата</TableHead>
              <TableHead className="text-white">Послуги</TableHead>
       <TableHead className="text-white">Сума</TableHead>
<TableHead className="text-white">Оплата</TableHead>
<TableHead className="text-white">Відгук</TableHead>
<TableHead className="text-white">Статус</TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {orders.map((order) => (
              <TableRow
                key={order.orderId}
                className="hover:bg-muted/40 transition"
              >

                <TableCell className="font-semibold text-orange-600">
                  #{order.orderId}
                </TableCell>

                <TableCell>
                  <div className="font-medium">{order.username}</div>
                  <div className="text-xs text-muted-foreground">
                    {order.email}
                  </div>
                </TableCell>

                <TableCell>
                  {new Date(order.orderDate).toLocaleString()}
                </TableCell>

                <TableCell className="text-sm space-y-1">
                  {order.items.map((i) => (
                    <div key={i.productId}>
                      {i.productName} × {i.quantity}
                    </div>
                  ))}
                </TableCell>

                <TableCell className="font-semibold">
                  {totalPrice(order).toFixed(2)} ₴
                </TableCell>

                <TableCell>
  {order.paymentStatus === "Success" && (
    <Badge className="bg-green-500 text-white">
      Оплачено
    </Badge>
  )}

  {order.paymentStatus === "Pending" && (
    <Badge className="bg-yellow-500 text-white">
      Очікує оплату
    </Badge>
  )}

  {order.paymentStatus === "Failure" && (
    <Badge className="bg-red-500 text-white">
      Помилка
    </Badge>
  )}

  {!order.paymentStatus && (
    <Badge variant="outline">
      Без платежу
    </Badge>
  )}
</TableCell>

                <TableCell className="text-sm">
                  {order.review ? (
                    <>
                      <div>⭐ {order.review.rating}</div>
                      <div className="text-xs text-muted-foreground">
                        {order.review.comment}
                      </div>
                    </>
                  ) : (
                    <span className="text-muted-foreground">
                      Немає
                    </span>
                  )}
                </TableCell>

                <TableCell>
                  <Select
                    value={order.status}
                    onValueChange={(v) =>
                      updateStatus(order.orderId, v)
                    }
                  >
                    <SelectTrigger className="w-[140px]">
                      <SelectValue />
                    </SelectTrigger>

                    <SelectContent>
                      {statuses.map((s) => (
                        <SelectItem key={s} value={s}>
                          {s}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </TableCell>

              </TableRow>
            ))}
          </TableBody>

        </Table>
      </div>

    </div>
  );
}