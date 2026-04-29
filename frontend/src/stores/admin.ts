import { defineStore } from 'pinia';
import { api } from 'src/services/http';
import type {
  CreateUserRequest,
  NotificationDeadLetterDto,
  OutboxDeadLetter,
  SendProbeEmailRequest,
  UpdateRolesRequest,
  UserDto,
} from 'src/types';
import { notifyApi } from 'src/services/http';

export const useAdminStore = defineStore('admin', {
  state: () => ({
    users: [] as UserDto[],
    outboxDeadLetters: [] as OutboxDeadLetter[],
    notificationDeadLetters: [] as NotificationDeadLetterDto[],
    loading: false,
    operationsLoading: false,
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

    async fetchOutboxDeadLetters() {
      this.operationsLoading = true;
      try {
        const { data } = await api.get<OutboxDeadLetter[]>('/admin/outbox/dead-letters');
        this.outboxDeadLetters = data;
      } finally {
        this.operationsLoading = false;
      }
    },

    async fetchNotificationDeadLetters() {
      this.operationsLoading = true;
      try {
        const { data } = await notifyApi.get<NotificationDeadLetterDto[]>(
          '/admin/notifications/dead-letters',
        );
        this.notificationDeadLetters = data;
      } finally {
        this.operationsLoading = false;
      }
    },

    async sendProbeEmail(payload: SendProbeEmailRequest) {
      const { data } = await notifyApi.post('/admin/notifications/probe-email', payload);
      return data;
    },
  },
});
