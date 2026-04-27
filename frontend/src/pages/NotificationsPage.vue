<template>
  <q-page class="q-pa-lg">
    <div class="text-h5 text-weight-bold q-mb-sm">Bildirimler</div>
    <div class="text-grey-5 text-body2 q-mb-lg">
      Bildirim Servisi üzerinden gelen olay kayıtları
    </div>

    <div class="glass-card q-pa-md">
      <q-table
        :rows="notification.notifications"
        :columns="columns"
        row-key="id"
        flat
        dark
        :loading="notification.loading"
        class="bg-transparent"
        no-data-label="Kayıt bulunamadı"
      >
        <template #body-cell-eventType="props">
          <q-td :props="props">
            <q-badge :color="eventColor(props.row.eventType)" outline>
              {{ eventLabel(props.row.eventType) }}
            </q-badge>
          </q-td>
        </template>
        <template #body-cell-emailStatus="props">
          <q-td :props="props">
            <q-badge
              v-if="props.row.emailStatus"
              :color="props.row.emailStatus === 'Sent' ? 'positive' : 'warning'"
              outline
            >
              {{ emailStatusLabel(props.row.emailStatus) }}
            </q-badge>
            <span v-else class="text-grey-6">—</span>
          </q-td>
        </template>
        <template #body-cell-sentAt="props">
          <q-td :props="props">
            {{ formatDateTime(props.row.sentAt) }}
          </q-td>
        </template>
      </q-table>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useNotificationStore } from 'stores/notification';
import { formatDateTime } from 'src/utils/tr';

const notification = useNotificationStore();

const columns = [
  { name: 'eventType', label: 'Olay Türü', field: 'eventType', align: 'left' as const },
  { name: 'message', label: 'Mesaj', field: 'message', align: 'left' as const },
  { name: 'recipientEmail', label: 'Alıcı', field: 'recipientEmail', align: 'left' as const },
  { name: 'emailStatus', label: 'E-posta Durumu', field: 'emailStatus', align: 'center' as const },
  { name: 'sentAt', label: 'Tarih', field: 'sentAt', align: 'left' as const },
];

function eventColor(type: string): string {
  const t = type?.toLowerCase() || '';
  if (t.includes('created')) return 'info';
  if (t.includes('requires_admin_approval')) return 'warning';
  if (t.includes('approved')) return 'positive';
  if (t.includes('rejected')) return 'negative';
  return 'grey';
}

function eventLabel(type: string): string {
  const t = type?.toLowerCase() || '';
  if (t.includes('created')) return 'Harcama Talebi Oluşturuldu';
  if (t.includes('requires_admin_approval')) return 'Yönetici Onayı İstendi';
  if (t.includes('approved')) return 'Harcama Onaylandı';
  if (t.includes('rejected')) return 'Harcama Reddedildi';
  return type;
}

function emailStatusLabel(status: string): string {
  switch (status) {
    case 'Sent': return 'Gönderildi';
    case 'Failed': return 'Başarısız';
    case 'Pending': return 'Bekliyor';
    default: return status;
  }
}

onMounted(async () => {
  await notification.fetchNotifications();
});
</script>
