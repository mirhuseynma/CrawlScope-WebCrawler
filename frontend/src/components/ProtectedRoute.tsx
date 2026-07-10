import type { ReactElement } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

type ProtectedRouteProps = {
  children: ReactElement;
  loginPath?: string;
  permission?: string;
};

export function ProtectedRoute({ children, loginPath = "/login", permission }: ProtectedRouteProps) {
  const location = useLocation();
  const { hasPermission, status } = useAuth();

  if (status === "checking") {
    return <div className="route-loading">Loading session...</div>;
  }

  if (status === "guest") {
    return <Navigate to={loginPath} replace state={{ from: location }} />;
  }

  if (permission && !hasPermission(permission)) {
    return <Navigate to={loginPath} replace state={{ from: location, reason: "forbidden" }} />;
  }

  return children;
}
