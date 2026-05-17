import { useState } from "react";
import { useNavigate } from "react-router-dom";

import {
  loginUser,
  registerUser,
} from "../api/auth";

import { useAuth } from "../lib/auth-context";

import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";

import {
  Card,
  CardContent,
} from "../components/ui/card";

import {
  LockKeyhole,
  Mail,
  User,
  ShieldCheck,
  Wrench,
} from "lucide-react";

import { toast } from "sonner";

export default function Auth() {
  const [isLogin, setIsLogin] = useState(true);

  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [loading, setLoading] = useState(false);

  // ✅ NEW STATE FOR EMAIL VERIFICATION UX
  const [awaitingVerification, setAwaitingVerification] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (
    e: React.FormEvent<HTMLFormElement>
  ) => {
    e.preventDefault();

    if (loading) return;

    setLoading(true);

    try {
      // LOGIN
      if (isLogin) {
        const res = await loginUser(username, password);

        const accessToken = res.data?.accessToken;
        const refreshToken = res.data?.refreshToken;

        if (!accessToken || !refreshToken) {
          toast.error("Сервер не повернув токени авторизації");
          return;
        }

        login(accessToken, refreshToken);

        toast.success("Вхід виконано успішно");

        setUsername("");
        setPassword("");

        navigate("/");
      }

      // REGISTER
      else {
        await registerUser(username, email, password);

        toast.success(
          "Реєстрація успішна! Перевір свою пошту для підтвердження акаунта"
        );

        // ✅ SHOW EMAIL VERIFICATION STATE
        setAwaitingVerification(true);

        // clear fields but DON'T switch to login
        setUsername("");
        setEmail("");
        setPassword("");
      }
    } catch (err: any) {
      console.error(err);

      const message =
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        err?.response?.data?.error ||
        (typeof err?.response?.data === "string"
          ? err.response.data
          : null) ||
        (err?.response?.status === 401
          ? "Невірний логін або пароль"
          : null) ||
        (err?.response?.status === 403
          ? "Доступ заборонено. Можливо користувача заблоковано"
          : null) ||
        "Сталася помилка під час авторизації";

      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-muted/30 flex items-center justify-center px-4 py-12">

      <div className="w-full max-w-5xl grid lg:grid-cols-2 overflow-hidden rounded-3xl border bg-card shadow-xl">

        {/* LEFT SIDE */}
        <div className="hidden lg:flex relative bg-gradient-to-br from-orange-500 to-orange-400 text-white p-12 flex-col justify-between">

          <div>
            <div className="w-16 h-16 rounded-2xl bg-white/20 flex items-center justify-center backdrop-blur mb-8">
              <Wrench className="h-8 w-8" />
            </div>

            <h1 className="text-5xl font-bold leading-tight mb-5">
              AutoService
            </h1>

            <p className="text-lg text-white/90 max-w-md leading-relaxed">
              Онлайн запис на сервіс,
              керування замовленнями
              та повний контроль
              над вашим автомобілем
            </p>
          </div>

          <div className="space-y-4">

            <div className="flex items-center gap-3">
              <ShieldCheck className="h-5 w-5" />
              <span>Безпечна авторизація</span>
            </div>

            <div className="flex items-center gap-3">
              <User className="h-5 w-5" />
              <span>Персональний кабінет</span>
            </div>

            <div className="flex items-center gap-3">
              <LockKeyhole className="h-5 w-5" />
              <span>Захист даних користувача</span>
            </div>

          </div>
        </div>

        {/* RIGHT SIDE */}
        <div className="p-8 md:p-12 flex items-center">

          <div className="w-full">

            {/* HEADER */}
            <div className="mb-8">
              <h2 className="text-4xl font-bold tracking-tight mb-3">
                {isLogin ? "Вхід" : "Реєстрація"}
              </h2>

              <p className="text-muted-foreground text-lg">
                {isLogin
                  ? "Увійдіть у свій акаунт"
                  : "Створіть новий акаунт"}
              </p>
            </div>

            {/* EMAIL VERIFICATION INFO */}
            {awaitingVerification && (
              <div className="mb-5 p-4 rounded-xl bg-yellow-50 text-yellow-700 text-sm border border-yellow-200">
                Ми надіслали лист для підтвердження email.
                Перевір пошту і активуй акаунт перед входом.
              </div>
            )}

            {/* FORM */}
            <Card className="border-0 shadow-none">
              <CardContent className="p-0">

                <form onSubmit={handleSubmit} className="space-y-5">

                  {/* USERNAME */}
                  <div className="space-y-2">
                    <Label>Ім’я користувача</Label>

                    <div className="relative">
                      <User className="absolute left-3 top-3.5 h-4 w-4 text-muted-foreground" />

                      <Input
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                        placeholder="Username"
                        className="pl-10 h-11 rounded-xl"
                      />
                    </div>
                  </div>

                  {/* EMAIL */}
                  {!isLogin && (
                    <div className="space-y-2">
                      <Label>Email</Label>

                      <div className="relative">
                        <Mail className="absolute left-3 top-3.5 h-4 w-4 text-muted-foreground" />

                        <Input
                          type="email"
                          value={email}
                          onChange={(e) => setEmail(e.target.value)}
                          required
                          placeholder="example@email.com"
                          className="pl-10 h-11 rounded-xl"
                        />
                      </div>
                    </div>
                  )}

                  {/* PASSWORD */}
                  <div className="space-y-2">
                    <Label>Пароль</Label>

                    <div className="relative">
                      <LockKeyhole className="absolute left-3 top-3.5 h-4 w-4 text-muted-foreground" />

                      <Input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        placeholder="••••••••"
                        className="pl-10 h-11 rounded-xl"
                      />
                    </div>
                  </div>

                  {/* BUTTON */}
                  <Button
                    type="submit"
                    disabled={loading}
                    className="w-full h-11 rounded-xl bg-orange-500 hover:bg-orange-600 text-white"
                  >
                    {loading
                      ? "Зачекайте..."
                      : isLogin
                      ? "Увійти"
                      : "Зареєструватися"}
                  </Button>

                </form>

                {/* SWITCH */}
                <div className="mt-6 text-center text-sm">

                  <span className="text-muted-foreground">
                    {isLogin
                      ? "Немає акаунта?"
                      : "Вже маєте акаунт?"}{" "}
                  </span>

                  <button
                    type="button"
                    onClick={() => setIsLogin((prev) => !prev)}
                    className="font-semibold text-orange-500 hover:text-orange-600 transition"
                  >
                    {isLogin ? "Зареєструватися" : "Увійти"}
                  </button>

                </div>

              </CardContent>
            </Card>

          </div>
        </div>

      </div>
    </div>
  );
}