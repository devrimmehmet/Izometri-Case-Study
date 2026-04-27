import { defineStore } from 'pinia';
import { api } from 'src/services/http';
import type { UserDto, CreateUserRequest, UpdateRolesRequest } from 'src/types';

export const useAdminStore = defineStore('admin', {
  state: () => ({
    users: [] as UserDto[],
    loading: false,
  }),

  actions: {
    async fetchUsers() {
      this.loading = true;
      try {
        const { data } = await api.get<UserDto[]>('/admin/users');
        this.users = data;
      } finally {
        this.loading = false;
      }
    },

    async createUser(payload: CreateUserRequest): Promise<UserDto> {
      const { data } = await api.post<UserDto>('/admin/users', payload);
      await this.fetchUsers();
      return data;
    },

    async updateRoles(userId: string, payload: UpdateRolesRequest) {
      await api.put(`/admin/users/${userId}/roles`, payload);
      await this.fetchUsers();
    },
  },
});
