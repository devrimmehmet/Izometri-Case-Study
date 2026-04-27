<template>
  <q-page class="q-pa-lg">
    <div class="row items-center q-mb-lg">
      <div>
        <div class="text-h5 text-weight-bold">Harcamalar</div>
        <div class="text-grey-5 text-body2">Tüm harcama taleplerini yönetin</div>
      </div>
      <q-space />
      <q-btn
        v-if="auth.isPersonnel"
        color="primary"
        icon="add"
        label="Yeni Harcama"
        rounded
        unelevated
        @click="showCreateDialog = true"
      />
    </div>

    <!-- Filters -->
    <div class="glass-card q-pa-md q-mb-lg">
      <div class="row q-gutter-sm items-end">
        <q-select
          v-model="filters.status"
          :options="statusOptions"
          label="Durum"
          outlined
          dark
          dense
          clearable
          emit-value
          map-options
          style="min-width: 160px"
          color="primary"
        />
        <q-select
          v-model="filters.category"
          :options="categoryOptions"
          label="Kategori"
          outlined
          dark
          dense
          clearable
          emit-value
          map-options
          style="min-width: 160px"
          color="primary"
        />
        <q-input
          v-model="filters.dateFrom"
          label="Başlangıç"
          type="date"
          outlined
          dark
          dense
          style="min-width: 150px"
          color="primary"
        />
        <q-input
          v-model="filters.dateTo"
          label="Bitiş"
          type="date"
          outlined
          dark
          dense
          style="min-width: 150px"
          color="primary"
        />
        <q-btn
          color="primary"
          icon="search"
          label="Filtrele"
          rounded
          unelevated
          dense
          @click="applyFilters"
        />
        <q-btn
          flat
          dense
          icon="clear"
          label="Temizle"
          color="grey-5"
          @click="clearFilters"
        />
      </div>
    </div>

    <!-- Table -->
    <div class="glass-card q-pa-md">
      <q-table
        :rows="expense.expenses"
        :columns="columns"
        row-key="id"
        flat
        dark
        :loading="expense.loading"
        :pagination="pagination"
        class="bg-transparent"
        @update:pagination="onPaginationChange"
      >
        <template #body-cell-category="props">
          <q-td :props="props">
            {{ translateCategory(props.row.category) }}
          </q-td>
        </template>
        <template #body-cell-status="props">
          <q-td :props="props">
            <span :class="['status-badge', statusClass(props.row.status)]">
              {{ statusLabel(props.row.status) }}
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
        <template #body-cell-actions="props">
          <q-td :props="props">
            <q-btn
              flat
              dense
              round
              icon="visibility"
              color="primary"
              @click="openDetail(props.row)"
            >
              <q-tooltip>Detay</q-tooltip>
            </q-btn>
            <q-btn
              v-if="props.row.status === 'Draft'"
              flat
              dense
              round
              icon="send"
              color="info"
              @click="submitExpense(props.row.id)"
            >
              <q-tooltip>Gönder</q-tooltip>
            </q-btn>
            <q-btn
              v-if="canApproveRow(props.row)"
              flat
              dense
              round
              icon="check"
              color="positive"
              @click="approveExpense(props.row.id)"
            >
              <q-tooltip>Onayla</q-tooltip>
            </q-btn>
            <q-btn
              v-if="canApproveRow(props.row)"
              flat
              dense
              round
              icon="close"
              color="negative"
              @click="openReject(props.row.id)"
            >
              <q-tooltip>Reddet</q-tooltip>
            </q-btn>
            <q-btn
              v-if="props.row.status === 'Draft'"
              flat
              dense
              round
              icon="delete"
              color="negative"
              @click="deleteExpense(props.row.id)"
            >
              <q-tooltip>Sil</q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </div>

    <!-- Create Expense Dialog -->
    <q-dialog v-model="showCreateDialog" persistent>
      <q-card dark style="min-width: 440px" class="glass-card">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Yeni Harcama Talebi</div>
        </q-card-section>
        <q-card-section>
          <q-form @submit.prevent="onCreate" class="q-gutter-md">
            <q-select
              v-model="createForm.category"
              :options="categoryOptions"
              label="Kategori"
              outlined
              dark
              dense
              emit-value
              map-options
              :rules="[(v: string) => !!v || 'Kategori seçiniz']"
            />
            <div class="row q-gutter-sm">
              <q-input
                v-model.number="createForm.amount"
                label="Tutar"
                type="number"
                outlined
                dark
                dense
                class="col"
                :rules="[(v: number) => v > 0 || 'Tutar pozitif olmalı']"
              />
              <q-select
                v-model="createForm.currency"
                :options="currencyOptions"
                label="Para Birimi"
                outlined
                dark
                dense
                emit-value
                map-options
                style="min-width: 120px"
              />
            </div>
            <q-input
              v-model="createForm.description"
              label="Açıklama (min. 20 karakter)"
              type="textarea"
              outlined
              dark
              dense
              :rules="[
                (v: string) => !!v || 'Açıklama gerekli',
                (v: string) => v.length >= 20 || 'En az 20 karakter gerekli',
              ]"
            />
            <div class="row justify-end q-gutter-sm">
              <q-btn flat label="İptal" color="grey" @click="showCreateDialog = false" />
              <q-btn
                type="submit"
                label="Oluştur"
                color="primary"
                rounded
                unelevated
                :loading="creating"
              />
            </div>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>

    <!-- Reject Dialog -->
    <q-dialog v-model="showRejectDialog" persistent>
      <q-card dark style="min-width: 400px" class="glass-card">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Harcama Reddi</div>
        </q-card-section>
        <q-card-section>
          <q-form @submit.prevent="onReject">
            <q-input
              v-model="rejectReason"
              label="Red Nedeni (min. 10 karakter)"
              type="textarea"
              outlined
              dark
              dense
              :rules="[
                (v: string) => !!v || 'Red nedeni gerekli',
                (v: string) => v.length >= 10 || 'En az 10 karakter',
              ]"
            />
            <div class="row justify-end q-gutter-sm q-mt-md">
              <q-btn flat label="İptal" color="grey" @click="showRejectDialog = false" />
              <q-btn
                type="submit"
                label="Reddet"
                color="negative"
                rounded
                unelevated
                :loading="rejecting"
              />
            </div>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>

    <!-- Detail Dialog -->
    <q-dialog v-model="showDetailDialog">
      <q-card dark style="min-width: 500px" class="glass-card">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Harcama Detayı</div>
        </q-card-section>
        <q-card-section v-if="detailExpense">
          <div class="q-gutter-sm">
            <div class="row">
              <div class="col-4 text-grey-5">ID:</div>
              <div class="col-8 text-weight-medium" style="font-size: 12px">
                {{ detailExpense.id }}
              </div>
            </div>
            <div class="row">
              <div class="col-4 text-grey-5">Kategori:</div>
              <div class="col-8">{{ translateCategory(detailExpense.category) }}</div>
            </div>
            <div class="row">
              <div class="col-4 text-grey-5">Tutar:</div>
              <div class="col-8 text-weight-bold">
                {{ formatAmount(detailExpense.amount, detailExpense.currency) }}
              </div>
            </div>
            <div class="row">
              <div class="col-4 text-grey-5">Durum:</div>
              <div class="col-8">
                <span
                  :class="['status-badge', statusClass(detailExpense.status)]"
                >
                  {{ statusLabel(detailExpense.status) }}
                </span>
              </div>
            </div>
            <div class="row">
              <div class="col-4 text-grey-5">Açıklama:</div>
              <div class="col-8">{{ detailExpense.description }}</div>
            </div>
            <div class="row">
              <div class="col-4 text-grey-5">Oluşturulma:</div>
              <div class="col-8">{{ formatDate(detailExpense.createdAt) }}</div>
            </div>
          </div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="Kapat" color="primary" @click="showDetailDialog = false" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import { useAuthStore } from 'stores/auth';
import { useExpenseStore } from 'stores/expense';
import type {
  ExpenseDto,
  ExpenseStatus,
  ExpenseQueryParams,
  CreateExpenseRequest,
} from 'src/types';
import {
  translateStatus,
  translateCategory,
  statusClasses,
  formatAmount,
  formatDate,
} from 'src/utils/tr';

const $q = useQuasar();
const auth = useAuthStore();
const expense = useExpenseStore();

// Filters
const filters = reactive({
  status: null as ExpenseStatus | null,
  category: null as string | null,
  dateFrom: '',
  dateTo: '',
});

const statusOptions = [
  { label: 'Taslak', value: 'Draft' },
  { label: 'Onay Bekliyor', value: 'Pending' },
  { label: 'Yönetici Onayı', value: 'PendingAdminApproval' },
  { label: 'Onaylandı', value: 'Approved' },
  { label: 'Reddedildi', value: 'Rejected' },
];

const categoryOptions = [
  { label: 'Seyahat', value: 'Travel' },
  { label: 'Malzeme', value: 'Material' },
  { label: 'Eğitim', value: 'Education' },
  { label: 'Diğer', value: 'Other' },
];

const currencyOptions = [
  { label: '₺ TRY', value: 'TRY' },
  { label: '$ USD', value: 'USD' },
  { label: '€ EUR', value: 'EUR' },
];

const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  rowsNumber: 0,
});

const columns = [
  { name: 'category', label: 'Kategori', field: 'category', align: 'left' as const, sortable: true },
  { name: 'amount', label: 'Tutar', field: 'amount', align: 'right' as const, sortable: true },
  { name: 'status', label: 'Durum', field: 'status', align: 'center' as const },
  { name: 'createdAt', label: 'Tarih', field: 'createdAt', align: 'left' as const, sortable: true },
  { name: 'actions', label: 'İşlemler', field: '', align: 'center' as const },
];

// Create
const showCreateDialog = ref(false);
const creating = ref(false);
const createForm = reactive<CreateExpenseRequest>({
  category: 'Travel',
  currency: 'TRY',
  amount: 0,
  description: '',
});

// Reject
const showRejectDialog = ref(false);
const rejecting = ref(false);
const rejectReason = ref('');
const rejectTargetId = ref('');

// Detail
const showDetailDialog = ref(false);
const detailExpense = ref<ExpenseDto | null>(null);

function statusClass(status: ExpenseStatus): string {
  return statusClasses[status] ?? 'draft';
}

function statusLabel(status: ExpenseStatus): string {
  return translateStatus(status);
}

function canApproveRow(row: ExpenseDto): boolean {
  if (row.status === 'Pending' && (auth.isHR || auth.isAdmin)) return true;
  if (row.status === 'PendingAdminApproval' && auth.isAdmin) return true;
  return false;
}

function openDetail(row: ExpenseDto) {
  detailExpense.value = row;
  showDetailDialog.value = true;
}

async function applyFilters() {
  await expense.fetchExpenses({
    status: filters.status ?? undefined,
    category: filters.category as ExpenseQueryParams['category'],
    dateFrom: filters.dateFrom !== '' ? filters.dateFrom : undefined,
    dateTo: filters.dateTo !== '' ? filters.dateTo : undefined,
    pageNumber: pagination.value.page,
    pageSize: pagination.value.rowsPerPage,
  });
}

async function clearFilters() {
  filters.status = null;
  filters.category = null;
  filters.dateFrom = '';
  filters.dateTo = '';
  await expense.fetchExpenses();
}

function onPaginationChange(p: { page: number; rowsPerPage: number }) {
  pagination.value = { ...pagination.value, ...p };
  void applyFilters();
}

async function onCreate() {
  creating.value = true;
  try {
    await expense.createExpense(createForm);
    showCreateDialog.value = false;
    createForm.amount = 0;
    createForm.description = '';
    $q.notify({ type: 'positive', message: 'Harcama oluşturuldu' });
  } catch (err: unknown) {
    const error = err as { response?: { data?: { detail?: string } }; message?: string };
    $q.notify({ type: 'negative', message: error.response?.data?.detail ?? 'Hata oluştu' });
  } finally {
    creating.value = false;
  }
}

async function submitExpense(id: string) {
  try {
    await expense.submitExpense(id);
    $q.notify({ type: 'positive', message: 'Harcama gönderildi' });
  } catch {
    $q.notify({ type: 'negative', message: 'Gönderme başarısız' });
  }
}

async function approveExpense(id: string) {
  try {
    await expense.approveExpense(id);
    $q.notify({ type: 'positive', message: 'Harcama onaylandı' });
  } catch {
    $q.notify({ type: 'negative', message: 'Onay başarısız' });
  }
}

function openReject(id: string) {
  rejectTargetId.value = id;
  rejectReason.value = '';
  showRejectDialog.value = true;
}

async function onReject() {
  rejecting.value = true;
  try {
    await expense.rejectExpense(rejectTargetId.value, {
      reason: rejectReason.value,
    });
    showRejectDialog.value = false;
    $q.notify({ type: 'positive', message: 'Harcama reddedildi' });
  } catch (err: unknown) {
    const error = err as { response?: { data?: { detail?: string } }; message?: string };
    $q.notify({ type: 'negative', message: error.response?.data?.detail ?? 'Hata oluştu' });
  } finally {
    rejecting.value = false;
  }
}

function deleteExpense(id: string) {
  $q.dialog({
    title: 'Silme Onayı',
    message: 'Bu harcamayı silmek istediğinize emin misiniz?',
    dark: true,
    cancel: true,
    persistent: true,
  }).onOk(() => {
    expense.deleteExpense(id).then(
      () => $q.notify({ type: 'positive', message: 'Harcama silindi (soft delete)' }),
      () => $q.notify({ type: 'negative', message: 'Silme başarısız' }),
    );
  });
}

onMounted(async () => {
  await expense.fetchExpenses();
});
</script>
