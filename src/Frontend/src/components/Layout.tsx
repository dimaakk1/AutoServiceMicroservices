import { useState } from "react";
import { Link, useLocation } from "react-router-dom";
import {
  CalendarDays,
  Car,
  Gauge,
  LogOut,
  Menu,
  MessageSquare,
  ScanLine,
  Shield,
  User,
  Wrench,
  X,
} from "lucide-react";
import { useAuth } from "../lib/auth-context";
import { Button } from "./ui/button";
import AiChat from "./AiChatting";

const navItems = [
  { to: "/", label: "Головна", icon: Gauge },
  { to: "/services", label: "Послуги", icon: Wrench },
  { to: "/vin-decoder", label: "VIN-декодер", icon: ScanLine },
  { to: "/reviews", label: "Відгуки", icon: MessageSquare },
  { to: "/booking", label: "Запис", icon: CalendarDays },
];

const authNavItems = [
  { to: "/my-bookings", label: "Мої записи" },
  { to: "/my-vehicles", label: "Мої авто" },
  { to: "/profile", label: "Профіль" },
];

export default function Layout({ children }: { children: React.ReactNode }) {
  const { user, logout } = useAuth();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const isAdmin = user?.role === "Admin";

  const active = (to: string) =>
    to === "/" ? location.pathname === "/" : location.pathname.startsWith(to);

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <header className="sticky top-0 z-50 border-b bg-background/95 backdrop-blur">
        <div className="container flex min-h-16 items-stretch justify-between px-4 sm:px-6 lg:px-8">
          <Link to="/" className="flex items-center gap-3 border-r border-border pr-5 lg:pr-7">
            <span className="grid h-10 w-10 place-items-center bg-accent font-display text-2xl text-accent-foreground">A</span>
            <span>
              <strong className="block font-display text-2xl uppercase leading-none">АвтоПро</strong>
              <small className="technical-label mt-1 hidden sm:block">Service station</small>
            </span>
          </Link>

          <nav className="ml-auto hidden items-stretch xl:flex">
            {navItems.map((item) => (
              <Link
                key={item.to}
                to={item.to}
                className={`flex items-center border-l border-border px-3 text-[11px] font-semibold uppercase transition-colors 2xl:px-4 ${
                  active(item.to)
                    ? "border-b-2 border-b-accent bg-primary text-foreground"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                }`}
              >
                {item.label}
              </Link>
            ))}
            {user && authNavItems.map((item) => (
              <Link
                key={item.to}
                to={item.to}
                className={`flex items-center border-l border-border px-3 text-[11px] font-semibold uppercase transition-colors 2xl:px-4 ${
                  active(item.to)
                    ? "border-b-2 border-b-accent bg-primary text-foreground"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground"
                }`}
              >
                {item.label}
              </Link>
            ))}
            {isAdmin && (
              <Link to="/admin" className="flex items-center gap-1 border-l border-border px-4 text-[11px] font-semibold uppercase text-accent">
                <Shield className="h-3.5 w-3.5" /> Адмін
              </Link>
            )}
          </nav>

          <div className="hidden items-center gap-3 border-l border-border pl-4 xl:flex">
            {user ? (
              <>
                <span className="flex max-w-32 items-center gap-1 truncate text-xs text-muted-foreground">
                  <User className="h-4 w-4 shrink-0" /> {user.name}
                </span>
                <Button variant="ghost" size="icon" onClick={logout} aria-label="Вийти">
                  <LogOut />
                </Button>
              </>
            ) : (
              <Button asChild size="sm"><Link to="/auth">Увійти</Link></Button>
            )}
          </div>

          <Button
            variant="ghost"
            size="icon"
            className="self-center xl:hidden"
            onClick={() => setMobileOpen((value) => !value)}
            aria-label={mobileOpen ? "Закрити меню" : "Відкрити меню"}
            aria-expanded={mobileOpen}
          >
            {mobileOpen ? <X /> : <Menu />}
          </Button>
        </div>

        {mobileOpen && (
          <nav className="grid border-t bg-card p-3 xl:hidden">
            {[...navItems, ...(user ? authNavItems : [])].map((item) => (
              <Link
                key={item.to}
                to={item.to}
                onClick={() => setMobileOpen(false)}
                className={`flex items-center gap-3 border-l-2 px-4 py-3 text-xs font-semibold uppercase ${
                  active(item.to) ? "border-accent bg-primary" : "border-transparent text-muted-foreground"
                }`}
              >
                {"icon" in item && item.icon ? <item.icon className="h-4 w-4" /> : null}
                {item.label}
              </Link>
            ))}
            {isAdmin && (
              <Link to="/admin" onClick={() => setMobileOpen(false)} className="flex items-center gap-3 border-l-2 border-transparent px-4 py-3 text-xs font-semibold uppercase text-accent">
                <Shield className="h-4 w-4" /> Адмін-панель
              </Link>
            )}
            {user ? (
              <Button variant="ghost" className="mt-2 justify-start" onClick={() => { logout(); setMobileOpen(false); }}>
                <LogOut /> Вийти
              </Button>
            ) : (
              <Button asChild className="mt-2"><Link to="/auth" onClick={() => setMobileOpen(false)}>Увійти</Link></Button>
            )}
          </nav>
        )}
      </header>

      <main className="flex-1">{children}</main>
      {user && !isAdmin && <AiChat />}

      <footer className="border-t bg-card">
        <div className="container flex flex-col gap-3 py-7 text-[10px] font-semibold uppercase text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
          <span className="flex items-center gap-2"><Car className="h-4 w-4 text-accent" /> © 2026 АвтоПро / Професійний автосервіс</span>
          <span className="text-accent">Діагностика · Ремонт · Обслуговування</span>
        </div>
      </footer>
    </div>
  );
}
