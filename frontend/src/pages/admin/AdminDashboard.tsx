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

      const [ordersRes, servicesRes, reviewsRes, usersRes] =
        await Promise.all([
          api.get("/Orders/Order"),
          api.get("/Catalog/Service"),
          api.get("/Reviews"),
          api.get("/users"),
        ]);

      setStats({
        orders: ordersRes.data?.length || 0,
        services: servicesRes.data?.length || 0,
        reviews: reviewsRes.data?.length || 0,
        users: usersRes.data?.length || 0,
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
      accent: "text-orange-500",
    },
    {
      title: "Послуги",
      value: stats.services,
      icon: Package,
      to: "/admin/services",
      accent: "text-green-500",
    },
    {
      title: "Відгуки",
      value: stats.reviews,
      icon: Star,
      to: "/admin/reviews",
      accent: "text-yellow-500",
    },
    {
      title: "Користувачі",
      value: stats.users,
      icon: Users,
      to: "/admin/users",
      accent: "text-blue-500",
    },
  ];

  if (loading) {
    return (
      <div className="container py-20 text-center text-orange-500">
        Завантаження...
      </div>
    );
  }

  return (
    <div className="container py-10 max-w-6xl">

      <div className="flex items-center gap-4 mb-8">
        <Link
          to="/"
          className="text-muted-foreground hover:text-orange-500 transition"
        >
          <ArrowLeft className="h-5 w-5" />
        </Link>

        <h1 className="text-3xl font-bold">
          Адмін-панель
        </h1>
      </div>

      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
        {cards.map((c) => (
          <Link key={c.to} to={c.to}>
            <Card className="border hover:shadow-md hover:border-orange-300 transition cursor-pointer">
              <CardHeader className="flex flex-row items-center justify-between pb-2">
                <CardTitle className="text-sm font-medium text-muted-foreground">
                  {c.title}
                </CardTitle>

                <c.icon className={`h-5 w-5 ${c.accent}`} />
              </CardHeader>

              <CardContent>
                <div className="text-3xl font-bold text-foreground">
                  {c.value}
                </div>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>

      <div className="mt-10 grid gap-6 md:grid-cols-2">

        <Card className="border">
          <CardHeader>
            <CardTitle>Швидкі дії</CardTitle>
          </CardHeader>

          <CardContent className="flex flex-wrap gap-3">
            <Link to="/admin/orders">
              <button className="px-4 py-2 rounded-md bg-orange-500 text-white hover:bg-orange-600 transition">
                Замовлення
              </button>
            </Link>

            <Link to="/admin/services">
              <button className="px-4 py-2 rounded-md border hover:bg-muted transition">
                Послуги
              </button>
            </Link>
          </CardContent>
        </Card>

        <Card className="border">
          <CardHeader>
            <CardTitle>Статистика</CardTitle>
          </CardHeader>

          <CardContent className="space-y-2 text-sm text-muted-foreground">
            <p>Замовлень: <span className="text-foreground font-medium">{stats.orders}</span></p>
            <p>Послуг: <span className="text-foreground font-medium">{stats.services}</span></p>
            <p>Відгуків: <span className="text-foreground font-medium">{stats.reviews}</span></p>
            <p>Користувачів: <span className="text-foreground font-medium">{stats.users}</span></p>
          </CardContent>
        </Card>

      </div>
    </div>
  );
}