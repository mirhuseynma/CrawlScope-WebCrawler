import { getAuthToken } from "./authStorage";

export const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5058";

type RequestOptions = {
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  skipAuth?: boolean;
};

type ApiErrorResponse = {
  title?: string;
  detail?: string;
  status?: number;
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
  return (
    typeof value === "object" &&
    value !== null &&
    ("message" in value || "detail" in value || "title" in value || "errors" in value || "statusCode" in value || "status" in value)
  );
}

function toFriendlyFieldName(field: string) {
  if (field === "$" || field.toLowerCase() === "request") {
    return "Request";
  }

  return field
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/^./, (letter) => letter.toUpperCase());
}

function cleanValidationMessage(message: string) {
  const withoutTechnicalPath = message
    .split(" Path: ")[0]
    .replaceAll("'", "")
    .trim();
  const requiredFieldMatch = withoutTechnicalPath.match(/^The (.+) field is required\.$/i);
  const notEmptyMatch = withoutTechnicalPath.match(/^(.+) must not be empty\.$/i);
  const minimumLengthMatch = withoutTechnicalPath.match(/^The length of (.+) must be at least (\d+) characters/i);
  const maximumLengthMatch = withoutTechnicalPath.match(/^The length of (.+) must be (\d+) characters or fewer/i);
  const emailMatch = withoutTechnicalPath.match(/^(.+) is not a valid email address\.$/i);

  if (requiredFieldMatch) {
    return `${toFriendlyFieldName(requiredFieldMatch[1])} is required.`;
  }

  if (notEmptyMatch) {
    return `${toFriendlyFieldName(notEmptyMatch[1])} is required.`;
  }

  if (minimumLengthMatch) {
    return `${toFriendlyFieldName(minimumLengthMatch[1])} must be at least ${minimumLengthMatch[2]} characters.`;
  }

  if (maximumLengthMatch) {
    return `${toFriendlyFieldName(maximumLengthMatch[1])} must be ${maximumLengthMatch[2]} characters or fewer.`;
  }

  if (emailMatch) {
    return `${toFriendlyFieldName(emailMatch[1])} must be a valid email address.`;
  }

  if (withoutTechnicalPath.includes("is invalid after a value")) {
    return "Request format is invalid.";
  }

  return withoutTechnicalPath;
}

function flattenErrors(errors: ApiErrorResponse["errors"]) {
  if (!errors) {
    return [];
  }

  if (Array.isArray(errors)) {
    return errors.map(cleanValidationMessage);
  }

  return Object.entries(errors).flatMap(([field, messages]) =>
    messages.map((message) => {
      const cleanedMessage = cleanValidationMessage(message);

      if (cleanedMessage.toLowerCase().includes(toFriendlyFieldName(field).toLowerCase())) {
        return cleanedMessage;
      }

      return `${toFriendlyFieldName(field)}: ${cleanedMessage}`;
    }),
  );
}

function getStatusMessage(statusCode: number) {
  if (statusCode === 400) {
    return "Please check the submitted information.";
  }

  if (statusCode === 401) {
    return "Please login to continue.";
  }

  if (statusCode === 403) {
    return "You do not have permission to perform this action.";
  }

  if (statusCode === 404) {
    return "The requested item was not found.";
  }

  if (statusCode >= 500) {
    return "Something went wrong on the server. Please try again.";
  }

  return "The request could not be completed.";
}

function formatApiError(error: ApiErrorResponse, statusCode: number) {
  const validationMessages = [...new Set(flattenErrors(error.errors).filter(Boolean))];
  const message = cleanValidationMessage(error.message || error.detail || error.title || getStatusMessage(statusCode)).replace(
    /^User registration failed:\s*/i,
    "",
  );

  if (validationMessages.length === 0) {
    return message;
  }

  return validationMessages.slice(0, 4).join(" ");
}

async function createRequestError(response: Response) {
  const contentType = response.headers.get("content-type") ?? "";

  if (contentType.includes("application/json")) {
    const payload = (await response.json()) as unknown;

    if (isApiErrorResponse(payload)) {
      const statusCode = payload.statusCode ?? payload.status ?? response.status;
      return new ApiRequestError(formatApiError(payload, statusCode), statusCode, payload.traceId);
    }
  }

  const errorText = await response.text();
  return new ApiRequestError(errorText && response.status < 500 ? errorText : getStatusMessage(response.status), response.status);
}

async function executeRequest(path: string, options: RequestOptions = {}) {
  const token = options.skipAuth ? null : getAuthToken();
  const response = await fetch(`${apiBaseUrl}${path}`, {
    method: options.method ?? "GET",
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    },
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  });

  if (!response.ok) {
    if (response.status === 401 && !options.skipAuth) {
      window.dispatchEvent(new Event("crawlscope:unauthorized"));
    }

    throw await createRequestError(response);
  }

  return response;
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const response = await executeRequest(path, options);

  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export async function requestBlob(path: string, options: RequestOptions = {}) {
  const response = await executeRequest(path, options);

  return response.blob();
}

