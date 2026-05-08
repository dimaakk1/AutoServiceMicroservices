import { useEffect, useState } from "react";
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

// ================= TYPES =================
type OrderItem = {
  orderItemId: number;
  productName: string;
  price: number;
  quantity: number;
  totalPrice: number;
};

type Order = {
  orderId: number;
  orderDate: string;
  status: string;
  items: OrderItem[];
};

// ================= PAGE =================
export default function AdminOrders() {
  const { user } = useAuth();

  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  const [filters, setFilters] = useState({
    userId: "",
    status: "",
    fromDate: "",
    toDate: "",
  });

  useEffect(() => {
    if (user?.role === "Admin") loadOrders();
  }, [user, filters]);

  const loadOrders = async () => {
    try {
      setLoading(true);

      const res = await api.get("/Orders/OrderItem/admin-with-items", {
        params: filters,
      });

      setOrders(res.data);
    } catch {
      toast.error("Не вдалося завантажити замовлення");
    } finally {
      setLoading(false);
    }
  };

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

  const statuses = [
    "Pending",
    "Confirmed",
    "InProgress",
    "Completed",
    "Cancelled",
  ];

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

  // ================= UI =================
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

      {/* ================= FILTER PANEL ================= */}
      <div className="bg-orange-50 border border-orange-200 rounded-xl p-4 mb-6 flex flex-wrap gap-3">

        <input
          className="border border-orange-200 p-2 rounded-lg focus:outline-orange-400"
          placeholder="User ID"
          value={filters.userId}
          onChange={(e) =>
            setFilters({ ...filters, userId: e.target.value })
          }
        />

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

        <input
          type="date"
          className="border border-orange-200 p-2 rounded-lg"
          value={filters.fromDate}
          onChange={(e) =>
            setFilters({ ...filters, fromDate: e.target.value })
          }
        />

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
              <TableHead className="text-white">Дата</TableHead>
              <TableHead className="text-white">Послуги</TableHead>
              <TableHead className="text-white">Сума</TableHead>
              <TableHead className="text-white">Статус</TableHead>
            </TableRow>
          </TableHeader>

          <TableBody>
            {orders.map((order) => {
              const total = order.items.reduce(
                (sum, i) => sum + i.totalPrice,
                0
              );

              return (
                <TableRow
                  key={order.orderId}
                  className="hover:bg-orange-50 transition"
                >
                  <TableCell className="font-bold text-orange-600">
                    #{order.orderId}
                  </TableCell>

                  <TableCell>
                    {new Date(order.orderDate).toLocaleString()}
                  </TableCell>

                  <TableCell>
                    {order.items.map((i) => (
                      <div key={i.orderItemId} className="text-sm">
                        <span className="font-medium text-orange-600">
                          {i.productName}
                        </span>{" "}
                        × {i.quantity}
                      </div>
                    ))}
                  </TableCell>

                  <TableCell className="font-semibold">
                    {total.toFixed(2)} ₴
                  </TableCell>

                  <TableCell>
                    <Select
                      value={order.status}
                      onValueChange={(v) =>
                        updateStatus(order.orderId, v)
                      }
                    >
                      <SelectTrigger className="w-[160px] border-orange-300">
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
              );
            })}
          </TableBody>

        </Table>
      </div>
    </div>
  );
}