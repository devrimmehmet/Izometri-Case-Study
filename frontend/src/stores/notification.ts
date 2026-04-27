import { defineStore } from 'pinia';
import { notifyApi } from 'src/boot/axios';
import { useAuthStore } from './auth';
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
        const auth = useAuthStore();
        const { data } = await notifyApi.get<NotificationDto[]>(
          `/notifications?tenantId=${auth.tenantId}`,
        );
        this.notifications = data;
      } finally {
        this.loading = false;
      }
    },
  },
});
