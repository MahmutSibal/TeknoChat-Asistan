import type { ApiErrorBody } from "../types/api";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL as string;

export class ApiError extends Error {
  status: number;
  body: ApiErrorBody | null;

  constructor(status: number, message: string, body: ApiErrorBody | null) {
    super(message);
    this.status = status;
    this.body = body;
  }
}

let authToken: string | null = localStorage.getItem("token");

export function setAuthToken(token: string | null) {
  authToken = token;
  if (token) {
    localStorage.setItem("token", token);
  } else {
    localStorage.removeItem("token");
  }
}

export function getAuthToken(): string | null {
  return authToken;
}

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  query?: Record<string, string | number | boolean | undefined | null>;
  isFormData?: boolean;
}

function buildQueryString(query?: RequestOptions["query"]): string {
  if (!query) return "";
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null) {
      params.append(key, String(value));
    }
  }
  const qs = params.toString();
  return qs ? `?${qs}` : "";
}

async function extractErrorMessage(response: Response): Promise<{ message: string; body: ApiErrorBody | null }> {
  try {
    const body = (await response.json()) as ApiErrorBody;
    const message =
      body.message ??
      body.title ??
      body.detail ??
      (body.errors ? Object.values(body.errors).flat().join(" ") : null) ??
      `İstek başarısız oldu (${response.status}).`;
    return { message, body };
  } catch {
    return { message: `İstek başarısız oldu (${response.status}).`, body: null };
  }
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, query, isFormData } = options;

  const headers: HeadersInit = {};
  if (authToken) {
    headers["Authorization"] = `Bearer ${authToken}`;
  }
  if (body && !isFormData) {
    headers["Content-Type"] = "application/json";
  }

  const response = await fetch(`${API_BASE_URL}${path}${buildQueryString(query)}`, {
    method,
    headers,
    body: body ? (isFormData ? (body as FormData) : JSON.stringify(body)) : undefined,
  });

  if (response.status === 401) {
    setAuthToken(null);
    window.location.href = "/login";
    throw new ApiError(401, "Oturum süresi doldu, lütfen tekrar giriş yapın.", null);
  }

  if (!response.ok) {
    const { message, body: errorBody } = await extractErrorMessage(response);
    throw new ApiError(response.status, message, errorBody);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  const text = await response.text();
  return text ? (JSON.parse(text) as T) : (undefined as T);
}

export const apiClient = {
  async get<T>(path: string, query?: Record<string, string | number | boolean | undefined | null>): Promise<T> {
    return apiRequest<T>(path, { method: "GET", query });
  },

  async post<T>(path: string, body?: unknown, query?: Record<string, string | number | boolean | undefined | null>): Promise<T> {
    return apiRequest<T>(path, { method: "POST", body, query });
  },

  async put<T>(path: string, body?: unknown, query?: Record<string, string | number | boolean | undefined | null>): Promise<T> {
    return apiRequest<T>(path, { method: "PUT", body, query });
  },

  async delete<T>(path: string, query?: Record<string, string | number | boolean | undefined | null>): Promise<T> {
    return apiRequest<T>(path, { method: "DELETE", query });
  },
};
