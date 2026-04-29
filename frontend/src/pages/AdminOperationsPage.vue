<template>
  <q-page class="q-pa-lg">
    <div class="row items-center q-mb-lg">
      <div>
        <div class="text-h5 text-weight-bold">Admin Operasyonları</div>
        <div class="text-grey-5 text-body2">Outbox, bildirim dead-letter ve probe email işlemleri</div>
      </div>
      <q-space />
      <q-btn color="primary" icon="refresh" label="Yenile" rounded unelevated @click="loadAll" />
    </div>

    <div class="row q-col-gutter-md q-mb-md">
      <div v-if="errorMessage" class="col-12">
        <q-banner rounded class="bg-negative text-white">
          {{ errorMessage }}
          <template #action>
            <q-btn flat color="white" icon="refresh" label="Tekrar dene" @click="loadAll" />
          </template>
        </q-banner>
      </div>
      <div class="col-12 col-lg-6">
        <q-card dark class="glass-card">
          <q-card-section>
            <div class="text-h6">Outbox Dead-Letter</div>
          </q-card-section>
          <q-card-section>
            <q-table
              :rows="admin.outboxDeadLetters"
              :columns="outboxColumns"
              :grid="$q.screen.xs"
              row-key="id"
              flat
              dark
              dense
              :loading="admin.operationsLoading"
              loading-label="Yükleniyor..."
              class="bg-transparent"
            >
              <template #no-data>
                <DataState
                  :title="errorMessage ? 'Outbox kayıtları yüklenemedi' : 'Kayıt bulunamadı'"
                  :message="errorMessage || 'Outbox dead-letter kaydı yok.'"
                  :icon="errorMessage ? 'error_outline' : 'inbox'"
                  :color="errorMessage ? 'negative' : 'grey-5'"
                  :retry-label="errorMessage ? 'Tekrar dene' : ''"
                  dense
                  @retry="loadAll"
                />
              </template>
              <template #body-cell-correlationId="props">
                <q-td :props="props">
                  <CopyValue :value="props.row.correlationId" tooltip="Correlation ID kopyala" />
                </q-td>
              </template>
              <template #body-cell-createdAt="props">
                <q-td :props="props">{{ formatDateTime(props.row.deadLetteredAt || props.row.createdAt) }}</q-td>
              </template>
            </q-table>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-lg-6">
        <q-card dark class="glass-card">
          <q-card-section>
            <div class="text-h6">Notification Dead-Letter</div>
          </q-card-section>
          <q-card-section>
            <q-table
              :rows="admin.notificationDeadLetters"
              :columns="notificationColumns"
              :grid="$q.screen.xs"
              row-key="id"
              flat
              dark
              dense
              :loading="admin.operationsLoading"
              loading-label="Yükleniyor..."
              class="bg-transparent"
            >
              <template #no-data>
                <DataState
                  :title="errorMessage ? 'Notification kayıtları yüklenemedi' : 'Kayıt bulunamadı'"
                  :message="errorMessage || 'Notification dead-letter kaydı yok.'"
                  :icon="errorMessage ? 'error_outline' : 'inbox'"
                  :color="errorMessage ? 'negative' : 'grey-5'"
                  :retry-label="errorMessage ? 'Tekrar dene' : ''"
                  dense
                  @retry="loadAll"
                />
              </template>
              <template #body-cell-eventId="props">
                <q-td :props="props">
                  <CopyValue :value="props.row.eventId" tooltip="Event ID kopyala" />
                </q-td>
              </template>
              <template #body-cell-expenseId="props">
                <q-td :props="props">
                  <CopyValue :value="props.row.expenseId" tooltip="Expense ID kopyala" />
                </q-td>
              </template>
              <template #body-cell-correlationId="props">
                <q-td :props="props">
                  <CopyValue :value="props.row.correlationId" tooltip="Correlation ID kopyala" />
                </q-td>
              </template>
              <template #body-cell-deadLetteredAt="props">
                <q-td :props="props">{{ formatDateTime(props.row.deadLetteredAt || props.row.createdAt) }}</q-td>
              </template>
            </q-table>
          </q-card-section>
        </q-card>
      </div>
    </div>

    <q-card dark class="glass-card">
      <q-card-section>
        <div class="text-h6">Probe Email</div>
        <div class="text-caption text-grey-5">NotificationService üzerinden test e-postası gönderir.</div>
      </q-card-section>
      <q-card-section>
        <q-form class="row q-col-gutter-sm items-start" @submit.prevent="sendProbeEmail">
          <div class="col-12 col-md-4">
            <q-input v-model="probe.toEmail" label="Alıcı e-posta" type="email" outlined dark dense :rules="emailRules" />
          </div>
          <div class="col-12 col-md-3">
            <q-input v-model="probe.subject" label="Konu" outlined dark dense :rules="[(v: string) => !!v || 'Konu gerekli']" />
          </div>
          <div class="col-12 col-md-4">
            <q-input v-model="probe.body" label="Mesaj" outlined dark dense :rules="[(v: string) => !!v || 'Mesaj gerekli']" />
          </div>
          <div class="col-12 col-md-1">
            <q-btn type="submit" color="primary" icon="send" label="Gönder" :loading="sendingProbe" unelevated />
          </div>
        </q-form>
      </q-card-section>
    </q-card>
  </q-page>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { useQuasar } from 'quasar';
import { useAdminStore } from 'stores/admin';
import CopyValue from 'components/CopyValue.vue';
import DataState from 'components/DataState.vue';
import { formatDateTime } from 'src/utils/tr';
import { notifyApiError } from 'src/utils/errors';

const $q = useQuasar();
const admin = useAdminStore();
const sendingProbe = ref(false);
const errorMessage = ref('');

const probe = reactive({
  toEmail: 'devrimmehmet@gmail.com',
  subject: 'Izometri probe email',
  body: 'Frontend probe email testi.',
});

const emailRules = [
  (v: string) => !!v || 'E-posta gerekli',
  (v: string) => /.+@.+\..+/.test(v) || 'Geçerli bir e-posta girin',
];

const outboxColumns = [
  { name: 'eventType', label: 'Event Type', field: 'eventType', align: 'left' as const },
  { name: 'routingKey', label: 'Routing Key', field: 'routingKey', align: 'left' as const },
  { name: 'retryCount', label: 'Retry', field: 'retryCount', align: 'right' as const },
  { name: 'error', label: 'Hata', field: 'error', align: 'left' as const },
  { name: 'correlationId', label: 'Correlation ID', field: 'correlationId', align: 'left' as const },
  { name: 'createdAt', label: 'Tarih', field: 'createdAt', align: 'left' as const },
];

const notificationColumns = [
  { name: 'eventId', label: 'Event ID', field: 'eventId', align: 'left' as const },
  { name: 'expenseId', label: 'Expense ID', field: 'expenseId', align: 'left' as const },
  { name: 'routingKey', label: 'Routing Key', field: 'routingKey', align: 'left' as const },
  { name: 'error', label: 'Hata', field: 'error', align: 'left' as const },
  { name: 'retryCount', label: 'Retry', field: 'retryCount', align: 'right' as const },
  { name: 'correlationId', label: 'Correlation ID', field: 'correlationId', align: 'left' as const },
  { name: 'deadLetteredAt', label: 'Tarih', field: 'deadLetteredAt', align: 'left' as const },
];

async function loadAll() {
  errorMessage.value = '';
  try {
    await Promise.all([admin.fetchOutboxDeadLetters(), admin.fetchNotificationDeadLetters()]);
  } catch (error) {
    errorMessage.value = 'Admin operasyon verileri yüklenemedi.';
    notifyApiError($q, error, 'Operasyon verileri yüklenemedi');
  }
}

async function sendProbeEmail() {
  sendingProbe.value = true;
  try {
    await admin.sendProbeEmail(probe);
    $q.notify({
      type: 'positive',
      message: 'Probe email gönderildi. Mailpit üzerinde kontrol edin: http://localhost:8025',
    });
  } catch (error) {
    notifyApiError($q, error, 'Probe email gönderilemedi');
  } finally {
    sendingProbe.value = false;
  }
}

onMounted(() => {
  void loadAll();
});
</script>

<style scoped>
</style>
