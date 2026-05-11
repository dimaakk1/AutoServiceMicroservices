import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import {
  Search,
  SlidersHorizontal,
  Wrench,
} from "lucide-react";

import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import {
  Card,
  CardContent,
} from "../components/ui/card";

import api from "../api/api";

type Service = {
  serviceId: number;
  name: string;
  price: number;
  categoryName: string;
};

export default function Services() {
  const [services, setServices] = useState<Service[]>([]);

  const [keyword, setKeyword] = useState("");
  const [price, setPrice] = useState("");

  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadAll();
  }, []);

  /* ================= LOAD ================= */

  const loadAll = async () => {
    try {
      setLoading(true);

      const res = await api.get("/Catalog/Service");

      setServices(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  /* ================= SEARCH ================= */

  const handleSearch = async () => {
    try {
      if (!keyword.trim()) {
        return loadAll();
      }

      const res = await api.get(
        `/Catalog/Service/search?keyword=${keyword}`
      );

      setServices(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  /* ================= PRICE FILTERS ================= */

  const handleAbove = async () => {
    try {
      if (!price) return;

      const res = await api.get(
        `/Catalog/Service/price/above/${price}`
      );

      setServices(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  const handleBelow = async () => {
    try {
      if (!price) return;

      const res = await api.get(
        `/Catalog/Service/price/below/${price}`
      );

      setServices(res.data);
    } catch (err) {
      console.error(err);
    }
  };

  /* ================= UI ================= */

  if (loading) {
    return (
      <div className="container py-20 text-center text-muted-foreground">
        Завантаження послуг...
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-muted/20">

      <div className="container py-12">

        {/* ================= HEADER ================= */}
        <div className="mb-10">

          <h1 className="text-4xl font-bold mb-4">
            Наші послуги
          </h1>

          <p className="text-muted-foreground max-w-2xl text-lg">
            Повний спектр послуг для вашого автомобіля —
            від діагностики до складного ремонту.
          </p>

        </div>

        {/* ================= FILTER PANEL ================= */}
        <Card className="mb-10 shadow-sm">
          <CardContent className="p-5">

            <div className="flex items-center gap-2 mb-5">
              <SlidersHorizontal className="h-5 w-5 text-muted-foreground" />

              <h2 className="font-semibold text-lg">
                Фільтри та пошук
              </h2>
            </div>

            <div className="flex flex-wrap gap-3">

              {/* SEARCH */}
              <div className="relative min-w-[240px] flex-1">

                <Search className="absolute left-3 top-3.5 h-4 w-4 text-muted-foreground" />

                <Input
                  placeholder="Пошук послуг..."
                  value={keyword}
                  onChange={(e) => setKeyword(e.target.value)}
                  className="pl-9"
                />
              </div>

              <Button
                onClick={handleSearch}
                className="bg-orange-500 hover:bg-orange-600 text-white"
              >
                Пошук
              </Button>

              {/* PRICE */}
              <Input
                type="number"
                placeholder="Ціна"
                value={price}
                onChange={(e) => setPrice(e.target.value)}
                className="w-[140px]"
              />

              <Button
                variant="outline"
                onClick={handleAbove}
                className="border-orange-200 hover:bg-orange-50"
              >
                Дорожче
              </Button>

              <Button
                variant="outline"
                onClick={handleBelow}
                className="border-orange-200 hover:bg-orange-50"
              >
                Дешевше
              </Button>

              <Button
                variant="ghost"
                onClick={loadAll}
                className="text-orange-600 hover:bg-orange-100"
              >
                Скинути
              </Button>

            </div>

          </CardContent>
        </Card>

        {/* ================= SERVICES ================= */}
        {services.length === 0 ? (
          <Card className="border-dashed">
            <CardContent className="py-14 text-center text-muted-foreground">
              Послуги не знайдено
            </CardContent>
          </Card>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6">

            {services.map((service) => (
              <Card
                key={service.serviceId}
                className="shadow-sm hover:shadow-lg transition-all hover:-translate-y-1"
              >
                <CardContent className="p-6 flex flex-col h-full">

                  {/* ICON */}
                  <div className="w-12 h-12 rounded-xl bg-orange-100 flex items-center justify-center mb-5">
                    <Wrench className="h-6 w-6 text-orange-500" />
                  </div>

                  {/* TITLE */}
                  <h3 className="text-xl font-semibold mb-2">
                    {service.name}
                  </h3>

                  {/* CATEGORY */}
                  <p className="text-sm text-muted-foreground mb-4">
                    {service.categoryName}
                  </p>

                  {/* PRICE */}
                  <div className="mt-auto mb-5">

                    <p className="text-sm text-muted-foreground mb-1">
                      Вартість
                    </p>

                    <p className="text-2xl font-bold">
                      {service.price} ₴
                    </p>

                  </div>

                  {/* BUTTON */}
                  <Link to={`/booking?service=${service.serviceId}`}>
                    <Button className="w-full bg-orange-500 hover:bg-orange-600 text-white">
                      Записатися
                    </Button>
                  </Link>

                </CardContent>
              </Card>
            ))}

          </div>
        )}

      </div>
    </div>
  );
}