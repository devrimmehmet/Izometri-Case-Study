<template>
  <q-page class="q-pa-lg">
    <div class="text-h5 text-weight-bold q-mb-sm">
      Hoş geldiniz, {{ auth.displayName || auth.email }}
    </div>
    <div class="text-grey-5 text-body2 q-mb-lg">
      {{ translateRole(auth.displayRole) }} — {{ auth.tenantCode.toUpperCase() }}
    </div>

    <!-- İstatistikler -->
    <div class="row q-gutter-md q-mb-lg">
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
          <div class="stat-value">{{ stats.pending }}</div>
          <div class="stat-label">Onay Bekleyen</div>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-md-3">
        <div class="stat-card">
          <q-icon name="check_circle" size="24px" color="positive" class="q-mb-sm" />
          <div class="stat-value">{{ stats.approved }}</div>
          <div class="stat-label">Onaylanan</div>
        </div>
      </div>
      <div class="col-12 col-sm-6 col-md-3">
        <div class="stat-card">
          <q-icon name="cancel" size="24px" color="negative" class="q-mb-sm" />
          <div class="stat-value">{{ stats.rejected }}</div>
          <div class="stat-label">Reddedilen</div>
        </div>
      </div>
    </div>

    <!-- Son Harcamalar -->
    <div class="glass-card q-pa-lg">
      <div class="row items-center q-mb-md">
        <div class="text-h6 text-weight-bold">Son Harcamalar</div>
        <q-space />
        <q-btn
          flat
          dense
          color="primary"
          label="Tümünü Gör"
          icon-right="arrow_forward"
          @click="$router.push('/expenses')"
        />
      </div>

      <q-table
        :rows="recentExpenses"
        :columns="columns"
        row-key="id"
        flat
        dark
        :loading="expense.loading"
        hide-pagination
        :rows-per-page-options="[5]"
        class="bg-transparent"
        no-data-label="Kayıt bulunamadı"
      >
        <template #body-cell-category="props">
          <q-td :props="props">
            {{ translateCategory(props.row.category) }}
          </q-td>
        </template>
        <template #body-cell-status="props">
          <q-td :props="props">
            <span :class="['status-badge', statusClasses[props.row.status as ExpenseStatus]]">
              {{ translateStatus(props.row.status) }}
            </span>
          </q-td>
        </template>
        <template #body-cell-amount="props">
          <q-td :props="props">
            <span class="text-weight-bold">
              {{ formatAmount(props.row.amount, props.row.currency) }}
            </span>
          </q-td>
        </template>
        <template #body-cell-createdAt="props">
          <q-td :props="props">
            {{ formatDate(props.row.createdAt) }}
          </q-td>
        </template>
      </q-table>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { useAuthStore } from 'stores/auth';
import { useExpenseStore } from 'stores/expense';
import type { ExpenseDto, ExpenseStatus } from 'src/types';
import {
  translateCategory,
  translateStatus,
  translateRole,
  statusClasses,
  formatAmount,
  formatDate,
} from 'src/utils/tr';

const auth = useAuthStore();
const expense = useExpenseStore();

const columns = [
  { name: 'category', label: 'Kategori', field: 'category', align: 'left' as const },
  { name: 'amount', label: 'Tutar', field: 'amount', align: 'right' as const },
  { name: 'status', label: 'Durum', field: 'status', align: 'center' as const },
  { name: 'createdAt', label: 'Tarih', field: 'createdAt', align: 'left' as const },
];

const recentExpenses = computed(() => expense.expenses.slice(0, 5));

const stats = computed(() => {
  const list = expense.expenses;
  return {
    total: list.length,
    pending: list.filter((e: ExpenseDto) => e.status === 'Pending').length,
    approved: list.filter((e: ExpenseDto) => e.status === 'Approved').length,
    rejected: list.filter((e: ExpenseDto) => e.status === 'Rejected').length,
  };
});

onMounted(async () => {
  await expense.fetchExpenses({ pageSize: 20 });
});
</script>
