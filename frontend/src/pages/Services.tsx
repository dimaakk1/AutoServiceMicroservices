import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Clock } from "lucide-react";
import { api } from "../api/api";

export default function Services() {
  const [services, setServices] = useState([]);
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

  // 🔍 пошук
  const handleSearch = () => {
    if (!keyword) return loadAll();

    api.get(`/Catalog/Service/search?keyword=${keyword}`)
      .then(res => setServices(res.data))
      .catch(err => console.error(err));
  };

  // 💰 більше ніж
  const handleAbove = () => {
    if (!price) return;

    api.get(`/Catalog/Service/price/above/${price}`)
      .then(res => setServices(res.data))
      .catch(err => console.error(err));
  };

  // 💰 менше ніж
  const handleBelow = () => {
    if (!price) return;

    api.get(`/Catalog/Service/price/below/${price}`)
      .then(res => setServices(res.data))
      .catch(err => console.error(err));
  };

  return (
    <div className="container py-12">
      <h1 className="text-3xl font-bold mb-6">Наші послуги</h1>

      {/* 🔍 ФІЛЬТРИ */}
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

      {/* 📦 СПИСОК */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        {services.map(service => (
          <div
            key={service.serviceId}
            className="bg-card border rounded-lg p-6 flex flex-col"
          >
            <h3 className="font-semibold text-lg mb-2">
              {service.name}
            </h3>

            <p className="text-sm text-muted-foreground mb-4">
              Категорія: {service.categoryId}
            </p>

            <div className="flex justify-between mb-4">
              <span className="font-bold">
                {service.price} грн
              </span>

              <span className="flex items-center gap-1 text-sm">
                <Clock className="h-4 w-4" />
                —
              </span>
            </div>

            <Link to={`/booking?service=${service.serviceId}`}>
              <Button className="w-full">
                Записатися
              </Button>
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
}