import { createContext, ReactNode, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { getCurrentUser, login, register, type AuthUser, type LoginRequest, type RegisterRequest } from "../api/authApi";
import { clearStoredAuth, getStoredAuth, setStoredAuth } from "../api/authStorage";

type AuthStatus = "checking" | "authenticated" | "guest";

type AuthContextValue = {
  status: AuthStatus;
  user: AuthUser | null;
  loginUser: (payload: LoginRequest) => Promise<void>;
  registerUser: (payload: RegisterRequest) => Promise<void>;
  logout: () => void;
  hasPermission: (permission: string) => boolean;
};

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

type AuthProviderProps = {
  children: ReactNode;
};

export function AuthProvider({ children }: AuthProviderProps) {
  const [status, setStatus] = useState<AuthStatus>("checking");
  const [user, setUser] = useState<AuthUser | null>(null);

  const logout = useCallback(() => {
    clearStoredAuth();
    setUser(null);
    setStatus("guest");
  }, []);

  const hydrateUser = useCallback(async () => {
    const storedAuth = getStoredAuth();

    if (!storedAuth) {
      setStatus("guest");
      return;
    }

    try {
      const currentUser = await getCurrentUser();
      setUser(currentUser);
      setStatus("authenticated");
    } catch {
      logout();
    }
  }, [logout]);

  useEffect(() => {
    void hydrateUser();
  }, [hydrateUser]);

  useEffect(() => {
    function handleUnauthorized() {
      logout();
    }

    window.addEventListener("crawlscope:unauthorized", handleUnauthorized);
    return () => window.removeEventListener("crawlscope:unauthorized", handleUnauthorized);
  }, [logout]);

  const loginUser = useCallback(async (payload: LoginRequest) => {
    const response = await login(payload);
    setStoredAuth({
      token: response.token,
      expiresAt: response.expiresAt,
    });
    setUser({
      userId: response.userId,
      userName: response.userName,
      email: response.email,
      fullName: response.fullName,
      roles: response.roles,
      permissions: response.permissions,
    });
    setStatus("authenticated");
  }, []);

  const registerUser = useCallback(async (payload: RegisterRequest) => {
    const response = await register(payload);
    setStoredAuth({
      token: response.token,
      expiresAt: response.expiresAt,
    });
    setUser({
      userId: response.userId,
      userName: response.userName,
      email: response.email,
      fullName: response.fullName,
      roles: response.roles,
      permissions: response.permissions,
    });
    setStatus("authenticated");
  }, []);

  const hasPermission = useCallback(
    (permission: string) => user?.permissions.includes(permission) ?? false,
    [user],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      user,
      loginUser,
      registerUser,
      logout,
      hasPermission,
    }),
    [hasPermission, loginUser, logout, registerUser, status, user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return context;
}
