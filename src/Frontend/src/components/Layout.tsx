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
  { to: "/my-bookings", label: "Мої записи", icon: CalendarDays },
  { to: "/my-vehicles", label: "Мої авто", icon: Car },
  { to: "/profile", label: "Профіль", icon: User },
];

export default function Layout({ children }: { children: React.ReactNode }) {
  const { user, logout } = useAuth();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const isAdmin = user?.role === "Admin";
  const isMechanic = user?.role === "Mechanic";
  const desktopNavItems = [
    ...navItems.filter((item) => item.to !== "/booking"),
    ...(user && !isMechanic ? authNavItems.filter((item) => item.to !== "/profile") : []),
  ];

  const active = (to: string) =>
    to === "/" ? location.pathname === "/" : location.pathname.startsWith(to);

  return (
    <div className="flex min-h-screen flex-col bg-background">
      <header className="sticky top-0 z-50 border-b border-accent/20 bg-[#061018]/95 shadow-[0_12px_38px_rgba(0,0,0,.28)] backdrop-blur-xl">
        <div aria-hidden="true" className="absolute inset-x-0 bottom-0 h-px bg-gradient-to-r from-transparent via-accent/70 to-transparent" />
        <div className="container flex h-16 items-center gap-4 px-4 sm:px-6 lg:px-8">
          <Link to="/" className="group flex shrink-0 items-center gap-3" aria-label="АвтоПро — головна">
            <span className="relative grid h-11 w-11 place-items-center border border-accent/60 bg-accent/10 font-display text-2xl text-accent shadow-[inset_0_0_18px_rgba(82,199,242,.12)] transition-colors group-hover:bg-accent group-hover:text-accent-foreground">
              A
              <span className="absolute -right-1 -top-1 h-2 w-2 border border-background bg-success shadow-[0_0_10px_rgba(52,211,153,.8)]" />
            </span>
            <span className="leading-none">
              <strong className="block font-display text-[1.7rem] uppercase tracking-wide">АвтоПро</strong>
              <small className="mt-1 hidden text-[8px] font-semibold uppercase tracking-[.28em] text-accent/80 sm:block">Intelligent service</small>
            </span>
          </Link>

          <nav className="ml-auto hidden h-10 items-center gap-1 border border-border/80 bg-card/55 p-1 shadow-[inset_0_0_20px_rgba(0,0,0,.16)] xl:flex">
            {desktopNavItems.map((item) => (
              <Link
                key={item.to}
                to={item.to}
                className={`relative flex h-full items-center gap-2 px-2.5 text-[10px] font-semibold uppercase tracking-[.08em] transition-all 2xl:px-3 ${
                  active(item.to)
                    ? "bg-accent/15 text-accent shadow-[inset_0_-2px_0_hsl(var(--accent))]"
                    : "text-muted-foreground hover:bg-muted/80 hover:text-foreground"
                }`}
              >
                <item.icon className="h-3.5 w-3.5" />
                <span>{item.label}</span>
              </Link>
            ))}
          </nav>

          <div className="ml-auto hidden items-center gap-2 xl:flex">
            <div className="mr-1 hidden items-center gap-2 border-r border-border pr-3 2xl:flex">
              <span className="h-1.5 w-1.5 bg-success shadow-[0_0_8px_rgba(52,211,153,.8)]" />
              <span className="text-[9px] font-semibold uppercase tracking-[.14em] text-muted-foreground">Станція онлайн</span>
            </div>

            {isAdmin && (
              <Link to="/admin" className="grid h-9 w-9 place-items-center border border-accent/35 text-accent transition-colors hover:bg-accent/10" aria-label="Адмін-панель" title="Адмін-панель">
                <Shield className="h-4 w-4" />
              </Link>
            )}
            {isMechanic && (
              <Link to="/mechanic" className="grid h-9 w-9 place-items-center border border-accent/35 text-accent transition-colors hover:bg-accent/10" aria-label="Кабінет майстра" title="Кабінет майстра">
                <Wrench className="h-4 w-4" />
              </Link>
            )}

            {user ? (
              <>
                <Link to="/profile" className="flex h-9 max-w-36 items-center gap-2 border border-border bg-card/55 px-3 text-[10px] font-semibold uppercase text-muted-foreground transition-colors hover:border-accent/50 hover:text-foreground">
                  <User className="h-3.5 w-3.5 shrink-0 text-accent" />
                  <span className="truncate">{user.name}</span>
                </Link>
                <Button variant="ghost" size="icon" className="h-9 w-9 border border-transparent hover:border-border" onClick={logout} aria-label="Вийти">
                  <LogOut className="h-4 w-4" />
                </Button>
              </>
            ) : (
              <Button asChild size="sm" variant="ghost" className="h-9 border border-border px-3 text-[10px] uppercase tracking-wider">
                <Link to="/auth"><User className="h-3.5 w-3.5" /> Увійти</Link>
              </Button>
            )}

            {!isMechanic && (
              <Button asChild size="sm" className="h-9 px-3 text-[10px] uppercase tracking-wider shadow-[0_0_22px_rgba(82,199,242,.15)] 2xl:px-4">
                <Link to="/booking"><CalendarDays className="h-3.5 w-3.5" /> Записатися</Link>
              </Button>
            )}
          </div>

          <Button
            variant="ghost"
            size="icon"
            className="ml-auto h-10 w-10 border border-border bg-card/50 xl:hidden"
            onClick={() => setMobileOpen((value) => !value)}
            aria-label={mobileOpen ? "Закрити меню" : "Відкрити меню"}
            aria-expanded={mobileOpen}
          >
            {mobileOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
          </Button>
        </div>

        {mobileOpen && (
          <div className="border-t border-border bg-[#07131b]/98 p-3 shadow-2xl xl:hidden">
            <div className="container p-0">
              {user && (
                <div className="mb-3 flex items-center justify-between border border-border bg-card/60 p-3">
                  <div className="flex min-w-0 items-center gap-3">
                    <span className="grid h-9 w-9 shrink-0 place-items-center bg-accent/15 text-accent"><User className="h-4 w-4" /></span>
                    <div className="min-w-0"><div className="truncate text-sm font-semibold">{user.name}</div><div className="technical-label text-muted-foreground">Особистий кабінет</div></div>
                  </div>
                  <Button variant="ghost" size="icon" onClick={() => { logout(); setMobileOpen(false); }} aria-label="Вийти"><LogOut className="h-4 w-4" /></Button>
                </div>
              )}

              <nav className="grid grid-cols-1 gap-1 sm:grid-cols-2">
                {[...navItems, ...(user && !isMechanic ? authNavItems : [])].map((item) => (
                  <Link
                    key={item.to}
                    to={item.to}
                    onClick={() => setMobileOpen(false)}
                    className={`flex items-center gap-3 border px-4 py-3 text-[11px] font-semibold uppercase tracking-wider transition-colors ${
                      active(item.to) ? "border-accent/50 bg-accent/15 text-accent" : "border-border bg-card/35 text-muted-foreground hover:text-foreground"
                    }`}
                  >
                    <item.icon className="h-4 w-4" />
                    {item.label}
                  </Link>
                ))}
                {isAdmin && (
                  <Link to="/admin" onClick={() => setMobileOpen(false)} className="flex items-center gap-3 border border-accent/40 bg-accent/10 px-4 py-3 text-[11px] font-semibold uppercase tracking-wider text-accent">
                    <Shield className="h-4 w-4" /> Адмін-панель
                  </Link>
                )}
                {isMechanic && (
                  <Link to="/mechanic" onClick={() => setMobileOpen(false)} className="flex items-center gap-3 border border-accent/40 bg-accent/10 px-4 py-3 text-[11px] font-semibold uppercase tracking-wider text-accent">
                    <Wrench className="h-4 w-4" /> Кабінет майстра
                  </Link>
                )}
              </nav>

              {!user && (
                <Button asChild className="mt-3 w-full"><Link to="/auth" onClick={() => setMobileOpen(false)}><User className="h-4 w-4" /> Увійти</Link></Button>
              )}
            </div>
          </div>
        )}
      </header>

      <main className="flex-1">{children}</main>
      {user?.role === "User" && <AiChat />}

      <footer className="border-t bg-card">
        <div className="container flex flex-col gap-3 py-7 text-[10px] font-semibold uppercase text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
          <span className="flex items-center gap-2"><Car className="h-4 w-4 text-accent" /> © 2026 АвтоПро / Професійний автосервіс</span>
          <span className="text-accent">Діагностика · Ремонт · Обслуговування</span>
        </div>
      </footer>
    </div>
  );
}
