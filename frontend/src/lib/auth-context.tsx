import { createContext, useContext, useEffect, useState } from "react";
import { jwtDecode } from "jwt-decode";
import api from "../api/api";

type User = {
  id: string;
  name: string;
  role: string;
};

type AuthContextType = {
  user: User | null;
  login: (accessToken: string, refreshToken: string) => Promise<void>;
  logout: () => void;
  refreshUser: () => Promise<void>;
};

const AuthContext = createContext<AuthContextType | null>(null);

// 🔹 decode JWT (fallback)
const decodeToken = (token: string): User => {
  const d: any = jwtDecode(token);

  return {
    id:
      d[
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
      ],
    name:
      d["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
    role:
      d["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
  };
};

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  // 🔥 беремо АКТУАЛЬНОГО юзера з бекенду
  const refreshUser = async () => {
  try {
    const res = await api.get("/users/me");

    setUser(prev => {
      if (!prev) return prev;

      return {
        ...prev,
        name: res.data.userName,
        // ❌ role НЕ чіпаємо
      };
    });
  } catch (e) {}
};

  // 🔹 login
  const login = async (accessToken: string, refreshToken: string) => {
    localStorage.setItem("accessToken", accessToken);
    localStorage.setItem("refreshToken", refreshToken);

    // тимчасово з токена (швидкий UI)
    const decoded = decodeToken(accessToken);
    setUser(decoded);

    // потім оновлюємо з БД (істина)
    await refreshUser();
  };

  // 🔹 logout
  const logout = () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    setUser(null);
  };

  // 🔹 init при старті сайту
  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (!token) return;

    setUser(decodeToken(token));
    refreshUser();
  }, []);

  return (
    <AuthContext.Provider
      value={{ user, login, logout, refreshUser }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx)
    throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
};