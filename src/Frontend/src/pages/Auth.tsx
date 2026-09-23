import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Car, Clock, ShieldCheck, Sparkles } from "lucide-react";
import { toast } from "sonner";
import { loginUser, registerUser } from "../api/auth";
import { useAuth } from "../lib/auth-context";
import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";

export default function Auth() {
  const [isLogin, setIsLogin] = useState(true);
  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [awaitingVerification, setAwaitingVerification] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (loading) return;
    setLoading(true);

    try {
      if (isLogin) {
        const response = await loginUser(username, password);
        const accessToken = response.data?.accessToken;
        const refreshToken = response.data?.refreshToken;

        if (!accessToken || !refreshToken) {
          toast.error("Сервер не повернув токени авторизації");
          return;
        }

        await login(accessToken, refreshToken);
        toast.success("Вхід виконано успішно");
        setUsername("");
        setPassword("");
        navigate("/");
      } else {
        await registerUser(username, email, password);
        toast.success("Реєстрація успішна! Перевірте пошту для підтвердження акаунта");
        setAwaitingVerification(true);
        setUsername("");
        setEmail("");
        setPassword("");
      }
    } catch (error: any) {
      const message =
        error?.response?.data?.message ||
        error?.response?.data?.title ||
        error?.response?.data?.error ||
        (typeof error?.response?.data === "string" ? error.response.data : null) ||
        (error?.response?.status === 401 ? "Невірний логін або пароль" : null) ||
        (error?.response?.status === 403 ? "Доступ заборонено. Можливо, користувача заблоковано" : null) ||
        "Сталася помилка під час авторизації";

      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  const perks = [
    { icon: Clock, text: "Онлайн-запис на зручний час" },
    { icon: ShieldCheck, text: "Безпечний персональний кабінет" },
    { icon: Sparkles, text: "AI-діагностика та рекомендації" },
  ];

  return (
    <div className="grid min-h-[calc(100vh-4rem)] lg:grid-cols-2">
      <div className="relative hidden flex-col justify-between overflow-hidden bg-gradient-hero p-12 text-primary-foreground lg:flex">
        <div className="absolute inset-0 bg-grid-dark opacity-30" />
        <div className="absolute -left-20 -top-20 h-80 w-80 bg-accent/20 blur-3xl" />
        <div className="relative flex items-center gap-3">
          <span className="grid h-11 w-11 place-items-center bg-accent font-display text-2xl text-accent-foreground">A</span>
          <span className="font-display text-3xl uppercase">АвтоПро</span>
        </div>

        <div className="relative space-y-9">
          <div>
            <div className="technical-label mb-4">Особистий кабінет</div>
            <h1 className="max-w-xl text-6xl leading-[.92] text-balance">Керуйте обслуговуванням авто з одного місця.</h1>
          </div>
          <div className="space-y-3">
            {perks.map((perk) => (
              <div key={perk.text} className="flex items-center gap-3 border border-white/10 bg-white/5 p-3 text-sm text-primary-foreground/80">
                <span className="grid h-9 w-9 place-items-center border border-white/15 bg-white/10"><perk.icon className="h-4 w-4 text-accent" /></span>
                {perk.text}
              </div>
            ))}
          </div>
        </div>
        <div className="relative text-xs uppercase tracking-widest text-primary-foreground/40">© 2026 АвтоПро</div>
      </div>

      <div className="flex items-center justify-center bg-background p-6 sm:p-12">
        <div className="w-full max-w-md">
          <div className="mb-8 flex w-fit border border-border bg-muted p-1">
            <button
              type="button"
              onClick={() => setIsLogin(true)}
              className={`px-5 py-2 text-xs font-semibold uppercase transition-colors ${isLogin ? "bg-accent text-accent-foreground" : "text-muted-foreground hover:text-foreground"}`}
            >
              Вхід
            </button>
            <button
              type="button"
              onClick={() => setIsLogin(false)}
              className={`px-5 py-2 text-xs font-semibold uppercase transition-colors ${!isLogin ? "bg-accent text-accent-foreground" : "text-muted-foreground hover:text-foreground"}`}
            >
              Реєстрація
            </button>
          </div>

          <div className="technical-label mb-3 flex items-center gap-3"><span className="h-px w-8 bg-accent" />Доступ до системи</div>
          <h2 className="text-5xl leading-none md:text-6xl">{isLogin ? "З поверненням" : "Створимо акаунт"}</h2>
          <p className="mb-8 mt-4 text-muted-foreground">{isLogin ? "Введіть свої дані, щоб продовжити." : "Заповніть форму — це займе менше хвилини."}</p>

          {awaitingVerification && (
            <div className="mb-5 border border-amber-400/30 bg-amber-400/10 p-4 text-sm text-amber-300">
              Ми надіслали лист для підтвердження email. Активуйте акаунт перед входом.
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            <div className="space-y-2">
              <Label htmlFor="username" className="text-xs uppercase tracking-wider text-muted-foreground">Ім'я користувача</Label>
              <Input id="username" value={username} onChange={(event) => setUsername(event.target.value)} required placeholder="Username" className="h-11" />
            </div>

            {!isLogin && (
              <div className="space-y-2">
                <Label htmlFor="email" className="text-xs uppercase tracking-wider text-muted-foreground">Email</Label>
                <Input id="email" type="email" value={email} onChange={(event) => setEmail(event.target.value)} required placeholder="example@email.com" className="h-11" />
              </div>
            )}

            <div className="space-y-2">
              <Label htmlFor="password" className="text-xs uppercase tracking-wider text-muted-foreground">Пароль</Label>
              <Input id="password" type="password" value={password} onChange={(event) => setPassword(event.target.value)} required placeholder="••••••••" className="h-11" />
            </div>

            <Button type="submit" disabled={loading} className="h-12 w-full bg-accent text-accent-foreground shadow-glow hover:bg-accent/90">
              {loading ? "Зачекайте..." : isLogin ? "Увійти" : "Створити акаунт"}
            </Button>
          </form>

          <div className="mt-7 flex items-center gap-3 text-xs text-muted-foreground">
            <Car className="h-4 w-4 text-accent" /> Ваші дані використовуються лише для роботи сервісу.
          </div>
        </div>
      </div>
    </div>
  );
}
