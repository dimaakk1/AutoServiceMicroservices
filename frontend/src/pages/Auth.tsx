import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { loginUser, registerUser } from "../api/auth";
import { useAuth } from "../lib/auth-context";

import { Button } from "../components/ui/button";
import { Input } from "../components/ui/input";
import { Label } from "../components/ui/label";
import { Card, CardContent, CardHeader, CardTitle } from "../components/ui/card";
import { toast } from "sonner";

export default function Auth() {
  const [isLogin, setIsLogin] = useState(true);

  const [username, setUsername] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const [loading, setLoading] = useState(false);

  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();

    if (loading) return;
    setLoading(true);

    try {
      // =====================
      // LOGIN
      // =====================
      if (isLogin) {
        const res = await loginUser(username, password);

        const accessToken = res.data?.accessToken;
        const refreshToken = res.data?.refreshToken;

        if (!accessToken || !refreshToken) {
          toast.error("Сервер не повернув токени");
          return;
        }

        // 🔥 через context
        login(accessToken, refreshToken);

        toast.success("Вхід виконано успішно!");

        setUsername("");
        setPassword("");

        navigate("/");
      }

      // =====================
      // REGISTER
      // =====================
      else {
        await registerUser(username, email, password);

        toast.success("Реєстрація успішна!");

        setIsLogin(true);

        setUsername("");
        setEmail("");
        setPassword("");
      }
    } catch (err: any) {
      console.error(err);

      const message =
        err?.response?.data?.message ||
        err?.response?.data ||
        "Помилка авторизації";

      toast.error(message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-[80vh] flex items-center justify-center py-12 px-4">
      <Card className="w-full max-w-md">
        
        <CardHeader className="text-center">
          <CardTitle className="text-2xl font-display">
            {isLogin ? "Вхід" : "Реєстрація"}
          </CardTitle>
        </CardHeader>

        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">

            {/* USERNAME */}
            <div className="space-y-2">
              <Label>Username</Label>
              <Input
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                placeholder="Username"
              />
            </div>

            {/* EMAIL (тільки register) */}
            {!isLogin && (
              <div className="space-y-2">
                <Label>Email</Label>
                <Input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                  placeholder="email@example.com"
                />
              </div>
            )}

            {/* PASSWORD */}
            <div className="space-y-2">
              <Label>Пароль</Label>
              <Input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                placeholder="••••••••"
              />
            </div>

            {/* BUTTON */}
            <Button
              type="submit"
              className="w-full bg-accent text-accent-foreground hover:bg-accent/90"
              disabled={loading}
            >
              {loading
                ? "Зачекайте..."
                : isLogin
                ? "Увійти"
                : "Зареєструватися"}
            </Button>

          </form>

          {/* SWITCH MODE */}
          <div className="mt-4 text-center text-sm">
            <span className="text-muted-foreground">
              {isLogin ? "Немає акаунту?" : "Вже маєте акаунт?"}{" "}
            </span>

            <button
              type="button"
              onClick={() => setIsLogin(prev => !prev)}
              className="text-accent hover:underline font-medium"
            >
              {isLogin ? "Зареєструватися" : "Увійти"}
            </button>
          </div>

        </CardContent>
      </Card>
    </div>
  );
}