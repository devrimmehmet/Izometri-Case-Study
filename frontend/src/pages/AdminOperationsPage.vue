<template>
  <q-page class="q-pa-lg">
    <div class="row items-center q-mb-lg">
      <div>
        <div class="text-h5 text-weight-bold">Admin Operasyonları</div>
        <div class="text-grey-5 text-body2">Hata kayıtları, dead-letter takibi ve servis deneme işlemleri</div>
      </div>
      <q-space />
      <q-btn color="primary" icon="refresh" label="Yenile" rounded unelevated @click="loadAll" />
    </div>

    <q-banner rounded class="ops-info q-mb-md">
      <template #avatar>
        <q-icon name="info" color="info" size="28px" />
      </template>
      Bu ekran normal bildirim akışını değil, sorunlu mesajları gösterir. Tablolar boşsa genellikle iyi haber:
      outbox mesajları RabbitMQ'ya yayınlanmış, notification consumer mesajları işlemiş ve dead-letter oluşmamıştır.
    </q-banner>

    <div class="row q-col-gutter-md q-mb-md">
      <div class="col-12 col-sm-6 col-lg-3">
        <div class="ops-summary">
          <q-icon name="outbox" color="primary" size="24px" />
          <div>
            <div class="ops-summary__value">{{ admin.outboxDeadLetters.length }}</div>
            <div class="ops-summary__label">Outbox hata kaydı</div>
            <div class="ops-summary__hint">0 olması beklenen sağlıklı durumdur.</div>
          </div>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-lg-3">
        <div class="ops-summary">
          <q-icon name="mark_email_unread" color="warning" size="24px" />
          <div>
            <div class="ops-summary__value">{{ admin.notificationDeadLetters.length }}</div>
            <div class="ops-summary__label">Notification hata kaydı</div>
            <div class="ops-summary__hint">Consumer 10 kez başaramazsa buraya düşer.</div>
          </div>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-lg-3">
        <div class="ops-summary">
          <q-icon name="check_circle" color="positive" size="24px" />
          <div>
            <div class="ops-summary__value">{{ healthLabel }}</div>
            <div class="ops-summary__label">Operasyon durumu</div>
            <div class="ops-summary__hint">Liste boşsa takip edilecek hata yoktur.</div>
          </div>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-lg-3">
        <div class="ops-summary">
          <q-icon name="science" color="deep-purple-4" size="24px" />
          <div>
            <div class="ops-summary__value">Probe</div>
            <div class="ops-summary__label">E-posta denemesi</div>
            <div class="ops-summary__hint">Mailpit üzerinden test e-postası doğrulanır.</div>
          </div>
        </div>
      </div>
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
            <div class="ops-card-help">
              ExpenseService bir event'i RabbitMQ'ya gönderemezse retry yapar. 10 deneme sonunda mesaj burada görünür.
              Boş olması, outbox publisher tarafında kalıcı hata olmadığını gösterir.
            </div>
          </q-card-section>
          <q-card-section>
            <q-table
              :rows="admin.outboxDeadLetters"
              :columns="outboxColumns"
              :pagination="{ rowsPerPage: 20 }"
              :rows-per-page-options="[20, 50, 100]"
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
                  :title="errorMessage ? 'Outbox kayıtları yüklenemedi' : 'Outbox hatası yok'"
                  :message="errorMessage || 'RabbitMQ yayını başarısız olup dead-letter olan outbox mesajı bulunmuyor.'"
                  :icon="errorMessage ? 'error_outline' : 'task_alt'"
                  :color="errorMessage ? 'negative' : 'positive'"
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
            <div class="ops-card-help">
              NotificationService RabbitMQ'dan aldığı mesajı işlerken hata alırsa mesajı tekrar dener.
              10 başarısız denemeden sonra kayıt burada incelenir.
            </div>
          </q-card-section>
          <q-card-section>
            <q-table
              :rows="admin.notificationDeadLetters"
              :columns="notificationColumns"
              :pagination="{ rowsPerPage: 20 }"
              :rows-per-page-options="[20, 50, 100]"
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
                  :title="errorMessage ? 'Notification kayıtları yüklenemedi' : 'Notification hatası yok'"
                  :message="errorMessage || 'Consumer tarafından işlenemeyip dead-letter olan bildirim mesajı bulunmuyor.'"
                  :icon="errorMessage ? 'error_outline' : 'task_alt'"
                  :color="errorMessage ? 'negative' : 'positive'"
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
        <div class="ops-card-help">
          RabbitMQ'dan bağımsız olarak NotificationService SMTP ayarlarını dener. Başarılı gönderimleri
          Mailpit ekranından kontrol edebilirsin.
        </div>
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
import { computed, onMounted, reactive, ref } from 'vue';
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

const healthLabel = computed(() =>
  admin.outboxDeadLetters.length === 0 && admin.notificationDeadLetters.length === 0 ? 'Sağlıklı' : 'İncele',
);

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
.ops-info {
  background: rgba(14, 116, 144, 0.16);
  border: 1px solid rgba(34, 211, 238, 0.22);
  color: #d1f7ff;
}

.ops-summary {
  align-items: flex-start;
  background: rgba(30, 27, 46, 0.68);
  border: 1px solid rgba(148, 163, 184, 0.14);
  border-radius: 10px;
  display: flex;
  gap: 12px;
  min-height: 112px;
  padding: 16px;
}

.ops-summary__value {
  color: #f9fafb;
  font-size: 22px;
  font-weight: 800;
  line-height: 1;
}

.ops-summary__label {
  color: #d1d5db;
  font-size: 13px;
  font-weight: 600;
  margin-top: 6px;
}

.ops-summary__hint,
.ops-card-help {
  color: #9ca3af;
  font-size: 12px;
  line-height: 1.45;
}

.ops-summary__hint {
  margin-top: 4px;
}

.ops-card-help {
  margin-top: 6px;
  max-width: 720px;
}
</style>
