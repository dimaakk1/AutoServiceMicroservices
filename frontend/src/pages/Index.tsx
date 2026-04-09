import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Star, Clock, Shield, Wrench } from "lucide-react";
import heroImage from "../assets/hero-workshop.jpg";

const features = [
  { icon: Wrench, title: "Професійний ремонт", desc: "Досвідчені майстри з сертифікатами" },
  { icon: Clock, title: "Швидко та зручно", desc: "Онлайн запис на зручний час" },
  { icon: Shield, title: "Гарантія якості", desc: "Гарантія на всі виконані роботи" },
  { icon: Star, title: "Найкращі відгуки", desc: "Понад 500 задоволених клієнтів" },
];

export default function Index() {
  return (
    <div>
      {/* Hero */}
      <section className="relative h-[70vh] min-h-[500px] flex items-center overflow-hidden">
        <img
          src={heroImage}
          alt="Сучасна майстерня автосервісу"
          className="absolute inset-0 w-full h-full object-cover"
        />
        <div className="absolute inset-0 bg-primary/70" />
        <div className="container relative z-10 text-primary-foreground">
          <h1 className="text-4xl md:text-6xl font-display font-extrabold max-w-2xl leading-tight">
            Ваш автомобіль у надійних руках
          </h1>
          <p className="mt-4 text-lg md:text-xl max-w-lg opacity-90 font-body">
            Сучасне обладнання, досвідчені майстри та прозорі ціни. Запишіться онлайн за 2 хвилини.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link to="/booking">
              <Button size="lg" className="bg-accent text-accent-foreground hover:bg-accent/90 text-base px-8">
                Записатися
              </Button>
            </Link>
            <Link to="/services">
              <Button size="lg" variant="outline" className="border-primary-foreground/30 text-primary-foreground hover:bg-primary-foreground/10 text-base px-8">
                Наші послуги
              </Button>
            </Link>
          </div>
        </div>
      </section>

      {/* Features */}
      <section className="py-20">
        <div className="container">
          <h2 className="text-3xl font-display font-bold text-center mb-12">Чому обирають нас</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
            {features.map((f, i) => (
              <div key={i} className="bg-card rounded-lg border p-6 text-center hover:shadow-lg transition-shadow">
                <div className="mx-auto w-12 h-12 rounded-full bg-accent/10 flex items-center justify-center mb-4">
                  <f.icon className="h-6 w-6 text-accent" />
                </div>
                <h3 className="font-display font-semibold text-lg mb-2">{f.title}</h3>
                <p className="text-sm text-muted-foreground">{f.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-primary text-primary-foreground py-16">
        <div className="container text-center">
          <h2 className="text-3xl font-display font-bold mb-4">Потрібен ремонт?</h2>
          <p className="text-lg opacity-80 mb-8 max-w-md mx-auto">
            Запишіться прямо зараз і отримайте знижку 10% на першу послугу.
          </p>
          <Link to="/booking">
            <Button size="lg" className="bg-accent text-accent-foreground hover:bg-accent/90 text-base px-10">
              Записатися зараз
            </Button>
          </Link>
        </div>
      </section>
    </div>
  );
}
