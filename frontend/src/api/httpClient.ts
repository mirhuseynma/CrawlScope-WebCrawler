export const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5058";

type RequestOptions = {
  method?: "GET" | "POST" | "PATCH" | "DELETE";
  body?: unknown;
};

type ApiErrorResponse = {
  statusCode?: number;
  message?: string;
  errors?: Record<string, string[]> | string[];
  traceId?: string;
};

export class ApiRequestError extends Error {
  constructor(
    message: string,
    public readonly statusCode: number,
    public readonly traceId?: string,
  ) {
    super(message);
    this.name = "ApiRequestError";
  }
}

function isApiErrorResponse(value: unknown): value is ApiErrorResponse {
  return typeof value === "object" && value !== null && ("message" in value || "errors" in value || "statusCode" in value);
}

function flattenErrors(errors: ApiErrorResponse["errors"]) {
  if (!errors) {
    return [];
  }

  if (Array.isArray(errors)) {
    return errors;
  }

  return Object.values(errors).flat();
}

function formatApiError(error: ApiErrorResponse, fallbackMessage: string) {
  const validationMessages = flattenErrors(error.errors).filter(Boolean);
  const message = error.message || fallbackMessage;

  if (validationMessages.length === 0) {
    return message;
  }

  return `${message} ${validationMessages.slice(0, 3).join(" ")}`;
}

async function createRequestError(response: Response) {
  const fallbackMessage = `Request failed with status ${response.status}`;
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    const payload = (await response.json()) as unknown;

    if (isApiErrorResponse(payload)) {
      return new ApiRequestError(formatApiError(payload, fallbackMessage), payload.statusCode ?? response.status, payload.traceId);
    }
  }

  const errorText = await response.text();
  return new ApiRequestError(errorText || fallbackMessage, response.status);
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: options.method ?? "GET",
    headers: {
      "Content-Type": "application/json",
    },
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  });

  if (!response.ok) {
    throw await createRequestError(response);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export async function requestBlob(path: string, options: RequestOptions = {}) {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: options.method ?? "GET",
    headers: {
      "Content-Type": "application/json",
    },
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  });

  if (!response.ok) {
    throw await createRequestError(response);
  }

  return response.blob();
}

