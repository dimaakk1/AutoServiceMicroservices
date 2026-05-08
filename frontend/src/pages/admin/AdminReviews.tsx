import { useEffect, useMemo, useState } from "react";
import api from "../../api/api";

import { Card, CardContent } from "../../components/ui/card";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";

import { Star, Trash2, MessageSquare, User } from "lucide-react";
import { toast } from "sonner";

/* ================= TYPES ================= */

type Review = {
  _id: string;
  orderId: number;
  rating: number;
  comment: string;
  createdAt: string;
};

type OrderReview = {
  orderId: number;
  username: string;
  email: string;
  status: string;
  orderDate: string;

  items: {
    productId: number;
    productName?: string; // якщо захочеш додати
    quantity: number;
    price: number;
  }[];

  review: Review;
};

/* ================= STAR ================= */

function StarRating({ value }: { value: number }) {
  return (
    <div className="flex gap-1">
      {[1, 2, 3, 4, 5].map((s) => (
        <Star
          key={s}
          className={`h-4 w-4 ${
            s <= value
              ? "fill-orange-500 text-orange-500"
              : "text-gray-300"
          }`}
        />
      ))}
    </div>
  );
}

/* ================= PAGE ================= */

export default function ReviewsAdmin() {
  const [reviews, setReviews] = useState<OrderReview[]>([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [ratingFilter, setRatingFilter] = useState<number | null>(null);

  /* ================= LOAD ================= */

  const load = async () => {
    try {
      setLoading(true);

      const res = await api.get(
        "/aggregation/orderswith-reviews"
      );

      setReviews(res.data);
    } catch (err) {
      toast.error("Не вдалося завантажити відгуки");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  /* ================= DELETE REVIEW ================= */

  const deleteReview = async (reviewId: string) => {
    try {
      await api.delete(`/Reviews/${reviewId}`);
      toast.success("Відгук видалено");
      load();
    } catch {
      toast.error("Помилка видалення");
    }
  };

  /* ================= FILTERED ================= */

  const filtered = useMemo(() => {
    return reviews.filter((r) => {
      const matchSearch =
        r.username?.toLowerCase().includes(search.toLowerCase()) ||
        r.review.comment.toLowerCase().includes(search.toLowerCase());

      const matchRating =
        ratingFilter ? r.review.rating === ratingFilter : true;

      return matchSearch && matchRating;
    });
  }, [reviews, search, ratingFilter]);

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
      <div className="container py-10 max-w-6xl">

        {/* HEADER */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-orange-600 mb-2">
            Адмін-панель відгуків
          </h1>

          <p className="text-muted-foreground">
            Керування всіма відгуками клієнтів
          </p>
        </div>

        {/* FILTERS */}
        <div className="flex flex-col md:flex-row gap-3 mb-6">

          <Input
            placeholder="Пошук по користувачу або коментарю..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="border-orange-200"
          />

          <div className="flex gap-2">
            {[1, 2, 3, 4, 5].map((r) => (
              <Button
                key={r}
                variant={ratingFilter === r ? "default" : "outline"}
                onClick={() =>
                  setRatingFilter(ratingFilter === r ? null : r)
                }
                className="border-orange-200"
              >
                {r}★
              </Button>
            ))}
          </div>

          <Button
            variant="outline"
            onClick={() => {
              setSearch("");
              setRatingFilter(null);
            }}
          >
            Скинути
          </Button>
        </div>

        {/* LIST */}
        <div className="space-y-4">
          {filtered.map((r) => (
            <Card
              key={r.orderId}
              className="border-orange-100 hover:shadow-md transition"
            >
              <CardContent className="p-5">

                {/* TOP */}
                <div className="flex justify-between mb-3">

                  <div className="flex items-center gap-3">

                    <div className="w-10 h-10 rounded-full bg-orange-100 flex items-center justify-center">
                      <User className="h-5 w-5 text-orange-600" />
                    </div>

                    <div>
                      <p className="font-semibold">
                        {r.username}
                      </p>

                      <p className="text-xs text-muted-foreground">
                        Замовлення #{r.orderId}
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center gap-3">
                    <StarRating value={r.review.rating} />

                    <Button
                      size="icon"
                      variant="destructive"
                      onClick={() => deleteReview(r.review._id)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  </div>
                </div>

                {/* SERVICE */}
                <div className="text-sm text-orange-600 font-medium mb-1">
                  Послуга:{" "}
                  {r.items?.[0]?.productName ?? "Невідома"}
                </div>

                {/* COMMENT */}
                <p className="text-muted-foreground text-sm">
                  {r.review.comment}
                </p>

                {/* DATE */}
                <p className="text-xs text-muted-foreground mt-2">
                  {new Date(
                    r.review.createdAt
                  ).toLocaleString("uk-UA")}
                </p>
              </CardContent>
            </Card>
          ))}

          {filtered.length === 0 && (
            <Card className="border-dashed border-orange-200">
              <CardContent className="py-12 text-center">
                <MessageSquare className="mx-auto mb-3 text-orange-300" />
                <p className="text-muted-foreground">
                  Відгуків не знайдено
                </p>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}