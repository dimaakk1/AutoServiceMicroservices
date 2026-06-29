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

      {/* HERO (Lovable style) */}
      <section className="relative overflow-hidden min-h-[85vh] flex items-center">

        {/* Background image */}
        <img
          src={heroImage}
          className="absolute inset-0 w-full h-full object-cover scale-105"
          alt="Auto service"
        />

        {/* overlays */}
        <div className="absolute inset-0 bg-gradient-to-r from-black/80 via-black/60 to-black/30" />
        <div className="absolute inset-0 bg-grid-dark opacity-20" />

        <div className="container relative z-10 grid lg:grid-cols-2 gap-12 items-center text-white">

          {/* LEFT */}
          <div>
            <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-white/10 border border-white/20 text-xs mb-6">
              <span className="h-2 w-2 rounded-full bg-accent animate-pulse" />
              Автосервіс нового покоління
            </div>

            <h1 className="text-4xl md:text-6xl font-bold leading-tight">
              Надійний сервіс для вашого авто
              <span className="block text-accent mt-2">
                швидко, чесно, якісно
              </span>
            </h1>

            <p className="mt-6 text-white/70 max-w-xl text-lg">
              Діагностика, ремонт і обслуговування з прозорими цінами та гарантією.
            </p>

            <div className="mt-8 flex flex-wrap gap-4">
              <Link to="/booking">
                <Button className="bg-accent hover:bg-accent/90 text-black px-8 h-12">
                  Записатися
                </Button>
              </Link>

              <Link to="/services">
                <Button
                  variant="outline"
                  className="border-white/30 bg-white/10 text-white hover:bg-white hover:text-black h-12 px-8"
                >
                  Послуги
                </Button>
              </Link>
            </div>
          </div>

          {/* RIGHT CARD */}
          <div className="relative">

            <div className="absolute -inset-6 bg-accent/20 blur-3xl rounded-full" />

            <div className="relative bg-white/10 backdrop-blur-xl border border-white/20 rounded-2xl p-6 shadow-2xl">

              <h3 className="text-lg font-semibold mb-4">
                Чому обирають нас
              </h3>

              <div className="space-y-4">
                {features.map((f, i) => (
                  <div key={i} className="flex gap-3 items-start">
                    <div className="p-2 rounded-lg bg-white/10">
                      <f.icon className="h-5 w-5 text-accent" />
                    </div>

                    <div>
                      <div className="font-medium">{f.title}</div>
                      <div className="text-sm text-white/60">{f.desc}</div>
                    </div>
                  </div>
                ))}
              </div>

            </div>
          </div>

        </div>
      </section>

      {/* CTA */}
      <section className="py-20 bg-background">
        <div className="container text-center max-w-2xl">

          <h2 className="text-3xl md:text-4xl font-bold">
            Потрібен ремонт авто?
          </h2>

          <p className="text-muted-foreground mt-4">
            Запишіться онлайн і отримайте швидке обслуговування без черг.
          </p>

          <Link to="/booking">
            <Button className="mt-8 bg-accent hover:bg-accent/90 text-black px-10 h-12">
              Записатися зараз
            </Button>
          </Link>

        </div>
      </section>

    </div>
  );
}