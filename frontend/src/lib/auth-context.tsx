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

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  // 🔹 decode fallback (тільки для id)
  const decodeToken = (token: string) => {
    const d: any = jwtDecode(token);

    return {
      id: d["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
      name: d["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
      role: d["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
    };
  };

  // 🔥 ГОЛОВНЕ: завантаження актуального user з бекенду
  const refreshUser = async () => {
    try {
      const res = await api.get("/users/me");

      setUser({
        id: "",
        name: res.data.userName,
        role: user?.role || "User",
      });
    } catch (e) {
      console.error("refreshUser failed", e);
    }
  };

  // 🔹 login
  const login = async (accessToken: string, refreshToken: string) => {
    localStorage.setItem("accessToken", accessToken);
    localStorage.setItem("refreshToken", refreshToken);

    // тимчасово з токена
    const decoded = decodeToken(accessToken);

    setUser(decoded);

    // 🔥 потім оновлюємо з БД
    await refreshUser();
  };

  // 🔹 logout
  const logout = () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    setUser(null);
  };

  // 🔹 init
  useEffect(() => {
    const token = localStorage.getItem("accessToken");

    if (!token) return;

    setUser(decodeToken(token));

    // 🔥 підтягнути актуальні дані
    refreshUser();
  }, []);

  return (
    <AuthContext.Provider value={{ user, login, logout, refreshUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
};