import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../lib/auth-context";
import { Car, Menu, X, LogOut, User, Shield } from "lucide-react";
import { Button } from "../components/ui/button";
import { useState } from "react";

const navItems = [
  { to: "/", label: "Головна" },
  { to: "/services", label: "Послуги" },
  { to: "/reviews", label: "Відгуки" },
  { to: "/booking", label: "Запис" },
];

const authNavItems = [
  { to: "/my-bookings", label: "Мої записи" },
  { to: "/profile", label: "Профіль" },
];

export default function Layout({ children }: { children: React.ReactNode }) {
  const { user, logout } = useAuth();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);

  const isAdmin = user?.role === "Admin";

  return (
    <div className="min-h-screen flex flex-col">

      {/* HEADER */}
      <header className="sticky top-0 z-50 border-b bg-card/95 backdrop-blur">
        <div className="container flex h-16 items-center justify-between">

          <Link to="/" className="flex items-center gap-2 font-bold text-xl">
            <Car className="h-6 w-6 text-accent" />
            <span>АвтоПро</span>
          </Link>

          {/* NAV DESKTOP */}
          <nav className="hidden md:flex items-center gap-1">
            {navItems.map(item => (
              <Link
                key={item.to}
                to={item.to}
                className={`px-3 py-2 rounded-md text-sm ${
                  location.pathname === item.to
                    ? "bg-primary text-primary-foreground"
                    : "hover:bg-muted"
                }`}
              >
                {item.label}
              </Link>
            ))}

            {user &&
              authNavItems.map(item => (
                <Link
                  key={item.to}
                  to={item.to}
                  className={`px-3 py-2 rounded-md text-sm ${
                    location.pathname === item.to
                      ? "bg-primary text-primary-foreground"
                      : "hover:bg-muted"
                  }`}
                >
                  {item.label}
                </Link>
              ))}

            {isAdmin && (
              <Link
                to="/admin"
                className="px-3 py-2 text-accent flex items-center gap-1"
              >
                <Shield className="h-4 w-4" />
                Адмін
              </Link>
            )}
          </nav>

          {/* USER DESKTOP */}
          <div className="hidden md:flex items-center gap-2">
            {user ? (
              <>
                <span className="text-sm flex items-center gap-1">
                  <User className="h-4 w-4" />
                  {user.name}
                </span>

                <Button variant="ghost" size="sm" onClick={logout}>
                  <LogOut className="h-4 w-4 mr-1" />
                  Вийти
                </Button>
              </>
            ) : (
              <Link to="/auth">
                <Button size="sm">Увійти</Button>
              </Link>
            )}
          </div>

          {/* MOBILE BUTTON */}
          <button
            className="md:hidden"
            onClick={() => setMobileOpen(!mobileOpen)}
          >
            {mobileOpen ? <X /> : <Menu />}
          </button>
        </div>

        {/* MOBILE MENU */}
        {mobileOpen && (
          <div className="md:hidden border-t p-4">
            {navItems.map(item => (
              <Link
                key={item.to}
                to={item.to}
                onClick={() => setMobileOpen(false)}
                className="block py-2"
              >
                {item.label}
              </Link>
            ))}

            {user &&
              authNavItems.map(item => (
                <Link
                  key={item.to}
                  to={item.to}
                  onClick={() => setMobileOpen(false)}
                  className="block py-2"
                >
                  {item.label}
                </Link>
              ))}

            {isAdmin && (
              <Link
                to="/admin"
                onClick={() => setMobileOpen(false)}
                className="block py-2 text-accent"
              >
                Адмін
              </Link>
            )}

            {user ? (
              <button
                onClick={() => {
                  logout();
                  setMobileOpen(false);
                }}
                className="text-red-500 py-2"
              >
                Вийти ({user.name})
              </button>
            ) : (
              <Link
                to="/auth"
                onClick={() => setMobileOpen(false)}
                className="text-accent py-2 block"
              >
                Увійти
              </Link>
            )}
          </div>
        )}
      </header>

      <main className="flex-1">{children}</main>
    </div>
  );
}