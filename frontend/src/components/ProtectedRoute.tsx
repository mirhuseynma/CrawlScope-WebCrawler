import type { ReactElement } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

type ProtectedRouteProps = {
  children: ReactElement;
  loginPath?: string;
  permission?: string;
  permissions?: string[];
};

export function ProtectedRoute({ children, loginPath = "/login", permission, permissions = [] }: ProtectedRouteProps) {
  const location = useLocation();
  const { hasPermission, status } = useAuth();
  const requiredPermissions = permission ? [permission, ...permissions] : permissions;

  if (status === "checking") {
    return <div className="route-loading">Loading session...</div>;
  }

  if (status === "guest") {
    return <Navigate to={loginPath} replace state={{ from: location }} />;
  }

  if (requiredPermissions.some((requiredPermission) => !hasPermission(requiredPermission))) {
    return <Navigate to={loginPath} replace state={{ from: location, reason: "forbidden" }} />;
  }

  return children;
}
