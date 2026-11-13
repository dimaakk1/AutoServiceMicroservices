import { useCallback, useEffect, useId, useState } from "react";
import type { FormEvent } from "react";
import { isAxiosError } from "axios";
import { Link } from "react-router-dom";
import { useAuth } from "../../lib/auth-context";

import { ArrowLeft } from "lucide-react";
import { Badge } from "../../components/ui/badge";
import { Button } from "../../components/ui/button";
import { Calendar } from "../../components/ui/calendar";
import { Input } from "../../components/ui/input";

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
import { setPlannedCompletion, updateAdminOrderStatus } from "../../api/order";


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
  vehicleId?: string | null;
  vehicleDisplayName?: string | null;
  vehicleVin?: string | null;
  vehicleLicensePlate?: string | null;
  mechanicId?: string | null;
  mechanicName?: string | null;
  plannedCompletionAt?: string | null;
  workStartedAt?: string | null;
  workCompletedAt?: string | null;
  planningLocked?: boolean;

  paymentStatus?: string | null;
  paymentId?: number;

  items: OrderItem[];
  review: Review | null;
};

type Payment = {
  orderId: number;
  paymentId: number;
  status: string;
};

const pageSize = 25;
const dateKey = (date: Date) =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;

type Mechanic = {
  userId: string;
  username: string;
  fullName?: string | null;
  isBlocked?: boolean;
};

function PlannedCompletionField({ order, onSaved }: {
  order: Order;
  onSaved: (plannedCompletionAt: string) => void;
}) {
  const fieldId = useId();
  const [saving, setSaving] = useState(false);
  const planned = order.plannedCompletionAt ? new Date(order.plannedCompletionAt) : null;
  const canEdit = !order.planningLocked && !order.workStartedAt && !order.workCompletedAt
    && ["Pending", "Confirmed"].includes(order.status);
  const localValue = planned && !Number.isNaN(planned.valueOf())
    ? new Date(planned.getTime() - planned.getTimezoneOffset() * 60_000).toISOString().slice(0, 16)
    : "";

  if (!canEdit) return <span>{planned ? planned.toLocaleString("uk-UA") : "Не вказано"}</span>;

  const save = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const value = new FormData(event.currentTarget).get("plannedCompletion");
    const date = new Date(typeof value === "string" ? value : "");
    if (Number.isNaN(date.valueOf())) {
      toast.error("Вкажіть дату й час завершення.");
      return;
    }

    setSaving(true);
    try {
      const timestamp = date.toISOString();
      await setPlannedCompletion(order.orderId, timestamp);
      onSaved(timestamp);
      toast.success("Плановий строк збережено");
    } catch (error) {
      const message = isAxiosError(error) ? error.response?.data?.message : undefined;
      toast.error(typeof message === "string" ? message : "Не вдалося зберегти плановий строк.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <form onSubmit={save} className="min-w-48 space-y-2">
      <label htmlFor={fieldId} className="sr-only">Планове завершення замовлення №{order.orderId}</label>
      <Input id={fieldId} name="plannedCompletion" type="datetime-local" defaultValue={localValue} required disabled={saving} />
      <Button type="submit" size="sm" variant="outline" disabled={saving}>{saving ? "Збереження…" : "Зберегти"}</Button>
    </form>
  );
}

export default function AdminOrders() {
  const { user } = useAuth();

  const [orders, setOrders] = useState<Order[]>([]);
  const [calendarOrders, setCalendarOrders] = useState<Order[]>([]);
  const [page, setPage] = useState(1);
  const [hasMore, setHasMore] = useState(false);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);
  const [retry, setRetry] = useState(0);
  const [mechanics, setMechanics] = useState<Mechanic[]>([]);

  const [selectedDate, setSelectedDate] = useState<Date | undefined>();

  const [filters, setFilters] = useState({
    status: "",
    fromDate: "",
    toDate: "",
  });

  const statuses = [
    "Pending",
    "Confirmed",
    "Diagnosing",
    "AwaitingApproval",
    "InProgress",
    "QualityCheck",
    "Completed",
    "Cancelled",
  ];


  const loadOrders = useCallback(async (signal: AbortSignal) => {
  try {
    setLoading(true);
    setLoadError(false);

    const params = {
          status: filters.status || undefined,
          fromDate: filters.fromDate || undefined,
          toDate: filters.toDate || undefined,
    };
    const [ordersRes, paymentsRes, mechanicsRes, calendarRes] = await Promise.all([
      api.get<{ orders: Order[]; hasMore: boolean }>("/aggregation/orders", {
        params: { ...params, page, pageSize }, signal,
      }),
      api.get<Payment[]>("/Orders/payments", { signal }),
      api.get<Mechanic[]>("/users/mechanics", { signal }),
      selectedDate ? api.get<{ orders: Order[] }>("/aggregation/orders", {
        params: { ...params, date: dateKey(selectedDate), page: 1, pageSize: 100 }, signal,
      }) : Promise.resolve(null),
    ]);
    if (signal.aborted) return;

    const payments = new Map(paymentsRes.data.map(payment => [payment.orderId, payment]));
    setMechanics(mechanicsRes.data.filter((mechanic: Mechanic) => !mechanic.isBlocked));

    const withPayment = (order: Order) => {
      const payment = payments.get(order.orderId);

      return {
        ...order,
        paymentStatus: payment?.status ?? null,
        paymentId: payment?.paymentId,
      };
    };

    setOrders(ordersRes.data.orders.map(withPayment));
    setHasMore(ordersRes.data.hasMore);
    setCalendarOrders(calendarRes?.data.orders.map(withPayment) ?? []);
  } catch {
    if (!signal.aborted) setLoadError(true);
  } finally {
    if (!signal.aborted) setLoading(false);
  }
  }, [filters, page, selectedDate]);

  useEffect(() => {
    if (user?.role !== "Admin") return;
    const controller = new AbortController();
    void loadOrders(controller.signal);
    return () => controller.abort();
  }, [user, loadOrders, retry]);

  const updateVisibleOrder = (orderId: number, patch: (order: Order) => Partial<Order>) => {
    const update = (list: Order[]) => list.map(order => order.orderId === orderId ? { ...order, ...patch(order) } : order);
    setOrders(update);
    setCalendarOrders(update);
  };

  const updateStatus = async (orderId: number, status: string) => {
    try {
      await updateAdminOrderStatus(orderId, status);

      const locksPlanning = ["Diagnosing", "InProgress", "Completed"].includes(status);
      updateVisibleOrder(orderId, order => ({ status, planningLocked: order.planningLocked || locksPlanning }));

      toast.success("Статус оновлено");
    } catch {
      toast.error("Помилка оновлення статусу");
    }
  };

  const assignOrderMechanic = async (orderId: number, mechanicId: string) => {
    const mechanic = mechanics.find((item) => item.userId === mechanicId);
    if (!mechanic) return;

    const mechanicName = mechanic.fullName || mechanic.username;
    try {
      await api.put(`/Orders/Order/${orderId}/assign-mechanic`, {
        mechanicId,
        mechanicName,
      });
      updateVisibleOrder(orderId, () => ({ mechanicId, mechanicName }));
      toast.success("Майстра призначено");
    } catch {
      toast.error("Не вдалося призначити майстра");
    }
  };


  const totalPrice = (order: Order) =>
    order.items.reduce((sum, i) => sum + i.price * i.quantity, 0);


  if (!user || user.role !== "Admin") {
    return (
      <div className="container py-10 text-center">
        Доступ заборонено
      </div>
    );
  }

  if (loading) {
    return (
      <div className="container py-10 text-center text-accent">
        Завантаження...
      </div>
    );
  }

  if (loadError) {
    return (
      <div role="alert" className="container flex flex-col items-center gap-4 py-10">
        Не вдалося завантажити замовлення.
        <Button type="button" variant="outline" onClick={() => setRetry(value => value + 1)}>Спробувати ще раз</Button>
      </div>
    );
  }

  return (
    <div className="container py-8 max-w-6xl">

      <div className="flex items-center gap-4 mb-6">
        <Link
          to="/admin"
          className="text-muted-foreground transition hover:text-accent"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>

        <h1 className="text-3xl font-bold">
          Замовлення
        </h1>

        <Badge className="ml-auto bg-accent text-accent-foreground">
          На сторінці: {orders.length}
        </Badge>
      </div>

      <div className="bg-card border rounded-xl p-4 mb-6 flex flex-wrap gap-3">

        <select
          className="border rounded-lg px-3 py-2 text-sm"
          value={filters.status}
          onChange={(e) => { setPage(1); setFilters({ ...filters, status: e.target.value }); }}
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
          onChange={(e) => { setPage(1); setFilters({ ...filters, fromDate: e.target.value }); }}
        />

        <input
          type="date"
          className="border rounded-lg px-3 py-2 text-sm"
          value={filters.toDate}
          onChange={(e) => { setPage(1); setFilters({ ...filters, toDate: e.target.value }); }}
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
  ) : calendarOrders.length === 0 ? (
    <p className="text-muted-foreground">
      Немає записів
    </p>
  ) : (
    <div className="space-y-3">
      {calendarOrders.map((o) => (
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

  <div className="text-sm text-muted-foreground">
    {o.vehicleDisplayName || "Автомобіль не вказано"}
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

            <Select
              value={o.mechanicId || undefined}
              onValueChange={(value) => assignOrderMechanic(o.orderId, value)}
            >
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="Призначити майстра" />
              </SelectTrigger>
              <SelectContent>
                {mechanics.map((mechanic) => (
                  <SelectItem key={mechanic.userId} value={mechanic.userId}>
                    {mechanic.fullName || mechanic.username}
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
          <TableHeader className="bg-accent">
            <TableRow>
              <TableHead className="text-white">ID</TableHead>
              <TableHead className="text-white">Користувач</TableHead>
              <TableHead className="text-white">Дата</TableHead>
              <TableHead className="text-white">Планове завершення</TableHead>
              <TableHead className="text-white">Автомобіль</TableHead>
              <TableHead className="text-white">Послуги</TableHead>
       <TableHead className="text-white">Сума</TableHead>
<TableHead className="text-white">Оплата</TableHead>
<TableHead className="text-white">Відгук</TableHead>
<TableHead className="text-white">Статус</TableHead>
<TableHead className="text-white">Майстер</TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {orders.map((order) => (
              <TableRow
                key={order.orderId}
                className="hover:bg-muted/40 transition"
              >

                <TableCell className="font-semibold text-accent">
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

                <TableCell>
                  <PlannedCompletionField
                    order={order}
                    onSaved={(plannedCompletionAt) => updateVisibleOrder(order.orderId, () => ({ plannedCompletionAt }))}
                  />
                </TableCell>

                <TableCell className="text-sm">
                  <div className="font-medium">{order.vehicleDisplayName || "Не вказано"}</div>
                  {order.vehicleLicensePlate && <div className="text-xs text-muted-foreground">{order.vehicleLicensePlate}</div>}
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

                <TableCell>
                  <Select
                    value={order.mechanicId || undefined}
                    onValueChange={(value) => assignOrderMechanic(order.orderId, value)}
                  >
                    <SelectTrigger className="w-[180px]">
                      <SelectValue placeholder="Не призначено" />
                    </SelectTrigger>
                    <SelectContent>
                      {mechanics.map((mechanic) => (
                        <SelectItem key={mechanic.userId} value={mechanic.userId}>
                          {mechanic.fullName || mechanic.username}
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
      <div className="mt-4 flex items-center justify-between gap-3">
        <Button type="button" variant="outline" disabled={page === 1} onClick={() => setPage(p => p - 1)}>Назад</Button>
        <span className="text-sm text-muted-foreground">Сторінка {page}</span>
        <Button type="button" variant="outline" disabled={!hasMore} onClick={() => setPage(p => p + 1)}>Далі</Button>
      </div>

    </div>
  );
}
