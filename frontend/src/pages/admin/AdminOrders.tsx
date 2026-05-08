import { useEffect, useState, useMemo } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../lib/auth-context";

import { ArrowLeft } from "lucide-react";
import { Badge } from "../../components/ui/badge";
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

/* ================= TYPES ================= */

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
  items: OrderItem[];
  review: Review | null;
};

/* ================= PAGE ================= */

export default function AdminOrders() {
  const { user } = useAuth();

  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  const [filters, setFilters] = useState({
    status: "",
    fromDate: "",
    toDate: "",
  });

  /* ================= LOAD ================= */

  const loadOrders = async () => {
    try {
      setLoading(true);

      const res = await api.get("/aggregation/orders", {
        params: {
          status: filters.status || undefined,
          fromDate: filters.fromDate || undefined,
          toDate: filters.toDate || undefined,
        },
      });

      setOrders(res.data);
    } catch {
      toast.error("Не вдалося завантажити замовлення");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (user?.role === "Admin") loadOrders();
  }, [user, filters]);

  /* ================= STATUS UPDATE ================= */

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

  /* ================= HELPERS ================= */

  const totalPrice = (order: Order) =>
    order.items.reduce((sum, i) => sum + i.price * i.quantity, 0);

  const statuses = [
    "Pending",
    "Confirmed",
    "InProgress",
    "Completed",
    "Cancelled",
  ];

  /* ================= ACCESS ================= */

  if (!user || user.role !== "Admin") {
    return (
      <div className="container py-10 text-center">
        Доступ заборонено
      </div>
    );
  }

  if (loading) {
    return <div className="container py-10">Завантаження...</div>;
  }

  /* ================= UI ================= */

  return (
    <div className="container py-8">

      {/* HEADER */}
      <div className="flex items-center gap-4 mb-6">
        <Link to="/admin" className="text-orange-500 hover:text-orange-600">
          <ArrowLeft className="h-5 w-5" />
        </Link>

        <h1 className="text-2xl font-bold text-orange-500">
          Усі замовлення
        </h1>

        <Badge className="ml-auto bg-orange-500 text-white">
          {orders.length}
        </Badge>
      </div>

      {/* ================= FILTERS ================= */}
      <div className="bg-orange-50 border border-orange-200 rounded-xl p-4 mb-6 flex flex-wrap gap-3">

        {/* STATUS */}
        <select
          className="border border-orange-200 p-2 rounded-lg"
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

        {/* FROM DATE */}
        <input
          type="date"
          className="border border-orange-200 p-2 rounded-lg"
          value={filters.fromDate}
          onChange={(e) =>
            setFilters({ ...filters, fromDate: e.target.value })
          }
        />

        {/* TO DATE */}
        <input
          type="date"
          className="border border-orange-200 p-2 rounded-lg"
          value={filters.toDate}
          onChange={(e) =>
            setFilters({ ...filters, toDate: e.target.value })
          }
        />
      </div>

      {/* ================= TABLE ================= */}
      <div className="rounded-xl border border-orange-200 overflow-hidden shadow-sm">

        <Table>
          <TableHeader className="bg-orange-500">
            <TableRow>
              <TableHead className="text-white">ID</TableHead>
              <TableHead className="text-white">Користувач</TableHead>
              <TableHead className="text-white">Дата</TableHead>
              <TableHead className="text-white">Замовлення</TableHead>
              <TableHead className="text-white">Сума</TableHead>
              <TableHead className="text-white">Відгук</TableHead>
              <TableHead className="text-white">Статус</TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {orders.map((order) => (
              <TableRow key={order.orderId} className="hover:bg-orange-50">

                {/* ID */}
                <TableCell className="font-bold text-orange-600">
                  #{order.orderId}
                </TableCell>

                {/* USER */}
                <TableCell>
                  <div>
                    <div className="font-medium">
                      {order.username}
                    </div>
                    <div className="text-xs text-muted-foreground">
                      {order.email}
                    </div>
                  </div>
                </TableCell>

                {/* DATE */}
                <TableCell>
                  {new Date(order.orderDate).toLocaleString()}
                </TableCell>

                {/* ITEMS */}
                <TableCell>
                  {order.items.map((i, idx) => (
                    <div key={idx} className="text-sm">
                      {i.productName} × {i.quantity}
                    </div>
                  ))}
                </TableCell>

                {/* TOTAL */}
                <TableCell className="font-semibold">
                  {totalPrice(order).toFixed(2)} ₴
                </TableCell>

                {/* REVIEW */}
                <TableCell>
                  {order.review ? (
                    <div className="text-sm">
                      ⭐ {order.review.rating}
                      <div className="text-xs text-muted-foreground">
                        {order.review.comment}
                      </div>
                    </div>
                  ) : (
                    <span className="text-muted-foreground">
                      Немає
                    </span>
                  )}
                </TableCell>

                {/* STATUS */}
                <TableCell>
                  <Select
                    value={order.status}
                    onValueChange={(v) =>
                      updateStatus(order.orderId, v)
                    }
                  >
                    <SelectTrigger className="w-[150px]">
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