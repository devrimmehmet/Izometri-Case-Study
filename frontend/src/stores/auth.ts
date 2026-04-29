import { defineStore } from 'pinia';
import axios, { isAxiosError } from 'axios';
import { api, notifyApi } from 'src/services/http';
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

function claimArray(claims: Record<string, unknown>, ...keys: string[]): string[] {
  for (const key of keys) {
    const value = claims[key];
    if (Array.isArray(value)) {
      return value.filter((item): item is string => typeof item === 'string');
    }
    if (typeof value === 'string') {
      return [value];
    }
  }

  return [];
}

function isKeycloakAccessToken(claims: Record<string, unknown>): boolean {
  const issuer = claimString(claims, 'iss');
  const audiences = claimArray(claims, 'aud');

  return issuer.includes('/realms/izometri') && audiences.includes('expense-service');
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
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    isAdmin: (state) => state.roles.includes('Admin'),
    isHR: (state) => state.roles.includes('HR'),
    isPersonel: (state) => state.roles.includes('Personel'),
    canApprove: (state) =>
      state.roles.includes('HR') || state.roles.includes('Admin'),
    displayRole: (state) => {
      if (state.roles.includes('Admin')) return 'Admin';
      if (state.roles.includes('HR')) return 'HR';
      return 'Personel';
    },
  },

  actions: {
    async login(payload: LoginPayload): Promise<void> {
      let accessToken: string;
      let fallbackData: LoginApiResponse | null = null;
      let authProvider = 'keycloak';

      try {
        accessToken = await loginWithKeycloak(payload.email, payload.password);
      } catch (keycloakError) {
        if (isAxiosError(keycloakError) && keycloakError.response) {
          throw keycloakError;
        }

        authProvider = 'local';
        const { data } = await api.post<LoginApiResponse>('/auth/login', payload);
        accessToken = data.accessToken;
        fallbackData = data;
      }

      const claims = parseJwtClaims(accessToken);
      const roles = claimArray(claims, 'role', 'roles', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role') as UserRole[];

      this.token = accessToken;
      this.userId = claimString(claims, 'UserId', 'sub') || fallbackData?.userId || '';
      this.email = claimString(claims, 'email') || fallbackData?.email || payload.email;
      this.displayName = String(
        claimString(claims, 'name', 'preferred_username') || fallbackData?.displayName || payload.email,
      );
      this.tenantId = claimString(claims, 'TenantId', 'tenantId') || fallbackData?.tenantId || '';
      this.tenantCode = payload.tenantCode;
      this.roles = roles.length > 0 ? roles : (fallbackData?.roles as UserRole[] | undefined) ?? [];

      localStorage.setItem('jwt', accessToken);
      localStorage.setItem('tenantCode', payload.tenantCode);
      localStorage.setItem('authProvider', authProvider);
      api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
      notifyApi.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;
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
      localStorage.removeItem('authProvider');
      delete api.defaults.headers.common['Authorization'];
      delete notifyApi.defaults.headers.common['Authorization'];
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

        const authProvider = localStorage.getItem('authProvider');
        if (authProvider !== 'local' && !isKeycloakAccessToken(claims)) {
          this.logout();
          return false;
        }

        this.token = token;
        this.userId = claimString(claims, 'UserId', 'sub');
        this.email = claimString(claims, 'email');
        this.displayName = claimString(claims, 'name', 'preferred_username', 'email');
        this.tenantId = claimString(claims, 'TenantId', 'tenantId');
        this.tenantCode = localStorage.getItem('tenantCode') ?? '';
        this.roles = claimArray(
          claims,
          'role',
          'roles',
          'http://schemas.microsoft.com/ws/2008/06/identity/claims/role',
        ) as UserRole[];

        api.defaults.headers.common['Authorization'] = `Bearer ${token}`;
        notifyApi.defaults.headers.common['Authorization'] = `Bearer ${token}`;
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
    client_id: 'izometri-spa',
    grant_type: 'password',
    username: email,
    password,
  });

  const { data } = await axios.post<KeycloakTokenResponse>(keycloakTokenUrl, body, {
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
  });

  return data.access_token;
}
