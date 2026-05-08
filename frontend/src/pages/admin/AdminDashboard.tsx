import { useEffect, useState } from "react";
import { Link } from "react-router-dom";

import {
  ClipboardList,
  Package,
  Users,
  ArrowLeft,
  Star,
} from "lucide-react";

import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "../../components/ui/card";

import api from "../../api/api";

export default function AdminDashboard() {
  const [stats, setStats] = useState({
    orders: 0,
    services: 0,
    reviews: 0,
    users: 0,
  });

  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setLoading(true);

      const [ordersRes, servicesRes, reviewsRes] =
        await Promise.all([
          api.get("/Orders/Order"),
          api.get("/Catalog/Service"),
          api.get("/Reviews"),
        ]);

      setStats({
        orders: ordersRes.data.length || 0,
        services: servicesRes.data.length || 0,
        reviews: reviewsRes.data.length || 0,
        users: 0,
      });
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const cards = [
    {
      title: "Замовлення",
      value: stats.orders,
      icon: ClipboardList,
      to: "/admin/orders",
      color: "text-orange-500",
    },

    {
      title: "Послуги",
      value: stats.services,
      icon: Package,
      to: "/admin/services",
      color: "text-green-500",
    },

    {
      title: "Відгуки",
      value: stats.reviews,
      icon: Star,
      to: "/admin/reviews",
      color: "text-yellow-500",
    },

    {
      title: "Користувачі",
      value: stats.users,
      icon: Users,
      to: "/admin/users",
      color: "text-blue-500",
    },
  ];

  if (loading) {
    return (
      <div className="container py-20 text-center">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="container py-8">
      {/* HEADER */}
      <div className="flex items-center gap-4 mb-8">
        <Link
          to="/"
          className="text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>

        <h1 className="text-3xl font-bold">
          Адмін-панель
        </h1>
      </div>

      {/* STATS */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        {cards.map((card) => (
          <Link key={card.to} to={card.to}>
            <Card className="hover:shadow-lg transition-all hover:scale-[1.02] cursor-pointer">
              <CardHeader className="flex flex-row items-center justify-between pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  {card.title}
                </CardTitle>

                <card.icon className={`h-5 w-5 ${card.color}`} />
              </CardHeader>

              <CardContent>
                <div className="text-3xl font-bold">
                  {card.value}
                </div>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>

      {/* INFO */}
      <div className="mt-10 grid gap-4 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>
              Швидкі дії
            </CardTitle>
          </CardHeader>

          <CardContent className="flex flex-wrap gap-3">
            <Link to="/admin/orders">
              <button className="px-4 py-2 rounded-md bg-accent text-accent-foreground hover:bg-accent/90">
                Замовлення
              </button>
            </Link>

            <Link to="/admin/services">
              <button className="px-4 py-2 rounded-md border hover:bg-muted">
                Послуги
              </button>
            </Link>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>
              Статистика
            </CardTitle>
          </CardHeader>

          <CardContent className="space-y-2 text-sm text-muted-foreground">
            <p>
              • Замовлень: {stats.orders}
            </p>

            <p>
              • Послуг: {stats.services}
            </p>

            <p>
              • Відгуків: {stats.reviews}
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}