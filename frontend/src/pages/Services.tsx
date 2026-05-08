import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
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

  useEffect(() => {
    loadAll();
  }, []);

  const loadAll = () => {
    api.get("/Catalog/Service")
      .then(res => setServices(res.data))
      .catch(err => console.error(err));
  };

  const handleSearch = () => {
    if (!keyword) return loadAll();

    api.get(`/Catalog/Service/search?keyword=${keyword}`)
      .then(res => setServices(res.data))
      .catch(err => console.error(err));
  };

  const handleAbove = () => {
    if (!price) return;

    api.get(`/Catalog/Service/price/above/${price}`)
      .then(res => setServices(res.data))
      .catch(err => console.error(err));
  };

  const handleBelow = () => {
    if (!price) return;

    api.get(`/Catalog/Service/price/below/${price}`)
      .then(res => setServices(res.data))
      .catch(err => console.error(err));
  };

  return (
    <div className="container py-12">
      <h1 className="text-3xl font-bold mb-6">
        Наші послуги
      </h1>
<p className="text-muted-foreground mb-10 max-w-lg">
        Повний спектр послуг для вашого автомобіля — від діагностики до капітального ремонту.
      </p>
      {/* 🔍 FILTERS */}
      <div className="flex flex-wrap gap-3 mb-8">

        <input
          type="text"
          placeholder="Пошук..."
          value={keyword}
          onChange={(e) => setKeyword(e.target.value)}
          className="border px-3 py-2 rounded"
        />

        <Button onClick={handleSearch}>
          Пошук
        </Button>

        <input
          type="number"
          placeholder="Ціна"
          value={price}
          onChange={(e) => setPrice(e.target.value)}
          className="border px-3 py-2 rounded"
        />

        <Button onClick={handleAbove}>
          Дорожче
        </Button>

        <Button onClick={handleBelow}>
          Дешевше
        </Button>

        <Button variant="outline" onClick={loadAll}>
          Скинути
        </Button>

      </div>

      {/* 📦 LIST */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">

        {services.map(service => (
          <div
            key={service.serviceId}
            className="bg-card border rounded-lg p-6 flex flex-col"
          >
            <h3 className="font-semibold text-lg mb-2">
              {service.name}
            </h3>

            {/* ✅ CATEGORY NAME FIXED */}
            <p className="text-sm text-orange-600 font-medium mb-3">
              {service.categoryName}
            </p>

            <div className="flex justify-between mb-4">
              <span className="font-bold">
                {service.price} грн
              </span>
            </div>

            <Link to={`/booking?service=${service.serviceId}`}>
              <Button className="w-full bg-orange-500 hover:bg-orange-600">
                Записатися
              </Button>
            </Link>
          </div>
        ))}

      </div>
    </div>
  );
}