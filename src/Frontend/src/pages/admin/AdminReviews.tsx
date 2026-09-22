import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import api from "../../api/api";
import { Badge } from "../../components/ui/badge";
import { Card, CardContent } from "../../components/ui/card";
import { Button } from "../../components/ui/button";
import { Input } from "../../components/ui/input";

import { Star, Trash2, MessageSquare, User, ArrowLeft } from "lucide-react";
import { toast } from "sonner";


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
    productName?: string;
    quantity: number;
    price: number;
  }[];

  review: Review;
};


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


export default function ReviewsAdmin() {
  const [reviews, setReviews] = useState<OrderReview[]>([]);
  const [loading, setLoading] = useState(true);

  const [search, setSearch] = useState("");
  const [ratingFilter, setRatingFilter] = useState<number | null>(null);


  const load = async () => {
    try {
      setLoading(true);
      const res = await api.get("/aggregation/orderswith-reviews");
      setReviews(res.data);
    } catch {
      toast.error("Не вдалося завантажити відгуки");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);


  const deleteReview = async (id: string) => {
    try {
      await api.delete(`/Reviews/${id}`);
      toast.success("Відгук видалено");
      load();
    } catch {
      toast.error("Помилка видалення");
    }
  };


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


  if (loading) {
    return (
      <div className="container py-20 text-center text-muted-foreground">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="container py-8 max-w-5xl">

      <div className="flex items-center gap-4 mb-6">

        <Link
          to="/admin"
          className="text-muted-foreground hover:text-orange-500 transition"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>

        <h1 className="text-3xl font-bold">
          Відгуки
        </h1>

       <Badge className="ml-auto bg-orange-500 text-white">
          {filtered.length}
        </Badge>

      </div>

      <div className="flex flex-col md:flex-row gap-3 mb-6">

        <Input
          placeholder="Пошук по користувачу або коментарю..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />

        <div className="flex gap-2 flex-wrap">
          {[1, 2, 3, 4, 5].map((r) => (
            <Button
              key={r}
              variant={ratingFilter === r ? "default" : "outline"}
              onClick={() =>
                setRatingFilter(ratingFilter === r ? null : r)
              }
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

      <div className="space-y-4">

        {filtered.map((r) => (
          <Card key={r.orderId} className="hover:shadow-md transition">

            <CardContent className="p-5">

              <div className="flex justify-between mb-3">

                <div className="flex items-center gap-3">

                  <div className="w-10 h-10 rounded-full bg-muted flex items-center justify-center">
                    <User className="h-5 w-5 text-orange-500" />
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

              <p className="text-sm text-muted-foreground mb-1">
                {r.items?.[0]?.productName || "Послуга"}
              </p>

              <p className="text-sm">
                {r.review.comment}
              </p>

              <p className="text-xs text-muted-foreground mt-2">
                {new Date(r.review.createdAt).toLocaleString("uk-UA")}
              </p>

            </CardContent>

          </Card>
        ))}

        {filtered.length === 0 && (
          <Card>
            <CardContent className="py-10 text-center text-muted-foreground">
              <MessageSquare className="mx-auto mb-2" />
              Відгуків не знайдено
            </CardContent>
          </Card>
        )}

      </div>
    </div>
  );
}