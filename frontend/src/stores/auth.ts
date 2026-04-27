import { defineStore } from 'pinia';
import axios, { isAxiosError } from 'axios';
import { api } from 'src/services/http';
import type { UserRole } from 'src/types';

interface LoginPayload {
  email: string;
  password: string;
  tenantCode: string;
}

interface LoginApiResponse {
  accessToken: string;
  userId: string;
  tenantId: string;
  email: string;
  displayName: string;
  roles: string[];
}

interface KeycloakTokenResponse {
  access_token: string;
}

const keycloakTokenUrl =
  import.meta.env.VITE_KEYCLOAK_TOKEN_URL ??
  'http://localhost:18080/realms/izometri/protocol/openid-connect/token';

function parseJwtClaims(token: string): Record<string, unknown> {
  try {
    const base64 = token.split('.')[1];
    if (!base64) return {};
    const json = atob(base64.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return {};
  }
}

function claimString(claims: Record<string, unknown>, ...keys: string[]): string {
  for (const key of keys) {
    const value = claims[key];
    if (typeof value === 'string') {
      return value;
    }
  }

  return '';
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: null as string | null,
    userId: '',
    email: '',
    displayName: '',
    tenantId: '',
    tenantCode: '',
    roles: [] as UserRole[],
    gateUnlocked: false,
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.roles.includes('Admin'),
    isHR: (state) => state.roles.includes('HR'),
    isPersonnel: (state) => state.roles.includes('Personnel'),
    canApprove: (state) =>
      state.roles.includes('HR') || state.roles.includes('Admin'),
    displayRole: (state) => {
      if (state.roles.includes('Admin')) return 'Admin';
      if (state.roles.includes('HR')) return 'HR';
      return 'Personnel';
    },
  },

  actions: {
    unlockGate() {
      this.gateUnlocked = true;
      sessionStorage.setItem('gate', '1');
    },

    checkGate(): boolean {
      this.gateUnlocked = sessionStorage.getItem('gate') === '1';
      return this.gateUnlocked;
    },

    async login(payload: LoginPayload): Promise<void> {
      let accessToken: string;
      let fallbackData: LoginApiResponse | null = null;

      try {
        const { data } = await api.post<LoginApiResponse>('/auth/login', payload);
        accessToken = data.accessToken;
        fallbackData = data;
      } catch (error) {
        if (!isAxiosError(error) || error.response?.status !== 404) {
          throw error;
        }

        accessToken = await loginWithKeycloak(payload.email, payload.password);
      }

      const claims = parseJwtClaims(accessToken);
      const role = claims.role ?? claims.roles;
      const roles = Array.isArray(role)
        ? (role as UserRole[])
        : role
          ? ([role] as UserRole[])
          : (fallbackData?.roles as UserRole[] | undefined) ?? [];

      this.token = accessToken;
      this.userId = claimString(claims, 'UserId', 'sub') || fallbackData?.userId || '';
      this.email = claimString(claims, 'email') || fallbackData?.email || payload.email;
      this.displayName = String(
        claimString(claims, 'name', 'preferred_username') || fallbackData?.displayName || payload.email,
      );
      this.tenantId = claimString(claims, 'TenantId', 'tenantId') || fallbackData?.tenantId || '';
      this.tenantCode = payload.tenantCode;
      this.roles = roles;

      localStorage.setItem('jwt', accessToken);
      localStorage.setItem('tenantCode', payload.tenantCode);
      api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
    },

    logout() {
      this.token = null;
      this.userId = '';
      this.email = '';
      this.displayName = '';
      this.tenantId = '';
      this.tenantCode = '';
      this.roles = [];
      localStorage.removeItem('jwt');
      localStorage.removeItem('tenantCode');
      delete api.defaults.headers.common['Authorization'];
    },

    restoreSession(): boolean {
      const token = localStorage.getItem('jwt');
      if (!token) return false;

      try {
        const claims = parseJwtClaims(token);
        const exp = claims.exp as number | undefined;
        if (exp && exp * 1000 < Date.now()) {
          this.logout();
          return false;
        }

        this.token = token;
        this.userId = claimString(claims, 'UserId', 'sub');
        this.email = claimString(claims, 'email');
        this.displayName = claimString(claims, 'name', 'preferred_username', 'email');
        this.tenantId = claimString(claims, 'TenantId', 'tenantId');
        this.tenantCode = localStorage.getItem('tenantCode') ?? '';

        const role = claims.role ?? claims.roles ?? claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
        this.roles = Array.isArray(role)
          ? (role as UserRole[])
          : role
            ? ([role] as UserRole[])
            : [];

        api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        return true;
      } catch {
        this.logout();
        return false;
      }
    },
  },
});

async function loginWithKeycloak(email: string, password: string): Promise<string> {
  const body = new URLSearchParams({
    client_id: 'expense-service',
    client_secret: 'expense-service-client-secret',
    grant_type: 'password',
    username: email,
    password,
  });

  const { data } = await axios.post<KeycloakTokenResponse>(keycloakTokenUrl, body, {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  });

  return data.access_token;
}
