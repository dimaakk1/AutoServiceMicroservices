import { createContext, useContext, useEffect, useState } from "react";
import { jwtDecode } from "jwt-decode";

type User = {
  id: string;
  name: string;
  role: string;
};

type AuthContextType = {
  user: User | null;
  login: (accessToken: string, refreshToken: string) => void;
  logout: () => void;
};

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);

  const decode = (token: string): User => {
    const d: any = jwtDecode(token);

    return {
      id: d["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"],
      name: d["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
      role: d["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
    };
  };

  const login = (accessToken: string, refreshToken: string) => {
    localStorage.setItem("accessToken", accessToken);
    localStorage.setItem("refreshToken", refreshToken);

    setUser(decode(accessToken));
  };

  const logout = () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    setUser(null);
  };

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (token) setUser(decode(token));
  }, []);

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
  return ctx;
};