import { defineStore } from 'pinia';
import { api } from 'src/boot/axios';
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
      const { data } = await api.post<LoginApiResponse>(
        '/auth/login',
        payload,
      );

      this.token = data.accessToken;
      this.userId = data.userId;
      this.email = data.email;
      this.displayName = data.displayName;
      this.tenantId = data.tenantId;
      this.tenantCode = payload.tenantCode;
      this.roles = data.roles as UserRole[];

      localStorage.setItem('jwt', data.accessToken);
      localStorage.setItem('tenantCode', payload.tenantCode);
      api.defaults.headers.common['Authorization'] = `Bearer ${data.accessToken}`;
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
        this.userId = (claims.sub as string) ?? '';
        this.email = (claims.email as string) ?? '';
        this.tenantId = (claims.tenantId as string) ?? '';
        this.tenantCode = localStorage.getItem('tenantCode') ?? '';

        const role = claims.role ?? claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
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
