import { useAuth } from "../lib/auth-context";
import { Navigate } from "react-router-dom";

export default function ProtectedRoute({
  children,
  role,
}: {
  children: React.ReactElement;
  role?: string;
}) {
  const { user } = useAuth();

  if (!user) return <Navigate to="/auth" />;

  if (role && user.role !== role) {
    return <Navigate to="/" />;
  }

  return children;
}