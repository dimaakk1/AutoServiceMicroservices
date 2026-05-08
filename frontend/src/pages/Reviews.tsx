import { useEffect, useState } from "react";
import { useAuth } from "../lib/auth-context";
import { Button } from "../components/ui/button";
import { Textarea } from "../components/ui/textarea";
import { Label } from "../components/ui/label";
import { Star } from "lucide-react";
import { toast } from "sonner";
import { createReview, getReviews } from "../api/reviews";
import { getOrdersWithItems } from "../api/order";

function StarRating({
  value,
  onChange,
}: {
  value: number;
  onChange?: (v: number) => void;
}) {
  return (
    <div className="flex gap-1">
      {[1, 2, 3, 4, 5].map((s) => (
        <button key={s} type="button" onClick={() => onChange?.(s)}>
          <Star
            className={`h-5 w-5 ${
              s <= value
                ? "fill-accent text-accent"
                : "text-muted-foreground/30"
            }`}
          />
        </button>
      ))}
    </div>
  );
}

export default function Reviews() {
  const { user } = useAuth();

  const [reviews, setReviews] = useState<any[]>([]);
  const [orders, setOrders] = useState<any[]>([]);

  const [selectedOrderId, setSelectedOrderId] = useState<number | null>(
    null
  );
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState("");

  // 🔥 LOAD REVIEWS
  useEffect(() => {
    getReviews()
      .then((r) => setReviews(r.data))
      .catch(() => toast.error("Не вдалося завантажити відгуки"));
  }, []);

  // 🔥 LOAD ORDERS (тільки completed / confirmed)
  useEffect(() => {
    if (!user) return;

    getOrdersWithItems()
      .then((r) => {
        const filtered = r.data.filter(
          (o: any) => o.status === "Confirmed"
        );
        setOrders(filtered);
      })
      .catch(() => toast.error("Не вдалося завантажити замовлення"));
  }, [user]);

  // ⭐ CREATE REVIEW
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!user) return;

    if (!selectedOrderId) {
      toast.error("Оберіть замовлення");
      return;
    }

    try {
      const res = await createReview({
        orderId: selectedOrderId,
        rating,
        comment,
      });

      setReviews((prev) => [res.data, ...prev]);

      setComment("");
      setRating(5);
      setSelectedOrderId(null);

      toast.success("Відгук додано");
    } catch (err) {
      console.error(err);
      toast.error("Помилка створення відгуку");
    }
  };

  return (
    <div className="container py-12 max-w-2xl">
      <h1 className="text-3xl font-bold mb-6">Відгуки</h1>

      {/* FORM */}
      {user ? (
        <form
          onSubmit={handleSubmit}
          className="border bg-card p-6 rounded-lg mb-10"
        >
          <h2 className="font-semibold mb-4">Залишити відгук</h2>

          {/* ORDER SELECT */}
          <div className="mb-4">
            <Label>Замовлення</Label>

            <select
              className="w-full border rounded p-2 mt-1"
              value={selectedOrderId ?? ""}
              onChange={(e) =>
                setSelectedOrderId(Number(e.target.value))
              }
            >
              <option value="">Оберіть замовлення</option>

              {orders.map((o) => (
                <option key={o.orderId} value={o.orderId}>
                  #{o.orderId} — {o.orderDate.split("T")[0]}
                </option>
              ))}
            </select>
          </div>

          {/* RATING */}
          <div className="mb-4">
            <Label>Оцінка</Label>
            <StarRating value={rating} onChange={setRating} />
          </div>

          {/* COMMENT */}
          <div className="mb-4">
            <Label>Коментар</Label>
            <Textarea
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              rows={3}
            />
          </div>

          <Button type="submit" className="bg-accent">
            Надіслати
          </Button>
        </form>
      ) : (
        <p>Увійдіть, щоб залишити відгук</p>
      )}

      {/* LIST */}
      <div className="space-y-3">
        {reviews.map((r) => (
          <div key={r.id} className="border p-4 rounded-lg bg-card">
            <div className="flex justify-between">
              <p className="text-sm">Order #{r.orderId}</p>
              <StarRating value={r.rating} />
            </div>
            <p className="mt-2 text-sm">{r.comment}</p>
          </div>
        ))}
      </div>
    </div>
  );
}