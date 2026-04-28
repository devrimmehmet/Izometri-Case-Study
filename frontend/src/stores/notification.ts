import { defineStore } from 'pinia';
import { notifyApi } from 'src/services/http';
import type { NotificationDto } from 'src/types';

export const useNotificationStore = defineStore('notification', {
  state: () => ({
    notifications: [] as NotificationDto[],
    loading: false,
  }),

  actions: {
    async fetchNotifications() {
      this.loading = true;
      try {
        const { data } = await notifyApi.get<NotificationDto[]>('/notifications');
        this.notifications = data;
      } finally {
        this.loading = false;
      }
    },
  },
});
