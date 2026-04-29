import axios, { AxiosHeaders, type AxiosError, type AxiosInstance } from 'axios';
import type { ProblemDetails } from 'src/types';

export interface ApiErrorDetails {
  message: string;
  status: number | undefined;
  correlationId: string | undefined;
  validationErrors: Record<string, string[]>;
}

const api = axios.create({ baseURL: '/api', timeout: 30000 });
const notifyApi = axios.create({ baseURL: '/notify-api', timeout: 30000 });

function createCorrelationId(): string {
  if (crypto.randomUUID) return crypto.randomUUID();
  return `${Date.now()}-${Math.random().toString(16).slice(2)}`;
}

function readHeader(headers: unknown, name: string): string | undefined {
  if (!headers) return undefined;
  if (headers instanceof AxiosHeaders) {
    const value = headers.get(name);
    return typeof value === 'string' ? value : undefined;
  }
  const record = headers as Record<string, string | string[] | undefined>;
  const value = record[name] ?? record[name.toLowerCase()];
  return Array.isArray(value) ? value[0] : value;
}

export function getCorrelationId(errorOrResponse: unknown): string | undefined {
  const candidate = errorOrResponse as {
    response?: { headers?: unknown };
    config?: { headers?: unknown };
    headers?: unknown;
  };

  return (
    readHeader(candidate.response?.headers, 'X-Correlation-Id') ??
    readHeader(candidate.headers, 'X-Correlation-Id') ??
    readHeader(candidate.config?.headers, 'X-Correlation-Id')
  );
}

export function parseApiError(error: unknown, fallback = 'İşlem tamamlanamadı'): ApiErrorDetails {
  const axiosError = error as AxiosError<ProblemDetails | string | undefined>;
  const status = axiosError.response?.status;
  const correlationId = getCorrelationId(axiosError);
  const data = axiosError.response?.data;
  const problem = typeof data === 'object' && data !== null ? data : undefined;
  const validationErrors = problem?.errors ?? {};

  if (status === 403) {
    return {
      status,
      correlationId,
      validationErrors,
      message: 'Bu işlem için gerekli rol veya yetki bulunmuyor.',
    };
  }

  if (status === 401) {
    return {
      status,
      correlationId,
      validationErrors,
      message: 'Oturum süresi doldu. Lütfen tekrar giriş yapın.',
    };
  }

  if (axiosError.code === 'ECONNABORTED') {
    return {
      status,
      correlationId,
      validationErrors,
      message: 'İstek zaman aşımına uğradı. Backend yanıt vermiyor olabilir.',
    };
  }

  if (!axiosError.response) {
    return {
      status,
      correlationId,
      validationErrors,
      message: 'Backend servisine ulaşılamıyor. Ağ bağlantısını veya servis durumunu kontrol edin.',
    };
  }

  const validationSummary = Object.entries(validationErrors)
    .map(([field, messages]) => `${field}: ${messages.join(', ')}`)
    .join(' | ');

  return {
    status,
    correlationId,
    validationErrors,
    message:
      validationSummary ||
      problem?.detail ||
      problem?.title ||
      (typeof data === 'string' ? data : undefined) ||
      fallback,
  };
}

function clearSession() {
  localStorage.removeItem('jwt');
  localStorage.removeItem('tenantCode');
  localStorage.removeItem('authProvider');
  delete api.defaults.headers.common.Authorization;
  delete notifyApi.defaults.headers.common.Authorization;
}

function configureClient(client: AxiosInstance) {
  client.interceptors.request.use((config) => {
    const token = localStorage.getItem('jwt');
    config.headers = AxiosHeaders.from(config.headers);
    config.headers.set('X-Correlation-Id', createCorrelationId());

    if (token) {
      config.headers.set('Authorization', `Bearer ${token}`);
    }

    return config;
  });

  client.interceptors.response.use(
    (response) => {
      const correlationId = getCorrelationId(response);
      if (correlationId) {
        sessionStorage.setItem('lastCorrelationId', correlationId);
      }
      return response;
    },
    (error) => {
      const correlationId = getCorrelationId(error);
      if (correlationId) {
        sessionStorage.setItem('lastCorrelationId', correlationId);
      }

      if (error?.response?.status === 401) {
        clearSession();
        if (window.location.pathname !== '/login' && !window.location.hash.includes('/login')) {
          window.location.href = '/#/login';
        }
      }

      throw error;
    },
  );
}

configureClient(api);
configureClient(notifyApi);

export { api, notifyApi };
