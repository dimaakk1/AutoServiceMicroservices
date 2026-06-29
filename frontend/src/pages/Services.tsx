import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  Search,
  SlidersHorizontal,
  Clock,
  ArrowRight,
  Wrench
} from "lucide-react";

import { Input } from "../components/ui/input";
import { Button } from "../components/ui/button";
import { Card, CardContent } from "../components/ui/card";
import api from "../api/api";

type Service = {
  serviceId: number;
  name: string;
  price: number;
  categoryName: string;
  description?: string;
  duration?: string;
};

export default function Services() {
  const [services, setServices] = useState<Service[]>([]);
  const [keyword, setKeyword] = useState("");
  const [price, setPrice] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAll();
  }, []);

  const loadAll = async () => {
    try {
      setLoading(true);
      const res = await api.get("/Catalog/Service");
      setServices(res.data);
    } finally {
      setLoading(false);
    }
  };

  const handleSearch = async () => {
    if (!keyword.trim()) return loadAll();
    const res = await api.get(`/Catalog/Service/search?keyword=${keyword}`);
    setServices(res.data);
  };

  const handleAbove = async () => {
    if (!price) return;
    const res = await api.get(`/Catalog/Service/price/above/${price}`);
    setServices(res.data);
  };

  const handleBelow = async () => {
    if (!price) return;
    const res = await api.get(`/Catalog/Service/price/below/${price}`);
    setServices(res.data);
  };

  return (
    <div className="min-h-screen bg-background">

      {/* HEADER */}
      <section className="relative overflow-hidden py-16 border-b border-border">
        <div className="container">

          <div className="max-w-2xl">
            <div className="text-accent text-xs uppercase tracking-[0.2em] mb-3">
              // Каталог послуг
            </div>

            <h1 className="text-4xl md:text-5xl font-bold">
              Все для вашого авто
            </h1>

            <p className="text-muted-foreground mt-4">
              Діагностика, ремонт і обслуговування з фіксованими цінами та прозорими умовами.
            </p>
          </div>

        </div>
      </section>

      {/* FILTERS */}
      <section className="py-10">
        <div className="container">

          <Card className="border border-border bg-card shadow-card">
            <CardContent className="p-5">

              <div className="flex items-center gap-2 mb-4">
                <SlidersHorizontal className="h-5 w-5 text-muted-foreground" />
                <h2 className="font-semibold">Фільтри</h2>
              </div>

              <div className="flex flex-wrap gap-3">

                {/* SEARCH */}
                <div className="relative flex-1 min-w-[240px]">
                  <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                  <Input
                    value={keyword}
                    onChange={(e) => setKeyword(e.target.value)}
                    placeholder="Пошук послуги..."
                    className="pl-9"
                  />
                </div>

                <Button onClick={handleSearch} className="bg-accent text-black hover:bg-accent/90">
                  Пошук
                </Button>

                {/* PRICE */}
                <Input
                  type="number"
                  value={price}
                  onChange={(e) => setPrice(e.target.value)}
                  placeholder="Ціна"
                  className="w-[140px]"
                />

                <Button variant="outline" onClick={handleAbove}>
                  Дорожче
                </Button>

                <Button variant="outline" onClick={handleBelow}>
                  Дешевше
                </Button>

                <Button variant="ghost" onClick={loadAll} className="text-accent">
                  Скинути
                </Button>

              </div>
            </CardContent>
          </Card>

        </div>
      </section>

      {/* LIST */}
      <section className="pb-20">
        <div className="container">

          {loading ? (
            <div className="text-center text-muted-foreground py-20">
              Завантаження...
            </div>
          ) : services.length === 0 ? (
            <Card className="border-dashed">
              <CardContent className="py-16 text-center text-muted-foreground">
                Послуги не знайдено
              </CardContent>
            </Card>
          ) : (
            <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">

              {services.map((s) => (
                <Card
                  key={s.serviceId}
                  className="group relative bg-card border border-border hover:border-accent/40 hover:-translate-y-1 transition-all shadow-card"
                >

                  <CardContent className="p-6 flex flex-col h-full">

                    {/* ICON */}
                    <div className="w-12 h-12 rounded-xl bg-accent/10 flex items-center justify-center mb-5 group-hover:bg-accent transition-colors">
                      <Wrench className="h-5 w-5 text-accent group-hover:text-black" />
                    </div>

                    {/* TITLE */}
                    <h3 className="font-semibold text-lg mb-1">
                      {s.name}
                    </h3>

                    <p className="text-xs text-muted-foreground mb-4">
                      {s.categoryName}
                    </p>

                    {/* PRICE */}
                    <div className="mt-auto flex items-center justify-between pt-4 border-t border-border">
                      <span className="font-bold text-lg">
                        {s.price} ₴
                      </span>

                      <span className="flex items-center gap-1 text-xs text-muted-foreground">
                        <Clock className="h-3 w-3" />
                        ~60 хв
                      </span>
                    </div>

                    {/* CTA */}
                    <Link
                      to={`/booking?service=${s.serviceId}`}
                      className="mt-5 flex items-center justify-between text-sm font-medium group/btn"
                    >
                      <span>Записатися</span>
                      <ArrowRight className="h-4 w-4 text-accent group-hover/btn:translate-x-1 transition-transform" />
                    </Link>

                  </CardContent>
                </Card>
              ))}

            </div>
          )}

        </div>
      </section>

    </div>
  );
}