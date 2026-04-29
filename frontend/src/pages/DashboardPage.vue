<template>
  <q-page class="q-pa-lg">
    <q-banner v-if="accessDenied" rounded class="bg-warning text-dark q-mb-md">
      Bu sayfaya erişmek için gerekli rolünüz yok. Yetkinizin olduğu gösterge paneline yönlendirildiniz.
    </q-banner>

    <div class="text-h5 text-weight-bold q-mb-sm">Hoş geldiniz, {{ auth.displayName || auth.email }}</div>
    <div class="text-grey-5 text-body2 q-mb-lg">{{ translateRole(auth.displayRole) }} - {{ auth.tenantCode.toUpperCase() }}</div>

    <div class="row q-col-gutter-md q-mb-lg">
      <div class="col-12 col-sm-6 col-md-3">
        <div class="stat-card">
          <q-icon name="receipt_long" size="24px" color="primary" class="q-mb-sm" />
          <div class="stat-value">{{ stats.total }}</div>
          <div class="stat-label">Toplam Harcama</div>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-md-3">
        <div class="stat-card">
          <q-icon name="hourglass_top" size="24px" color="warning" class="q-mb-sm" />
          <div class="stat-value">{{ stats.pendingHr }}</div>
          <div class="stat-label">İK Onayı Bekleyen</div>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-md-3">
        <div class="stat-card">
          <q-icon name="admin_panel_settings" size="24px" color="deep-purple-4" class="q-mb-sm" />
          <div class="stat-value">{{ stats.pendingAdmin }}</div>
          <div class="stat-label">Admin Onayı Bekleyen</div>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-md-3">
        <div class="stat-card">
          <q-icon name="check_circle" size="24px" color="positive" class="q-mb-sm" />
          <div class="stat-value">{{ stats.approved }}</div>
          <div class="stat-label">Onaylanan</div>
        </div>
      </div>
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-lg-7">
        <div class="glass-card q-pa-lg">
          <q-banner v-if="errorMessage" rounded class="bg-negative text-white q-mb-md">
            {{ errorMessage }}
            <template #action>
              <q-btn flat color="white" icon="refresh" label="Tekrar dene" @click="loadDashboard" />
            </template>
          </q-banner>
          <div class="row items-center q-mb-md">
            <div class="text-h6 text-weight-bold">Son Harcamalar</div>
            <q-space />
            <q-btn flat dense color="primary" label="Tümünü Gör" icon-right="arrow_forward" @click="$router.push('/expenses')" />
          </div>

          <q-table
            :rows="recentExpenses"
            :columns="columns"
            :grid="$q.screen.xs"
            row-key="id"
            flat
            dark
            :loading="expense.loading || statsLoading"
            loading-label="Yükleniyor..."
            hide-pagination
            :rows-per-page-options="[5]"
            class="bg-transparent"
          >
            <template #no-data>
              <DataState
                :title="errorMessage ? 'Dashboard verileri yüklenemedi' : 'Kayıt bulunamadı'"
                :message="errorMessage || 'Henüz gösterilecek harcama kaydı yok.'"
                :icon="errorMessage ? 'error_outline' : 'inbox'"
                :color="errorMessage ? 'negative' : 'grey-5'"
                :retry-label="errorMessage ? 'Tekrar dene' : ''"
                dense
                @retry="loadDashboard"
              />
            </template>
            <template #body-cell-category="props">
              <q-td :props="props">{{ translateCategory(props.row.category) }}</q-td>
            </template>
            <template #body-cell-status="props">
              <q-td :props="props">
                <span :class="['status-badge', statusClass(props.row)]">{{ statusLabel(props.row) }}</span>
              </q-td>
            </template>
            <template #body-cell-amount="props">
              <q-td :props="props">
                <q-badge :color="props.row.amountInTry > 5000 ? 'warning' : 'grey-7'" outline>
                  {{ formatAmount(props.row.amount, props.row.currency) }}
                  <q-tooltip v-if="props.row.amountInTry > 5000">5.000 TL üstü</q-tooltip>
                </q-badge>
              </q-td>
            </template>
            <template #body-cell-createdAt="props">
              <q-td :props="props">{{ formatDate(props.row.createdAt) }}</q-td>
            </template>
          </q-table>
        </div>
      </div>

      <div class="col-12 col-lg-5">
        <div class="glass-card q-pa-lg">
          <div class="text-h6 text-weight-bold q-mb-md">Demo Akışı</div>
          <q-timeline color="primary" dark dense>
            <q-timeline-entry title="Personel" subtitle="test1 / devrimmehmet@msn.com / Pass123!" icon="person" />
            <q-timeline-entry title="HR" subtitle="test1 / devrimmehmet@gmail.com / Pass123!" icon="badge" />
            <q-timeline-entry title="Admin" subtitle="test1 / pattabanoglu@devrimmehmet.com / Pass123!" icon="admin_panel_settings" />
            <q-timeline-entry title="Bildirim" subtitle="Bildirimler ekranında event kaydını kontrol edin." icon="notifications" />
            <q-timeline-entry title="Mailpit" subtitle="http://localhost:8025 üzerinden e-postayı doğrulayın." icon="mark_email_read" />
          </q-timeline>
        </div>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useQuasar } from 'quasar';
import { useRoute } from 'vue-router';
import { useAuthStore } from 'stores/auth';
import { useExpenseStore } from 'stores/expense';
import DataState from 'components/DataState.vue';
import { api } from 'src/services/http';
import type { ExpenseDto, ExpenseStatus, PagedResponse } from 'src/types';
import { formatAmount, formatDate, statusClasses, translateCategory, translateRole, translateStatus } from 'src/utils/tr';
import { notifyApiError } from 'src/utils/errors';

const $q = useQuasar();
const route = useRoute();
const auth = useAuthStore();
const expense = useExpenseStore();
const statsLoading = ref(false);
const errorMessage = ref('');
const accessDenied = computed(() => route.query.denied === 'role');

const stats = reactive({
  total: 0,
  pendingHr: 0,
  pendingAdmin: 0,
  approved: 0,
});

const columns = [
  { name: 'category', label: 'Kategori', field: 'category', align: 'left' as const },
  { name: 'amount', label: 'Tutar', field: 'amount', align: 'right' as const },
  { name: 'status', label: 'Durum', field: 'status', align: 'center' as const },
  { name: 'createdAt', label: 'Tarih', field: 'createdAt', align: 'left' as const },
];

const recentExpenses = computed(() => expense.expenses.slice(0, 5));

function statusClass(row: ExpenseDto): string {
  if (row.status === 'Pending' && row.requiresAdminApproval && row.hrApproved) return 'pending-admin';
  return statusClasses[row.status] ?? 'draft';
}

function statusLabel(row: ExpenseDto): string {
  if (row.status === 'Pending' && row.requiresAdminApproval && row.hrApproved) return 'Admin Onayı Bekliyor';
  return translateStatus(row.status);
}

async function countExpenses(status?: ExpenseStatus): Promise<PagedResponse<ExpenseDto>> {
  const query = new URLSearchParams({ pageNumber: '1', pageSize: '100' });
  if (status) query.set('status', status);
  const { data } = await api.get<PagedResponse<ExpenseDto>>(`/expenses?${query.toString()}`);
  return data;
}

async function loadDashboard() {
  statsLoading.value = true;
  errorMessage.value = '';
  try {
    const [all, pending, approved] = await Promise.all([
      countExpenses(),
      countExpenses('Pending'),
      countExpenses('Approved'),
      expense.fetchExpenses({ pageNumber: 1, pageSize: 5 }),
    ]);

    stats.total = all.totalCount;
    stats.approved = approved.totalCount;
    stats.pendingHr = pending.items.filter((item) => !item.hrApproved).length;
    stats.pendingAdmin = pending.items.filter((item) => item.requiresAdminApproval && item.hrApproved && !item.adminApproved).length;
  } catch (error) {
    errorMessage.value = 'Backend verileri alınamadı.';
    notifyApiError($q, error, 'Dashboard verileri yüklenemedi');
  } finally {
    statsLoading.value = false;
  }
}

onMounted(() => {
  void loadDashboard();
});
</script>
