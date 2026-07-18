import { useNavigate } from "react-router-dom";
import { Button } from "../components/ui/button";
import { CheckCircle2, CalendarDays, ArrowRight } from "lucide-react";

export default function PaymentSuccess() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-muted/30">
      <div className="container py-16 max-w-3xl">
        <div className="bg-card border rounded-3xl shadow-sm p-10 text-center">
          <div className="w-24 h-24 rounded-full bg-green-100 flex items-center justify-center mx-auto mb-6">
            <CheckCircle2 className="h-14 w-14 text-green-600" />
          </div>

          <h1 className="text-4xl font-bold mb-4">
            Оплата успішна
          </h1>

          <p className="text-muted-foreground text-lg max-w-xl mx-auto mb-8">
            Дякуємо за оплату. Ваш платіж успішно оброблено,
            а замовлення підтверджено.
          </p>

          <div className="grid gap-4 md:grid-cols-2 mb-8">
            <div className="border rounded-2xl p-5 bg-muted/30">
              <h3 className="font-semibold mb-2">
                Статус замовлення
              </h3>

              <p className="text-green-600 font-bold">
                Підтверджено
              </p>
            </div>

            <div className="border rounded-2xl p-5 bg-muted/30">
              <h3 className="font-semibold mb-2">
                Статус платежу
              </h3>

              <p className="text-green-600 font-bold">
                Оплачено
              </p>
            </div>
          </div>

          <div className="flex flex-col sm:flex-row justify-center gap-4">
            <Button
              onClick={() => navigate("/my-bookings")}
              className="bg-orange-500 hover:bg-orange-600 text-white rounded-xl"
            >
              <CalendarDays className="h-4 w-4 mr-2" />
              Мої записи
            </Button>

            <Button
              variant="outline"
              onClick={() => navigate("/")}
              className="rounded-xl"
            >
              На головну
              <ArrowRight className="h-4 w-4 ml-2" />
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}