import { useEffect, useMemo, useState } from "react";
import api from "../../api/api";
import { Card, CardContent } from "../../components/ui/card";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";
import { toast } from "sonner";
import {
  Ban,
  CheckCircle,
  RefreshCcw,
  User,
  Search,
  X,
} from "lucide-react";

/* ================= TYPES ================= */

type UserDto = {
  userId: string;
  username: string;
  email: string;
  isBlocked?: boolean;
};

type Order = {
  orderId: number;
  userId: string;
  username: string;
  email: string;
  orderDate: string;
  status: string;
  items: {
    productName: string;
  }[];
  review: {
    rating: number;
    comment: string;
  } | null;
};

/* ================= PAGE ================= */

export default function UsersAdmin() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");

  const [selectedUser, setSelectedUser] = useState<UserDto | null>(null);
  const [userOrders, setUserOrders] = useState<Order[]>([]);
  const [loadingDetails, setLoadingDetails] = useState(false);

  /* ================= LOAD ================= */

  const loadUsers = async () => {
    try {
      setLoading(true);
      const res = await api.get("/users");
      setUsers(res.data);
    } catch {
      toast.error("Не вдалося завантажити користувачів");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadUsers();
  }, []);

  /* ================= FILTER ================= */

  const filteredUsers = useMemo(() => {
    return users.filter((u) =>
      u.username.toLowerCase().includes(search.toLowerCase())
    );
  }, [users, search]);

  /* ================= ACTIONS ================= */

  const blockUser = async (id: string) => {
    try {
      await api.post(`/users/block/${id}`);
      toast.success("Заблоковано");
      loadUsers();
    } catch {
      toast.error("Помилка");
    }
  };

  const unblockUser = async (id: string) => {
    try {
      await api.post(`/users/unblock/${id}`);
      toast.success("Розблоковано");
      loadUsers();
    } catch {
      toast.error("Помилка");
    }
  };

  /* ================= DETAILS ================= */

  const loadUserDetails = async (user: UserDto) => {
    try {
      setSelectedUser(user);
      setLoadingDetails(true);

      const res = await api.get("/aggregation/orders", {
        params: { userId: user.userId },
      });

      setUserOrders(res.data);
    } catch {
      toast.error("Помилка завантаження");
    } finally {
      setLoadingDetails(false);
    }
  };

  /* ================= UI ================= */

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center text-orange-500">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-orange-50 to-white">
      <div className="container py-10 max-w-6xl">

        {/* HEADER */}
        <div className="flex justify-between items-center mb-6">
          <div>
            <h1 className="text-4xl font-bold text-orange-600">
              Користувачі
            </h1>
            <p className="text-sm text-muted-foreground">
              Адмін панель керування
            </p>
          </div>

          <Button onClick={loadUsers} variant="outline">
            <RefreshCcw className="h-4 w-4 mr-2" />
            Оновити
          </Button>
        </div>

        {/* SEARCH */}
        <div className="relative mb-8">
          <Search className="absolute left-3 top-3 text-gray-400 h-4 w-4" />

          <Input
            placeholder="Пошук користувача..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-10 shadow-sm"
          />
        </div>

        {/* USERS GRID */}
        <div className="grid md:grid-cols-2 gap-4">

          {filteredUsers.map((u) => (
            <Card
              key={u.userId}
              className={`shadow-md hover:shadow-lg transition border ${
                u.isBlocked ? "border-red-200" : "border-orange-100"
              }`}
            >
              <CardContent className="p-5">

                {/* TOP */}
                <div className="flex justify-between items-start">

                  <div className="flex items-center gap-3">

                    <div className="w-10 h-10 rounded-full bg-orange-100 flex items-center justify-center">
                      <User className="text-orange-600" />
                    </div>

                    <div>
                      <div className="flex items-center gap-2">
                        <p className="font-semibold">
                          {u.username}
                        </p>

                        {u.isBlocked && (
                          <span className="text-xs bg-red-500 text-white px-2 py-0.5 rounded">
                            BLOCKED
                          </span>
                        )}
                      </div>

                      <p className="text-sm text-muted-foreground">
                        {u.email}
                      </p>
                    </div>

                  </div>

                </div>

                {/* ACTIONS */}
                <div className="flex gap-2 mt-4">

                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => loadUserDetails(u)}
                  >
                    Деталі
                  </Button>

                  {u.isBlocked ? (
                    <Button
                      size="sm"
                      variant="outline"
                      className="text-green-600"
                      onClick={() => unblockUser(u.userId)}
                    >
                      <CheckCircle className="h-4 w-4 mr-1" />
                      Unblock
                    </Button>
                  ) : (
                    <Button
                      size="sm"
                      variant="outline"
                      onClick={() => blockUser(u.userId)}
                    >
                      <Ban className="h-4 w-4 mr-1" />
                      Block
                    </Button>
                  )}

                </div>

              </CardContent>
            </Card>
          ))}

        </div>

        {/* EMPTY */}
        {filteredUsers.length === 0 && (
          <div className="text-center mt-16 text-muted-foreground">
            Користувачів не знайдено
          </div>
        )}

        {/* ================= MODAL ================= */}

        {selectedUser && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">

            <div className="bg-white w-full max-w-3xl rounded-2xl shadow-2xl overflow-hidden">

              {/* HEADER */}
              <div className="flex justify-between items-center p-5 border-b">
                <div>
                  <h2 className="text-xl font-bold">
                    {selectedUser.username}
                  </h2>
                  <p className="text-sm text-muted-foreground">
                    {selectedUser.email}
                  </p>
                </div>

                <Button
                  variant="ghost"
                  onClick={() => setSelectedUser(null)}
                >
                  <X className="h-5 w-5" />
                </Button>
              </div>

              {/* CONTENT */}
              <div className="p-5 space-y-4">

                {/* STATS */}
                <div className="grid grid-cols-3 gap-3">

                  <div className="bg-orange-50 rounded-xl p-3">
                    <p className="text-sm">Замовлення</p>
                    <p className="text-xl font-bold">
                      {userOrders.length}
                    </p>
                  </div>

                  <div className="bg-orange-50 rounded-xl p-3">
                    <p className="text-sm">Відгуки</p>
                    <p className="text-xl font-bold">
                      {userOrders.filter(o => o.review).length}
                    </p>
                  </div>

                  <div className="bg-orange-50 rounded-xl p-3">
                    <p className="text-sm">Рейтинг</p>
                    <p className="text-xl font-bold">
                      {(
                        userOrders
                          .filter(o => o.review)
                          .reduce((s, o) => s + (o.review?.rating || 0), 0) /
                        (userOrders.filter(o => o.review).length || 1)
                      ).toFixed(1)}
                    </p>
                  </div>

                </div>

                {/* ORDERS */}
                <div>
                  <h3 className="font-semibold mb-3">
                    Замовлення
                  </h3>

                  {loadingDetails ? (
                    <p>Завантаження...</p>
                  ) : (
                    <div className="space-y-2 max-h-[300px] overflow-auto pr-2">

                      {userOrders.map((o) => (
                        <div
                          key={o.orderId}
                          className="border rounded-lg p-3 text-sm"
                        >

                          <div className="flex justify-between">
                            <p className="font-semibold">
                              #{o.orderId}
                            </p>
                            <span className="text-xs text-muted-foreground">
                              {o.status}
                            </span>
                          </div>

                          <p className="text-muted-foreground text-xs">
                            {new Date(o.orderDate).toLocaleDateString("uk-UA")}
                          </p>

                          <p>
                            {o.items.map(i => i.productName).join(", ")}
                          </p>

                          {o.review && (
                            <p className="text-orange-600 mt-1">
                              ⭐ {o.review.rating} — {o.review.comment}
                            </p>
                          )}

                        </div>
                      ))}

                    </div>
                  )}

                </div>

              </div>

            </div>

          </div>
        )}

      </div>
    </div>
  );
}