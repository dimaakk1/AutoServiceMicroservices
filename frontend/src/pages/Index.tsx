import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Star, Clock, Shield, Wrench } from "lucide-react";
import heroImage from "../assets/photo.jpg";

const features = [
  { icon: Wrench, title: "Професійний ремонт", desc: "Сертифіковані майстри з досвідом" },
  { icon: Clock, title: "Швидкий запис", desc: "Онлайн бронювання за 2 хвилини" },
  { icon: Shield, title: "Гарантія якості", desc: "Гарантія на всі виконані роботи" },
  { icon: Star, title: "Високий рейтинг", desc: "500+ задоволених клієнтів" },
];

export default function Index() {
  return (
    <div className="bg-background">

      <section className="relative h-[70vh] min-h-[520px] flex items-center overflow-hidden">

        <img
          src={heroImage}
          alt="Автосервіс"
          className="absolute inset-0 w-full h-full object-cover"
        />

        <div className="absolute inset-0 bg-black/60" />

        <div className="container relative z-10 text-white">

          <h1 className="text-4xl md:text-6xl font-bold leading-tight max-w-2xl">
            Надійний автосервіс для вашого авто
          </h1>

          <p className="mt-5 text-lg text-white/80 max-w-xl">
            Діагностика, ремонт і обслуговування з гарантією якості та прозорими цінами.
          </p>

          <div className="mt-8 flex flex-wrap gap-3">
            <Link to="/booking">
              <Button
                size="lg"
                className="bg-accent hover:bg-accent/90 text-accent-foreground px-8"
              >
                Записатися
              </Button>
            </Link>

            <Link to="/services">
  <Button
    size="lg"
    variant="outline"
    className="border-white/40 bg-white/10 text-white hover:bg-white hover:text-black px-8"
  >
    Послуги
  </Button>
</Link>
          </div>

        </div>
      </section>

      <section className="py-20">
        <div className="container">

          <h2 className="text-3xl font-bold text-center mb-12">
            Чому обирають нас
          </h2>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">

            {features.map((f, i) => (
              <div
                key={i}
                className="bg-card border rounded-xl p-6 text-center shadow-sm hover:shadow-md transition"
              >
                <div className="mx-auto w-12 h-12 rounded-full bg-accent/10 flex items-center justify-center mb-4">
                  <f.icon className="h-6 w-6 text-accent" />
                </div>

                <h3 className="font-semibold text-lg mb-2">
                  {f.title}
                </h3>

                <p className="text-sm text-muted-foreground">
                  {f.desc}
                </p>
              </div>
            ))}

          </div>
        </div>
      </section>

      <section className="bg-primary text-primary-foreground py-16">
        <div className="container text-center">

          <h2 className="text-3xl font-bold mb-4">
            Потрібен ремонт авто?
          </h2>

          <p className="text-white/80 mb-8 max-w-md mx-auto">
            Запишіться зараз і отримайте швидке обслуговування без черг.
          </p>

          <Link to="/booking">
            <Button
              size="lg"
              className="bg-accent hover:bg-accent/90 text-accent-foreground px-10"
            >
              Записатися зараз
            </Button>
          </Link>

        </div>
      </section>

    </div>
  );
}