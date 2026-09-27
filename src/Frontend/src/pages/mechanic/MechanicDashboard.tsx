import { useCallback, useEffect, useMemo, useState } from "react";
import { CalendarDays, Car, CheckCircle2, Clock3, Gauge, Wrench } from "lucide-react";
import { toast } from "sonner";
import { PageHeader } from "../../components/PageHeader";
import { Badge } from "../../components/ui/badge";
import { Button } from "../../components/ui/button";
import { Card, CardContent } from "../../components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../../components/ui/select";
import {
  getMechanicOrders,
  updateMechanicOrderStatus,
} from "../../api/order";

type OrderItem = {
  orderItemId: number;
  productId: number;
  productName?: string;
  quantity: number;
};

type MechanicOrder = {
  orderId: number;
  orderDate: string;
  status: string;
  vehicleDisplayName?: string | null;
  vehicleVin?: string | null;
  vehicleLicensePlate?: string | null;
  workStartedAt?: string | null;
  workCompletedAt?: string | null;
  items: OrderItem[];
};

const statuses = [
  { value: "Pending", label: "Очікує підтвердження" },
  { value: "Confirmed", label: "Підтверджено" },
  { value: "Diagnosing", label: "Діагностика" },
  { value: "AwaitingApproval", label: "Очікує погодження" },
  { value: "InProgress", label: "Ремонт" },
  { value: "QualityCheck", label: "Контроль якості" },
  { value: "Completed", label: "Завершено" },
];

const statusLabel = (status: string) =>
  statuses.find((item) => item.value === status)?.label ?? status;

export default function MechanicDashboard() {
  const [orders, setOrders] = useState<MechanicOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [updatingId, setUpdatingId] = useState<number | null>(null);

  const loadOrders = useCallback(async () => {
    try {
      setLoading(true);
      const { data } = await getMechanicOrders();
      setOrders(data);
    } catch {
      toast.error("Не вдалося завантажити призначені роботи");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadOrders();
  }, [loadOrders]);

  const activeOrders = useMemo(
    () => orders.filter((order) => order.status !== "Completed" && order.status !== "Cancelled"),
    [orders]
  );

  const todayOrders = useMemo(() => {
    const today = new Date().toDateString();
    return activeOrders.filter((order) => new Date(order.orderDate).toDateString() === today).length;
  }, [activeOrders]);

  const updateStatus = async (orderId: number, status: string) => {
    try {
      setUpdatingId(orderId);
      await updateMechanicOrderStatus(orderId, status);
      setOrders((current) =>
        current.map((order) =>
          order.orderId === orderId ? { ...order, status } : order
        )
      );
      toast.success("Статус роботи оновлено");
    } catch {
      toast.error("Не вдалося оновити статус");
    } finally {
      setUpdatingId(null);
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <PageHeader
        eyebrow="Робоча зона"
        title="Кабінет майстра"
        description="Призначені автомобілі, поточний етап ремонту та заплановані роботи."
      >
        <Button variant="outline" onClick={loadOrders} disabled={loading}>
          Оновити
        </Button>
      </PageHeader>

      <div className="container py-8 md:py-12">
        <div className="mb-8 grid gap-px bg-border sm:grid-cols-3">
          <div className="bg-card p-5">
            <span className="technical-label">Сьогодні</span>
            <div className="mt-2 flex items-center justify-between">
              <strong className="font-display text-4xl font-normal">{todayOrders}</strong>
              <CalendarDays className="text-accent" />
            </div>
          </div>
          <div className="bg-card p-5">
            <span className="technical-label">Активні роботи</span>
            <div className="mt-2 flex items-center justify-between">
              <strong className="font-display text-4xl font-normal">{activeOrders.length}</strong>
              <Wrench className="text-accent" />
            </div>
          </div>
          <div className="bg-card p-5">
            <span className="technical-label">Завершено</span>
            <div className="mt-2 flex items-center justify-between">
              <strong className="font-display text-4xl font-normal">
                {orders.filter((order) => order.status === "Completed").length}
              </strong>
              <CheckCircle2 className="text-accent" />
            </div>
          </div>
        </div>

        {loading ? (
          <div className="py-20 text-center text-muted-foreground">Завантаження робіт…</div>
        ) : orders.length === 0 ? (
          <Card className="border-dashed">
            <CardContent className="py-20 text-center">
              <Gauge className="mx-auto mb-4 h-10 w-10 text-accent" />
              <h2 className="text-2xl">Призначених робіт поки немає</h2>
              <p className="mt-2 text-muted-foreground">Нові замовлення з’являться після призначення адміністратором.</p>
            </CardContent>
          </Card>
        ) : (
          <div className="space-y-4">
            {orders.map((order) => (
              <Card key={order.orderId} className="overflow-hidden border-border shadow-card">
                <CardContent className="p-0">
                  <div className="grid lg:grid-cols-[1fr_240px]">
                    <div className="p-6">
                      <div className="mb-5 flex flex-wrap items-start justify-between gap-4">
                        <div>
                          <div className="technical-label mb-2">Замовлення #{order.orderId}</div>
                          <h2 className="text-3xl">
                            {order.vehicleDisplayName || "Автомобіль не вказано"}
                          </h2>
                        </div>
                        <Badge variant="outline">{statusLabel(order.status)}</Badge>
                      </div>

                      <div className="grid gap-3 text-sm sm:grid-cols-2">
                        <div className="flex items-center gap-2 text-muted-foreground">
                          <Clock3 className="h-4 w-4 text-accent" />
                          {new Date(order.orderDate).toLocaleString("uk-UA")}
                        </div>
                        <div className="flex items-center gap-2 text-muted-foreground">
                          <Car className="h-4 w-4 text-accent" />
                          {order.vehicleLicensePlate || order.vehicleVin || "Без даних автомобіля"}
                        </div>
                      </div>

                      <div className="mt-5 border-t border-border pt-4">
                        <p className="technical-label mb-2">Заплановані роботи</p>
                        {order.items.length > 0 ? (
                          <ul className="space-y-1 text-sm">
                            {order.items.map((item) => (
                              <li key={item.orderItemId} className="flex items-center gap-2">
                                <span className="h-1.5 w-1.5 bg-accent" />
                                {item.productName || `Послуга #${item.productId}`}
                                {item.quantity > 1 ? ` × ${item.quantity}` : ""}
                              </li>
                            ))}
                          </ul>
                        ) : (
                          <p className="text-sm text-muted-foreground">Послуги не завантажені</p>
                        )}
                      </div>
                    </div>

                    <div className="border-t border-border bg-muted/30 p-5 lg:border-l lg:border-t-0">
                      <p className="technical-label mb-3">Етап роботи</p>
                      <Select
                        value={order.status}
                        disabled={updatingId === order.orderId || order.status === "Cancelled"}
                        onValueChange={(value) => updateStatus(order.orderId, value)}
                      >
                        <SelectTrigger className="w-full">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {statuses.map((status) => (
                            <SelectItem key={status.value} value={status.value}>
                              {status.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>

                      {order.workStartedAt && (
                        <p className="mt-4 text-xs text-muted-foreground">
                          Розпочато: {new Date(order.workStartedAt).toLocaleString("uk-UA")}
                        </p>
                      )}
                      {order.workCompletedAt && (
                        <p className="mt-2 text-xs text-muted-foreground">
                          Завершено: {new Date(order.workCompletedAt).toLocaleString("uk-UA")}
                        </p>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
