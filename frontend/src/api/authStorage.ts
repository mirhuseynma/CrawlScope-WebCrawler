const authStorageKey = "crawlscope.auth";

export type StoredAuth = {
  token: string;
  expiresAt: string;
};

export function getStoredAuth(): StoredAuth | null {
  const rawValue = localStorage.getItem(authStorageKey);

  if (!rawValue) {
    return null;
  }

  try {
    const parsed = JSON.parse(rawValue) as StoredAuth;

    if (!parsed.token || !parsed.expiresAt || new Date(parsed.expiresAt).getTime() <= Date.now()) {
      clearStoredAuth();
      return null;
    }

    return parsed;
  } catch {
    clearStoredAuth();
    return null;
  }
}

export function getAuthToken() {
  return getStoredAuth()?.token ?? null;
}

export function setStoredAuth(auth: StoredAuth) {
  localStorage.setItem(authStorageKey, JSON.stringify(auth));
}

export function clearStoredAuth() {
  localStorage.removeItem(authStorageKey);
}
