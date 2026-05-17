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

import {
  Card,
  CardContent,
} from "../components/ui/card";

import {
  Star,
  MessageSquare,
  User,
  Pencil,
  Trash2,
} from "lucide-react";

import { toast } from "sonner";
import api from "../api/api";


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


function StarRating({
  value,
  onChange,
  size = "default",
}: {
  value: number;
  onChange?: (v: number) => void;
  size?: "default" | "small";
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
            className={`transition ${
              size === "small"
                ? "h-4 w-4"
                : "h-6 w-6"
            } ${
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


export default function Reviews() {
  const { user } = useAuth();

  const [orders, setOrders] = useState<Order[]>([]);
  const [reviewOrders, setReviewOrders] = useState<Order[]>([]);

  const [loading, setLoading] = useState(true);

  const [mode, setMode] = useState<"all" | "mine">("all");

  const [selectedOrderId, setSelectedOrderId] = useState("");
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");

  const [editReview, setEditReview] =
    useState<Review | null>(null);


  useEffect(() => {
    loadReviews();

    if (user) {
      loadOrders();
    }
  }, [user]);


  const loadReviews = async () => {
    try {
      setLoading(true);

      const res = await api.get(
        "/aggregation/orderswith-reviews"
      );

      setReviewOrders(res.data);
    } catch {
      toast.error("Не вдалося завантажити відгуки");
    } finally {
      setLoading(false);
    }
  };


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


  const handleSubmit = async (
    e: React.FormEvent
  ) => {
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


  const reviews = useMemo(() => {
    return reviewOrders.filter((o) => o.review);
  }, [reviewOrders]);

  const avgRating = reviews.length
    ? (
        reviews.reduce(
          (s, r) => s + (r.review?.rating || 0),
          0
        ) / reviews.length
      ).toFixed(1)
    : "0";

  const availableOrders = orders.filter(
    (o) =>
      !o.review &&
      o.status === "Completed"
  );

  const getServiceName = (order: Order) => {
    if (!order.items?.length) {
      return "Послуга";
    }

    return order.items
      .map((i) => i.productName)
      .join(", ");
  };


  if (loading) {
    return (
      <div className="container py-20 text-center text-muted-foreground">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-muted/20">

      <div className="container py-12 max-w-6xl">

        <div className="mb-10">

          <h1 className="text-4xl font-bold mb-3">
            Відгуки клієнтів
          </h1>

          <div className="flex items-center gap-3 text-muted-foreground">

            <div className="flex items-center gap-1">
              <Star className="h-5 w-5 fill-orange-500 text-orange-500" />

              <span className="font-semibold text-foreground">
                {avgRating}
              </span>
            </div>

            <span>
              ({reviews.length} відгуків)
            </span>

          </div>

        </div>

        <div className="flex gap-3 mb-8">

          <Button
            onClick={() => {
              setMode("all");
              loadReviews();
            }}
            className={
              mode === "all"
                ? "bg-orange-500 hover:bg-orange-600 text-white"
                : ""
            }
            variant={
              mode === "all"
                ? "default"
                : "outline"
            }
          >
            Всі відгуки
          </Button>

          <Button
            onClick={() => {
              setMode("mine");
              loadMyReviews();
            }}
            className={
              mode === "mine"
                ? "bg-orange-500 hover:bg-orange-600 text-white"
                : ""
            }
            variant={
              mode === "mine"
                ? "default"
                : "outline"
            }
          >
            Мої відгуки
          </Button>

        </div>

        {user && (
          <Card className="mb-10 shadow-sm border-0">

            <CardContent className="p-6">

              <div className="flex items-center gap-2 mb-6">

                <div className="w-10 h-10 rounded-xl bg-orange-100 flex items-center justify-center">
                  <MessageSquare className="h-5 w-5 text-orange-500" />
                </div>

                <div>
                  <h2 className="text-xl font-semibold">
                    Залишити відгук
                  </h2>

                  <p className="text-sm text-muted-foreground">
                    Поділіться враженням про сервіс
                  </p>
                </div>

              </div>

              {availableOrders.length === 0 ? (
                <div className="text-muted-foreground">
                  Немає замовлень для відгуку
                </div>
              ) : (
                <form
                  onSubmit={handleSubmit}
                  className="space-y-5"
                >

                  <div className="space-y-2">

                    <Label>
                      Замовлення
                    </Label>

                    <Select
                      value={selectedOrderId}
                      onValueChange={
                        setSelectedOrderId
                      }
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Оберіть замовлення" />
                      </SelectTrigger>

                      <SelectContent>
                        {availableOrders.map((o) => (
                          <SelectItem
                            key={o.orderId}
                            value={String(o.orderId)}
                          >
                            {getServiceName(o)} •{" "}
                            {new Date(
                              o.orderDate
                            ).toLocaleDateString(
                              "uk-UA"
                            )}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>

                  </div>

                  <div className="space-y-2">

                    <Label>
                      Оцінка
                    </Label>

                    <StarRating
                      value={rating}
                      onChange={setRating}
                    />

                  </div>

                  <div className="space-y-2">

                    <Label>
                      Коментар
                    </Label>

                    <Textarea
                      rows={5}
                      placeholder="Напишіть ваш відгук..."
                      value={comment}
                      onChange={(e) =>
                        setComment(
                          e.target.value
                        )
                      }
                    />

                  </div>

                  <Button className="bg-orange-500 hover:bg-orange-600 text-white">
                    Надіслати відгук
                  </Button>

                </form>
              )}

            </CardContent>

          </Card>
        )}

        <div className="space-y-5">

          {reviews.map((order) => (
            <Card
              key={order.orderId}
              className="shadow-sm border-0 hover:shadow-md transition"
            >

              <CardContent className="p-6">

                <div className="flex flex-col md:flex-row md:items-start md:justify-between gap-5">

                  <div className="flex gap-4">

                    <div className="w-12 h-12 rounded-xl bg-orange-100 flex items-center justify-center shrink-0">
                      <User className="h-6 w-6 text-orange-500" />
                    </div>

                    <div>

                      <div className="flex items-center gap-3 mb-1">

                        <h3 className="font-semibold text-lg">
                          {order.username ||
                            "Користувач"}
                        </h3>

                        <StarRating
                          value={
                            order.review!.rating
                          }
                          size="small"
                        />

                      </div>

                      <p className="text-sm text-muted-foreground mb-1">
                        {getServiceName(order)}
                      </p>

                      <p className="text-xs text-muted-foreground mb-4">
                        {new Date(
                          order.review!.createdAt
                        ).toLocaleDateString(
                          "uk-UA"
                        )}
                      </p>

                      <p className="leading-relaxed text-[15px]">
                        {
                          order.review!.comment
                        }
                      </p>

                      {mode === "mine" && (
                        <div className="flex gap-2 mt-5">

                          <Button
                            size="sm"
                            variant="outline"
                            className="border-orange-200 hover:bg-orange-50"
                            onClick={() =>
                              setEditReview(
                                order.review!
                              )
                            }
                          >
                            <Pencil className="h-4 w-4 mr-2" />
                            Редагувати
                          </Button>

                          <Button
                            size="sm"
                            variant="destructive"
                            onClick={() =>
                              handleDelete(
                                order.review!._id
                              )
                            }
                          >
                            <Trash2 className="h-4 w-4 mr-2" />
                            Видалити
                          </Button>

                        </div>
                      )}

                    </div>

                  </div>

                </div>

              </CardContent>

            </Card>
          ))}

          {reviews.length === 0 && (
            <Card className="border-dashed">

              <CardContent className="py-14 text-center text-muted-foreground">
                Відгуків немає
              </CardContent>

            </Card>
          )}

        </div>

        {editReview && (
          <div className="fixed inset-0 bg-black/50 backdrop-blur-sm flex items-center justify-center z-50 p-4">

            <div className="bg-background rounded-2xl shadow-2xl w-full max-w-md p-6 space-y-5">

              <div>
                <h2 className="text-2xl font-bold mb-1">
                  Редагування відгуку
                </h2>

                <p className="text-sm text-muted-foreground">
                  Оновіть оцінку або текст відгуку
                </p>
              </div>

              <div className="space-y-2">

                <Label>
                  Оцінка
                </Label>

                <StarRating
                  value={editReview.rating}
                  onChange={(v) =>
                    setEditReview({
                      ...editReview,
                      rating: v,
                    })
                  }
                />

              </div>

              <div className="space-y-2">

                <Label>
                  Коментар
                </Label>

                <Textarea
                  rows={5}
                  value={editReview.comment}
                  onChange={(e) =>
                    setEditReview({
                      ...editReview,
                      comment:
                        e.target.value,
                    })
                  }
                />

              </div>

              <div className="flex justify-end gap-3 pt-2">

                <Button
                  variant="outline"
                  onClick={() =>
                    setEditReview(null)
                  }
                >
                  Скасувати
                </Button>

                <Button
                  className="bg-orange-500 hover:bg-orange-600 text-white"
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