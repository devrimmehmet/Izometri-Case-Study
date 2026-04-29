<template>
  <q-page class="q-pa-lg">
    <div class="text-h5 text-weight-bold q-mb-sm">Bildirimler</div>
    <div class="text-grey-5 text-body2 q-mb-lg">
      Bildirim Servisi üzerinden gelen olay kayıtları
    </div>

    <div class="glass-card q-pa-md q-mb-lg">
      <div class="row q-col-gutter-sm items-end">
        <div class="col-12 col-sm-4">
          <q-select v-model="filters.eventType" :options="eventOptions" label="Olay tipi" outlined dark dense clearable emit-value map-options />
        </div>
        <div class="col-12 col-sm-3">
          <q-input v-model="filters.dateFrom" label="Başlangıç" type="date" outlined dark dense />
        </div>
        <div class="col-12 col-sm-3">
          <q-input v-model="filters.dateTo" label="Bitiş" type="date" outlined dark dense />
        </div>
        <div class="col-12 col-sm-2">
          <q-btn flat dense icon="clear" label="Temizle" color="grey-5" @click="clearFilters" />
        </div>
      </div>
    </div>

    <div class="glass-card q-pa-md">
      <q-banner v-if="errorMessage" rounded class="bg-negative text-white q-mb-md">
        {{ errorMessage }}
        <template #action>
          <q-btn flat color="white" icon="refresh" label="Tekrar dene" @click="loadNotifications" />
        </template>
      </q-banner>
      <q-table
        :rows="filteredNotifications"
        :columns="columns"
        :grid="$q.screen.xs"
        row-key="id"
        flat
        dark
        :loading="notification.loading"
        loading-label="Yükleniyor..."
        class="bg-transparent"
      >
        <template #no-data>
          <DataState
            :title="errorMessage ? 'Bildirimler yüklenemedi' : 'Kayıt bulunamadı'"
            :message="errorMessage || 'Seçili filtrelerle eşleşen bildirim yok.'"
            :icon="errorMessage ? 'error_outline' : 'inbox'"
            :color="errorMessage ? 'negative' : 'grey-5'"
            :retry-label="errorMessage ? 'Tekrar dene' : ''"
            dense
            @retry="loadNotifications"
          />
        </template>
        <template #body-cell-eventType="props">
          <q-td :props="props">
            <q-badge :color="eventColor(props.row.eventType)" outline>
              {{ eventLabel(props.row.eventType) }}
            </q-badge>
          </q-td>
        </template>
        <template #body-cell-correlationId="props">
          <q-td :props="props">
            <CopyValue :value="props.row.correlationId" tooltip="Correlation ID kopyala" />
          </q-td>
        </template>
        <template #body-cell-emailStatus="props">
          <q-td :props="props">
            <q-badge :color="emailStatusColor(props.row.emailStatus)" outline>
              {{ emailStatusLabel(props.row.emailStatus) }}
            </q-badge>
          </q-td>
        </template>
        <template #body-cell-sentAt="props">
          <q-td :props="props">{{ formatDateTime(props.row.sentAt) }}</q-td>
        </template>
        <template #body-cell-actions="props">
          <q-td :props="props">
            <q-btn flat dense round icon="visibility" color="primary" @click="openDetail(props.row)">
              <q-tooltip>Detay</q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </div>

    <q-dialog v-model="showDetail">
      <q-card dark class="glass-card detail-dialog">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Bildirim Detayı</div>
        </q-card-section>
        <q-card-section v-if="selected">
          <div class="q-gutter-sm">
            <div class="row"><div class="col-4 text-grey-5">Event ID:</div><div class="col-8 id-wrap"><CopyValue :value="selected.eventId" tooltip="Event ID kopyala" /></div></div>
            <div class="row"><div class="col-4 text-grey-5">Correlation ID:</div><div class="col-8 id-wrap"><CopyValue :value="selected.correlationId" tooltip="Correlation ID kopyala" /></div></div>
            <div class="row"><div class="col-4 text-grey-5">Olay:</div><div class="col-8">{{ eventLabel(selected.eventType) }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Alıcı:</div><div class="col-8">{{ selected.recipient }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">E-posta:</div><div class="col-8">{{ selected.recipientEmail }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Telefon:</div><div class="col-8">{{ selected.recipientPhone || '-' }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Durum:</div><div class="col-8">{{ emailStatusLabel(selected.emailStatus) }}</div></div>
            <div v-if="selected.emailError" class="row"><div class="col-4 text-grey-5">E-posta Hatası:</div><div class="col-8 text-negative">{{ selected.emailError }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Mesaj:</div><div class="col-8">{{ selected.message }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Tarih:</div><div class="col-8">{{ formatDateTime(selected.sentAt) }}</div></div>
          </div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Kapat" color="primary" @click="showDetail = false" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useQuasar } from 'quasar';
import { useNotificationStore } from 'stores/notification';
import CopyValue from 'components/CopyValue.vue';
import DataState from 'components/DataState.vue';
import type { NotificationDto } from 'src/types';
import { formatDateTime } from 'src/utils/tr';
import { notifyApiError } from 'src/utils/errors';

const $q = useQuasar();
const notification = useNotificationStore();

const filters = reactive({
  eventType: null as string | null,
  dateFrom: '',
  dateTo: '',
});

const selected = ref<NotificationDto | null>(null);
const showDetail = ref(false);
const errorMessage = ref('');

const columns = [
  { name: 'eventType', label: 'Olay Türü', field: 'eventType', align: 'left' as const },
  { name: 'message', label: 'Mesaj', field: 'message', align: 'left' as const },
  { name: 'recipientEmail', label: 'Alıcı', field: 'recipientEmail', align: 'left' as const },
  { name: 'correlationId', label: 'Correlation ID', field: 'correlationId', align: 'left' as const },
  { name: 'emailStatus', label: 'E-posta Durumu', field: 'emailStatus', align: 'center' as const },
  { name: 'sentAt', label: 'Tarih', field: 'sentAt', align: 'left' as const },
  { name: 'actions', label: 'İşlemler', field: '', align: 'center' as const },
];

const eventOptions = computed(() => {
  const values = new Set(notification.notifications.map((item) => item.eventType).filter(Boolean));
  return Array.from(values).map((value) => ({ label: eventLabel(value), value }));
});

const filteredNotifications = computed(() => {
  return notification.notifications.filter((item) => {
    if (filters.eventType && item.eventType !== filters.eventType) return false;
    const sentAt = new Date(item.sentAt).getTime();
    if (filters.dateFrom && sentAt < new Date(`${filters.dateFrom}T00:00:00`).getTime()) return false;
    if (filters.dateTo && sentAt > new Date(`${filters.dateTo}T23:59:59`).getTime()) return false;
    return true;
  });
});

function eventColor(type: string): string {
  const t = type?.toLowerCase() || '';
  if (t.includes('created') || t.includes('submitted')) return 'info';
  if (t.includes('requires_admin_approval') || t.includes('admin')) return 'warning';
  if (t.includes('approved')) return 'positive';
  if (t.includes('rejected')) return 'negative';
  return 'grey';
}

function eventLabel(type: string): string {
  const t = type?.toLowerCase() || '';
  if (t.includes('expense.created') || t.includes('created')) return 'Harcama Talebi Oluşturuldu';
  if (t.includes('expense.submitted') || t.includes('submitted')) return 'Harcama Onaya Gönderildi';
  if (t.includes('requires_admin_approval')) return 'Admin Onayı İstendi';
  if (t.includes('expense.approved') || t.includes('approved')) return 'Harcama Onaylandı';
  if (t.includes('expense.rejected') || t.includes('rejected')) return 'Harcama Reddedildi';
  return type;
}

function emailStatusColor(status: string): string {
  switch (status) {
    case 'Sent':
      return 'positive';
    case 'Failed':
      return 'negative';
    case 'Pending':
      return 'warning';
    default:
      return 'grey';
  }
}

function emailStatusLabel(status: string): string {
  switch (status) {
    case 'Sent':
      return 'Gönderildi';
    case 'Failed':
      return 'Başarısız';
    case 'Pending':
      return 'Bekliyor';
    default:
      return status || '-';
  }
}

function openDetail(row: NotificationDto) {
  selected.value = row;
  showDetail.value = true;
}

function clearFilters() {
  filters.eventType = null;
  filters.dateFrom = '';
  filters.dateTo = '';
}

async function loadNotifications() {
  errorMessage.value = '';
  try {
    await notification.fetchNotifications();
  } catch (error) {
    errorMessage.value = 'Backend servisine ulaşılamadı veya yetki reddedildi.';
    notifyApiError($q, error, 'Bildirimler yüklenemedi');
  }
}

onMounted(async () => {
  await loadNotifications();
});
</script>

<style scoped>
.id-text {
  display: inline-block;
  max-width: 150px;
  overflow: hidden;
  text-overflow: ellipsis;
  vertical-align: middle;
  white-space: nowrap;
}

.id-wrap {
  overflow-wrap: anywhere;
}

.detail-dialog {
  width: 620px;
  max-width: 92vw;
}
</style>
