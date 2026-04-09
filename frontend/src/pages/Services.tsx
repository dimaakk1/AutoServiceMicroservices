import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Button } from "../components/ui/button";
import { Clock } from "lucide-react";
import { api } from "../api/api";

export default function Services() {
  const [services, setServices] = useState([]);

  useEffect(() => {
    api.get("/Catalog/Service")
      .then(res => setServices(res.data))
      .catch(err => console.error("Error loading services:", err));
  }, []);

  return (
    <div className="container py-12">
      <h1 className="text-3xl md:text-4xl font-display font-bold mb-2">
        Наші послуги
      </h1>

      <p className="text-muted-foreground mb-10 max-w-lg">
        Повний спектр послуг для вашого автомобіля
      </p>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {services.map(service => (
          <div
            key={service.serviceId}
            className="bg-card border rounded-lg p-6 flex flex-col hover:shadow-lg transition-shadow"
          >
            <div className="w-10 h-10 rounded-full bg-accent/10 flex items-center justify-center mb-4">
              <Clock className="h-5 w-5 text-accent" />
            </div>

            {/* categoryId поки просто показуємо */}
            <span className="text-xs text-muted-foreground mb-1">
              Категорія: {service.categoryId}
            </span>

            <h3 className="font-display font-semibold text-lg mb-2">
              {service.name}
            </h3>

            <p className="text-sm text-muted-foreground mb-4 flex-1">
              Послуга автосервісу
            </p>

            <div className="flex items-center justify-between text-sm mb-4">
              <span className="font-bold">
                {service.price} грн
              </span>
            </div>

            <Link to={`/booking?service=${service.serviceId}`}>
              <Button className="w-full" size="sm">
                Записатися
              </Button>
            </Link>
          </div>
        ))}
      </div>
    </div>
  );
}