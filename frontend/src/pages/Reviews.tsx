import { useEffect, useMemo, useState } from "react";
import { useAuth } from "../lib/auth-context";

import { Button } from "../components/ui/button";
import { Textarea } from "../components/ui/textarea";
import { Label } from "../components/ui/label";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "../components/ui/select";

import { Card, CardContent } from "../components/ui/card";

import { Star, MessageSquare, User } from "lucide-react";
import { toast } from "sonner";
import api from "../api/api";

/* ================= TYPES ================= */

type Review = {
  _id: string;
  orderId: number;
  rating: number;
  comment: string;
  createdAt: string;
};

type Order = {
  orderId: number;
  userId?: string;
  username?: string;
  email?: string;

  status: string;
  orderDate: string;

  items: {
    productId: number;
    productName: string;
    quantity: number;
    price: number;
  }[];

  review?: Review | null;
};

/* ================= STAR ================= */

function StarRating({
  value,
  onChange,
}: {
  value: number;
  onChange?: (v: number) => void;
}) {
  return (
    <div className="flex gap-1">
      {[1, 2, 3, 4, 5].map((star) => (
        <button
          key={star}
          type="button"
          onClick={() => onChange?.(star)}
          className="transition hover:scale-110"
        >
          <Star
            className={`h-6 w-6 transition ${
              star <= value
                ? "fill-orange-500 text-orange-500"
                : "text-gray-300"
            }`}
          />
        </button>
      ))}
    </div>
  );
}

/* ================= PAGE ================= */

export default function Reviews() {
  const { user } = useAuth();

  const [orders, setOrders] = useState<Order[]>([]);
  const [reviewOrders, setReviewOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);

  const [mode, setMode] = useState<"all" | "mine">("all");

  const [selectedOrderId, setSelectedOrderId] = useState("");
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");

  /* 🔥 EDIT STATE */
  const [editReview, setEditReview] = useState<Review | null>(null);

  /* ================= LOAD ================= */

  useEffect(() => {
    loadReviews();
    if (user) loadOrders();
  }, [user]);

  /* ================= ALL REVIEWS ================= */

  const loadReviews = async () => {
    try {
      setLoading(true);

      const res = await api.get("/aggregation/orderswith-reviews");

      setReviewOrders(res.data);
    } catch {
      toast.error("Не вдалося завантажити відгуки");
    } finally {
      setLoading(false);
    }
  };

  /* ================= MY ORDERS ================= */

  const loadMyReviews = async () => {
    try {
      setLoading(true);

      const res = await api.get("/aggregation/my-orders");

      setReviewOrders(res.data);
    } catch {
      toast.error("Не вдалося завантажити ваші відгуки");
    } finally {
      setLoading(false);
    }
  };

  const loadOrders = async () => {
    try {
      const res = await api.get("/aggregation/my-orders");
      setOrders(res.data);
    } catch {
      toast.error("Не вдалося завантажити замовлення");
    }
  };

  /* ================= CREATE REVIEW ================= */

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!selectedOrderId) {
      toast.error("Оберіть замовлення");
      return;
    }

    if (!comment.trim()) {
      toast.error("Напишіть коментар");
      return;
    }

    try {
      await api.post("/Reviews", {
        orderId: Number(selectedOrderId),
        rating,
        comment,
      });

      toast.success("Відгук додано");

      setComment("");
      setRating(5);
      setSelectedOrderId("");

      await loadOrders();
      await loadReviews();
    } catch {
      toast.error("Помилка створення відгуку");
    }
  };

  /* ================= DELETE REVIEW ================= */

  const handleDelete = async (id: string) => {
    try {
      await api.delete(`/Reviews/${id}`);

      toast.success("Відгук видалено");

      await loadReviews();
      await loadOrders();
    } catch {
      toast.error("Помилка видалення");
    }
  };

  /* ================= UPDATE REVIEW ================= */

  const handleUpdate = async () => {
  if (!editReview) return;

  try {
    await api.put(`/Reviews/${editReview._id}`, {
      id: editReview._id,
      rating: editReview.rating,
      comment: editReview.comment,
    });

    toast.success("Відгук оновлено");

    setEditReview(null);

    await loadReviews();
    await loadOrders();
  } catch (err) {
    console.error(err);
    toast.error("Помилка оновлення");
  }
};

  /* ================= DATA ================= */

  const reviews = useMemo(() => {
    return reviewOrders.filter((o) => o.review);
  }, [reviewOrders]);

  const avgRating = reviews.length
    ? (
        reviews.reduce((s, r) => s + (r.review?.rating || 0), 0) /
        reviews.length
      ).toFixed(1)
    : "0";

  const availableOrders = orders.filter(
    (o) => !o.review && o.status === "Completed"
  );

  const getServiceName = (order: Order) => {
    if (!order.items?.length) return "Послуга";
    return order.items.map((i) => i.productName).join(", ");
  };

  /* ================= UI ================= */

  if (loading) {
    return (
      <div className="container py-20 text-center text-orange-500">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-orange-50/30">
      <div className="container py-12 max-w-5xl">

        {/* HEADER */}
        <div className="mb-6">
          <h1 className="text-4xl font-bold text-orange-600 mb-3">
            Відгуки клієнтів
          </h1>

          <div className="flex items-center gap-3 text-muted-foreground">
            <Star className="h-5 w-5 fill-orange-500 text-orange-500" />

            <span className="font-semibold text-foreground">
              {avgRating}
            </span>

            <span>({reviews.length} відгуків)</span>
          </div>
        </div>

        {/* SWITCH */}
        <div className="flex gap-2 mb-8">
          <Button
            variant={mode === "all" ? "default" : "outline"}
            onClick={() => {
              setMode("all");
              loadReviews();
            }}
          >
            Всі відгуки
          </Button>

          <Button
            variant={mode === "mine" ? "default" : "outline"}
            onClick={() => {
              setMode("mine");
              loadMyReviews();
            }}
          >
            Мої відгуки
          </Button>
        </div>

        {/* FORM */}
        {user && (
          <Card className="border-orange-200 shadow-md mb-10">
            <CardContent className="p-6">

              <div className="flex items-center gap-2 mb-6">
                <MessageSquare className="text-orange-500" />
                <h2 className="text-xl font-semibold">
                  Залишити відгук
                </h2>
              </div>

              {availableOrders.length === 0 ? (
                <div className="text-muted-foreground">
                  Немає замовлень для відгуку
                </div>
              ) : (
                <form onSubmit={handleSubmit} className="space-y-5">

                  <div>
                    <Label>Замовлення</Label>

                    <Select
                      value={selectedOrderId}
                      onValueChange={setSelectedOrderId}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Оберіть" />
                      </SelectTrigger>

                      <SelectContent>
                        {availableOrders.map((o) => (
                          <SelectItem
                            key={o.orderId}
                            value={String(o.orderId)}
                          >
                            {getServiceName(o)} •{" "}
                            {new Date(o.orderDate).toLocaleDateString("uk-UA")}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div>
                    <Label>Оцінка</Label>
                    <StarRating value={rating} onChange={setRating} />
                  </div>

                  <div>
                    <Label>Коментар</Label>
                    <Textarea
                      value={comment}
                      onChange={(e) => setComment(e.target.value)}
                    />
                  </div>

                  <Button className="bg-orange-500 hover:bg-orange-600">
                    Надіслати
                  </Button>
                </form>
              )}
            </CardContent>
          </Card>
        )}

        {/* LIST */}
        <div className="space-y-5">
          {reviews.map((order) => (
            <Card key={order.orderId}>
              <CardContent className="p-6">

                <div className="flex justify-between mb-3">
                  <div className="flex items-center gap-3">

                    <div className="w-10 h-10 rounded-full bg-orange-100 flex items-center justify-center">
                      <User className="text-orange-600" />
                    </div>

                    <div>
                      <p className="font-semibold">
                        {order.username || "Користувач"}
                      </p>

                      <p className="text-sm text-muted-foreground">
                        {getServiceName(order)}
                      </p>

                      <p className="text-xs text-muted-foreground">
                        {new Date(order.review!.createdAt).toLocaleDateString("uk-UA")}
                      </p>

                      {/* 🔥 ACTIONS ONLY FOR MINE */}
                      {mode === "mine" && (
                        <div className="flex gap-2 mt-2">
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() =>
                              setEditReview(order.review!)
                            }
                          >
                            Редагувати
                          </Button>

                          <Button
                            size="sm"
                            variant="destructive"
                            onClick={() =>
                              handleDelete(order.review!._id)
                            }
                          >
                            Видалити
                          </Button>
                        </div>
                      )}
                    </div>
                  </div>

                  <StarRating value={order.review!.rating} />
                </div>

                <p className="text-muted-foreground">
                  {order.review!.comment}
                </p>

              </CardContent>
            </Card>
          ))}

          {reviews.length === 0 && (
            <Card>
              <CardContent className="py-12 text-center">
                Відгуків немає
              </CardContent>
            </Card>
          )}
        </div>

        {/* EDIT MODAL */}
        {editReview && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
            <div className="bg-white p-6 rounded-xl w-[420px] space-y-4">

              <h2 className="text-lg font-semibold">
                Редагування відгуку
              </h2>

              <StarRating
                value={editReview.rating}
                onChange={(v) =>
                  setEditReview({ ...editReview, rating: v })
                }
              />

              <Textarea
                value={editReview.comment}
                onChange={(e) =>
                  setEditReview({
                    ...editReview,
                    comment: e.target.value,
                  })
                }
              />

              <div className="flex justify-end gap-2">
                <Button
                  variant="outline"
                  onClick={() => setEditReview(null)}
                >
                  Скасувати
                </Button>

                <Button
                  className="bg-orange-500 hover:bg-orange-600"
                  onClick={handleUpdate}
                >
                  Зберегти
                </Button>
              </div>

            </div>
          </div>
        )}

      </div>
    </div>
  );
}