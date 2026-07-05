import { request } from "./httpClient";

export type AuthUser = {
  userId: string;
  userName: string;
  email: string;
  fullName: string | null;
  roles: string[];
  permissions: string[];
};

export type AuthResponse = AuthUser & {
  token: string;
  expiresAt: string;
  expiresInSeconds: number;
};

export type LoginRequest = {
  emailOrUserName: string;
  password: string;
};

export type RegisterRequest = {
  userName: string;
  email: string;
  password: string;
  fullName?: string;
};

export function login(payload: LoginRequest) {
  return request<AuthResponse>("/api/Auth/login", {
    method: "POST",
    body: payload,
    skipAuth: true,
  });
}

export function register(payload: RegisterRequest) {
  return request<AuthResponse>("/api/Auth/register", {
    method: "POST",
    body: payload,
    skipAuth: true,
  });
}

export function getCurrentUser() {
  return request<AuthUser>("/api/Auth/me");
}
