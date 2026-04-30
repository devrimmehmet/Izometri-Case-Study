<template>
  <q-page class="q-pa-lg">
    <div class="row items-center q-mb-lg">
      <div>
        <div class="text-h5 text-weight-bold">Harcamalar</div>
        <div class="text-grey-5 text-body2">Tüm harcama taleplerini yönetin</div>
      </div>
      <q-space />
      <q-btn
        v-if="auth.isPersonel"
        color="primary"
        icon="add"
        label="Yeni Harcama"
        rounded
        unelevated
        @click="openCreate"
      />
    </div>

    <div class="glass-card q-pa-md q-mb-lg">
      <div class="row q-gutter-sm items-end">
        <q-select v-model="filters.status" :options="statusOptions" label="Durum" outlined dark dense clearable emit-value map-options style="min-width: 160px" color="primary" />
        <q-select v-model="filters.category" :options="categoryOptions" label="Kategori" outlined dark dense clearable emit-value map-options style="min-width: 160px" color="primary" />
        <q-input v-model="filters.dateFrom" label="Başlangıç" type="date" outlined dark dense style="min-width: 150px" color="primary" />
        <q-input v-model="filters.dateTo" label="Bitiş" type="date" outlined dark dense style="min-width: 150px" color="primary" />
        <q-btn color="primary" icon="search" label="Filtrele" rounded unelevated dense @click="applyFilters(true)" />
        <q-btn flat dense icon="clear" label="Temizle" color="grey-5" @click="clearFilters" />
      </div>
    </div>

    <div class="glass-card q-pa-md">
      <q-banner v-if="errorMessage" rounded class="bg-negative text-white q-mb-md">
        {{ errorMessage }}
        <template #action>
          <q-btn flat color="white" icon="refresh" label="Tekrar dene" @click="loadExpenses" />
        </template>
      </q-banner>
      <q-table
        v-model:pagination="pagination"
        :rows="expense.expenses"
        :columns="columns"
        :rows-per-page-options="[20, 50, 100]"
        :grid="$q.screen.xs"
        row-key="id"
        flat
        dark
        :loading="expense.loading"
        loading-label="Yükleniyor..."
        class="bg-transparent"
        @request="onTableRequest"
      >
        <template #no-data>
          <DataState
            :title="errorMessage ? 'Harcamalar yüklenemedi' : 'Kayıt bulunamadı'"
            :message="errorMessage || 'Filtreleri değiştirerek tekrar deneyin.'"
            :icon="errorMessage ? 'error_outline' : 'inbox'"
            :color="errorMessage ? 'negative' : 'grey-5'"
            :retry-label="errorMessage ? 'Tekrar dene' : ''"
            dense
            @retry="loadExpenses"
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
            <span class="text-weight-bold">{{ formatAmount(props.row.amount, props.row.currency) }}</span>
          </q-td>
        </template>
        <template #body-cell-exchangeRate="props">
          <q-td :props="props" class="text-grey-5">
            {{ props.row.currency !== 'TRY' ? props.row.exchangeRate.toFixed(4) : '-' }}
          </q-td>
        </template>
        <template #body-cell-amountInTry="props">
          <q-td :props="props">
            <q-badge :color="props.row.amountInTry > 5000 ? 'warning' : 'grey-7'" outline>
              {{ formatAmount(props.row.amountInTry, 'TRY') }}
              <q-tooltip v-if="props.row.amountInTry > 5000">5.000 TL üstü: admin onayı gerekir</q-tooltip>
            </q-badge>
          </q-td>
        </template>
        <template #body-cell-createdAt="props">
          <q-td :props="props">{{ formatDate(props.row.createdAt) }}</q-td>
        </template>
        <template #body-cell-actions="props">
          <q-td :props="props" class="action-cell">
            <q-btn flat dense round icon="visibility" color="primary" @click="openDetail(props.row)">
              <q-tooltip>Detay</q-tooltip>
            </q-btn>
            <q-btn v-if="canSubmitRow(props.row)" flat dense round icon="send" color="info" @click="submitExpense(props.row.id)">
              <q-tooltip>Onaya gönder</q-tooltip>
            </q-btn>
            <q-btn
              v-if="canApproveRow(props.row)"
              flat
              dense
              round
              :icon="props.row.requiresAdminApproval && !props.row.hrApproved ? 'send' : 'check'"
              :color="props.row.requiresAdminApproval && !props.row.hrApproved ? 'warning' : 'positive'"
              @click="approveExpense(props.row.id)"
            >
              <q-tooltip>{{ props.row.requiresAdminApproval && !props.row.hrApproved ? 'Admin onayına ilet' : 'Onayla' }}</q-tooltip>
            </q-btn>
            <q-btn v-if="canApproveRow(props.row)" flat dense round icon="close" color="negative" @click="openReject(props.row.id)">
              <q-tooltip>Reddet</q-tooltip>
            </q-btn>
            <q-btn v-if="canEditRow(props.row)" flat dense round icon="edit" color="primary" @click="openEdit(props.row)">
              <q-tooltip>Düzenle</q-tooltip>
            </q-btn>
            <q-btn v-if="canEditRow(props.row)" flat dense round icon="delete" color="negative" @click="deleteExpense(props.row.id)">
              <q-tooltip>Sil</q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </div>

    <q-dialog v-model="showCreateDialog" persistent>
      <q-card dark class="glass-card responsive-dialog">
        <q-card-section>
          <div class="text-h6 text-weight-bold">{{ isEditing ? 'Harcamayı Düzenle' : 'Yeni Harcama Talebi' }}</div>
        </q-card-section>
        <q-card-section>
          <q-form @submit.prevent="onCreate" class="q-gutter-md">
            <q-select v-model="createForm.category" :options="categoryOptions" label="Kategori" outlined dark dense emit-value map-options :rules="[(v: string) => !!v || 'Kategori seçiniz']" />
            <div class="row q-col-gutter-sm">
              <div class="col-12 col-sm">
                <q-input v-model.number="createForm.amount" label="Tutar" type="number" outlined dark dense :rules="[(v: number) => v > 0 || 'Tutar pozitif olmalı']" />
              </div>
              <div class="col-12 col-sm-4">
                <q-select v-model="createForm.currency" :options="currencyOptions" label="Para Birimi" outlined dark dense emit-value map-options />
              </div>
            </div>
            <q-input
              v-model="createForm.description"
              label="Açıklama (en az 20 karakter)"
              type="textarea"
              outlined
              dark
              dense
              :rules="[
                (v: string) => !!v || 'Açıklama gerekli',
                (v: string) => v.length >= 20 || 'Backend kuralı: en az 20 karakter gerekli',
              ]"
            />
            <div class="row justify-end q-gutter-sm">
              <q-btn flat label="İptal" color="grey" @click="showCreateDialog = false" />
              <q-btn type="submit" :label="isEditing ? 'Güncelle' : 'Oluştur'" color="primary" rounded unelevated :loading="creating" />
            </div>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>

    <q-dialog v-model="showRejectDialog" persistent>
      <q-card dark class="glass-card responsive-dialog">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Harcama Reddi</div>
          <div class="text-caption text-grey-5">Backend kuralı: red nedeni en az 10 karakter olmalıdır.</div>
        </q-card-section>
        <q-card-section>
          <q-form @submit.prevent="onReject">
            <q-input
              v-model="rejectReason"
              label="Red nedeni"
              type="textarea"
              outlined
              dark
              dense
              :rules="[
                (v: string) => !!v || 'Red nedeni gerekli',
                (v: string) => v.length >= 10 || 'En az 10 karakter yazın',
              ]"
            />
            <div class="row justify-end q-gutter-sm q-mt-md">
              <q-btn flat label="İptal" color="grey" @click="showRejectDialog = false" />
              <q-btn type="submit" label="Reddet" color="negative" rounded unelevated :loading="rejecting" />
            </div>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>

    <q-dialog v-model="showDetailDialog">
      <q-card dark class="glass-card responsive-dialog detail-dialog">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Harcama Detayı</div>
        </q-card-section>
        <q-card-section v-if="detailExpense">
          <q-timeline color="primary" dark dense class="q-mb-md">
            <q-timeline-entry v-for="step in workflowSteps" :key="step.title" :title="step.title" :subtitle="step.subtitle" :color="step.color" :icon="step.icon" />
          </q-timeline>

          <div class="q-gutter-sm">
            <div class="row"><div class="col-4 text-grey-5">ID:</div><div class="col-8 text-caption id-text"><CopyValue :value="detailExpense.id" tooltip="Harcama ID kopyala" /></div></div>
            <div class="row"><div class="col-4 text-grey-5">Kategori:</div><div class="col-8">{{ translateCategory(detailExpense.category) }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Tutar:</div><div class="col-8 text-weight-bold">{{ formatAmount(detailExpense.amount, detailExpense.currency) }}</div></div>
            <div v-if="detailExpense.currency !== 'TRY'" class="row"><div class="col-4 text-grey-5">Kur:</div><div class="col-8">{{ detailExpense.exchangeRate.toFixed(4) }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Tutar (TL):</div><div class="col-8 text-weight-bold text-primary">{{ formatAmount(detailExpense.amountInTry, 'TRY') }}</div></div>
            <div v-if="detailExpense.requiresAdminApproval" class="row"><div class="col-4 text-grey-5">Onay Eşiği:</div><div class="col-8 text-warning">5.000 TL üstü, admin onayı gerekir.</div></div>
            <div class="row"><div class="col-4 text-grey-5">Durum:</div><div class="col-8"><span :class="['status-badge', statusClass(detailExpense)]">{{ statusLabel(detailExpense) }}</span></div></div>
            <div class="row"><div class="col-4 text-grey-5">Açıklama:</div><div class="col-8">{{ detailExpense.description }}</div></div>
            <div v-if="detailExpense.rejectionReason" class="row"><div class="col-4 text-grey-5">Red Nedeni:</div><div class="col-8 text-negative">{{ detailExpense.rejectionReason }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Oluşturulma:</div><div class="col-8">{{ formatDate(detailExpense.createdAt) }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Gönderilme:</div><div class="col-8">{{ formatDate(detailExpense.submittedAt) }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Onaylanma:</div><div class="col-8">{{ formatDate(detailExpense.approvedAt) }}</div></div>
            <div class="row"><div class="col-4 text-grey-5">Reddedilme:</div><div class="col-8">{{ formatDate(detailExpense.rejectedAt) }}</div></div>
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
import { computed, onMounted, reactive, ref } from 'vue';
import { useQuasar } from 'quasar';
import { useAuthStore } from 'stores/auth';
import { useExpenseStore } from 'stores/expense';
import CopyValue from 'components/CopyValue.vue';
import DataState from 'components/DataState.vue';
import type { CreateExpenseRequest, ExpenseDto, ExpenseQueryParams, ExpenseStatus } from 'src/types';
import { formatAmount, formatDate, statusClasses, translateCategory, translateStatus } from 'src/utils/tr';
import { notifyApiError } from 'src/utils/errors';

const $q = useQuasar();
const auth = useAuthStore();
const expense = useExpenseStore();

const filters = reactive({
  status: null as ExpenseStatus | null,
  category: null as ExpenseQueryParams['category'] | null,
  dateFrom: '',
  dateTo: '',
});

const statusOptions = [
  { label: 'Taslak', value: 'Draft' },
  { label: 'Onay Bekliyor', value: 'Pending' },
  { label: 'Onaylandı', value: 'Approved' },
  { label: 'Reddedildi', value: 'Rejected' },
];

const categoryOptions = [
  { label: 'Seyahat', value: 'Travel' },
  { label: 'Ekipman', value: 'Equipment' },
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
  rowsPerPage: 20,
  rowsNumber: 0,
});

const columns = [
  { name: 'category', label: 'Kategori', field: 'category', align: 'left' as const, sortable: true },
  { name: 'amount', label: 'Tutar', field: 'amount', align: 'right' as const, sortable: true },
  { name: 'exchangeRate', label: 'Kur', field: 'exchangeRate', align: 'right' as const },
  { name: 'amountInTry', label: 'Tutar (TL)', field: 'amountInTry', align: 'right' as const, sortable: true },
  { name: 'status', label: 'Durum', field: 'status', align: 'center' as const },
  { name: 'createdAt', label: 'Tarih', field: 'createdAt', align: 'left' as const, sortable: true },
  { name: 'actions', label: 'İşlemler', field: '', align: 'center' as const },
];

const showCreateDialog = ref(false);
const creating = ref(false);
const createForm = reactive<CreateExpenseRequest>({
  category: 'Travel',
  currency: 'TRY',
  amount: 0,
  description: '',
});
const isEditing = ref(false);
const editTargetId = ref('');

const showRejectDialog = ref(false);
const rejecting = ref(false);
const rejectReason = ref('');
const rejectTargetId = ref('');

const showDetailDialog = ref(false);
const detailExpense = ref<ExpenseDto | null>(null);
const errorMessage = ref('');

const workflowSteps = computed(() => {
  const row = detailExpense.value;
  if (!row) return [];

  return [
    { title: 'Draft', subtitle: formatDate(row.createdAt), color: 'grey', icon: 'edit_note' },
    { title: 'Submitted', subtitle: formatDate(row.submittedAt), color: row.submittedAt ? 'info' : 'grey', icon: 'send' },
    { title: 'HR Approved', subtitle: row.hrApproved ? 'İK onayı verildi' : 'İK onayı bekleniyor', color: row.hrApproved ? 'positive' : 'grey', icon: 'badge' },
    { title: 'Admin Required', subtitle: row.requiresAdminApproval ? 'Admin onayı gerekiyor' : 'Admin onayı gerekmiyor', color: row.requiresAdminApproval ? 'warning' : 'grey', icon: 'admin_panel_settings' },
    { title: 'Approved', subtitle: formatDate(row.approvedAt), color: row.status === 'Approved' ? 'positive' : 'grey', icon: 'check' },
    { title: 'Rejected', subtitle: row.rejectionReason || formatDate(row.rejectedAt), color: row.status === 'Rejected' ? 'negative' : 'grey', icon: 'close' },
  ];
});

function statusClass(row: ExpenseDto): string {
  if (row.status === 'Pending' && row.requiresAdminApproval && row.hrApproved) return 'pending-admin';
  return statusClasses[row.status] ?? 'draft';
}

function statusLabel(row: ExpenseDto): string {
  if (row.status === 'Pending' && row.requiresAdminApproval && row.hrApproved) return 'Admin Onayı Bekliyor';
  return translateStatus(row.status);
}

function canEditRow(row: ExpenseDto): boolean {
  return auth.isPersonel && row.status === 'Draft' && row.requestedByUserId === auth.userId;
}

function canSubmitRow(row: ExpenseDto): boolean {
  return canEditRow(row);
}

function canApproveRow(row: ExpenseDto): boolean {
  if (row.status !== 'Pending') return false;
  if (auth.isHR) return !row.hrApproved;
  if (auth.isAdmin) return row.requiresAdminApproval && row.hrApproved && !row.adminApproved;
  return false;
}

function openCreate() {
  isEditing.value = false;
  editTargetId.value = '';
  createForm.category = 'Travel';
  createForm.currency = 'TRY';
  createForm.amount = 0;
  createForm.description = '';
  showCreateDialog.value = true;
}

function openEdit(row: ExpenseDto) {
  isEditing.value = true;
  editTargetId.value = row.id;
  createForm.category = row.category;
  createForm.currency = row.currency;
  createForm.amount = row.amount;
  createForm.description = row.description;
  showCreateDialog.value = true;
}

function openDetail(row: ExpenseDto) {
  detailExpense.value = row;
  showDetailDialog.value = true;
}

function toUtcDayStart(dateStr: string): string {
  return new Date(`${dateStr}T00:00:00.000Z`).toISOString();
}

function toUtcDayEnd(dateStr: string): string {
  return new Date(`${dateStr}T23:59:59.999Z`).toISOString();
}

function buildQuery(): ExpenseQueryParams {
  return {
    status: filters.status ?? undefined,
    category: filters.category ?? undefined,
    dateFrom: filters.dateFrom ? toUtcDayStart(filters.dateFrom) : undefined,
    dateTo: filters.dateTo ? toUtcDayEnd(filters.dateTo) : undefined,
    pageNumber: pagination.value.page,
    pageSize: pagination.value.rowsPerPage,
  };
}

async function loadExpenses() {
  errorMessage.value = '';
  try {
    await expense.fetchExpenses(buildQuery());
    pagination.value.rowsNumber = expense.totalCount;
    pagination.value.page = expense.pageNumber;
    pagination.value.rowsPerPage = expense.pageSize;
  } catch (error) {
    errorMessage.value = 'Harcama listesi backend servisinden alınamadı.';
    notifyApiError($q, error, 'Harcamalar yüklenemedi');
  }
}

async function applyFilters(resetPage = false) {
  if (resetPage) pagination.value.page = 1;
  await loadExpenses();
}

async function clearFilters() {
  filters.status = null;
  filters.category = null;
  filters.dateFrom = '';
  filters.dateTo = '';
  pagination.value.page = 1;
  await loadExpenses();
}

function onTableRequest(props: { pagination: { page: number; rowsPerPage: number } }) {
  pagination.value = {
    ...pagination.value,
    page: props.pagination.page,
    rowsPerPage: props.pagination.rowsPerPage,
  };
  void loadExpenses();
}

async function onCreate() {
  creating.value = true;
  try {
    if (isEditing.value) {
      await expense.updateExpense(editTargetId.value, createForm);
      $q.notify({ type: 'positive', message: 'Harcama güncellendi' });
    } else {
      await expense.createExpense(createForm);
      $q.notify({ type: 'positive', message: 'Harcama oluşturuldu' });
    }
    showCreateDialog.value = false;
    await loadExpenses();
  } catch (error) {
    notifyApiError($q, error, 'Harcama kaydedilemedi');
  } finally {
    creating.value = false;
  }
}

async function submitExpense(id: string) {
  try {
    await expense.submitExpense(id);
    await loadExpenses();
    $q.notify({ type: 'positive', message: 'Harcama onaya gönderildi' });
  } catch (error) {
    notifyApiError($q, error, 'Gönderme başarısız');
  }
}

async function approveExpense(id: string) {
  try {
    await expense.approveExpense(id);
    await loadExpenses();
    $q.notify({ type: 'positive', message: 'Harcama onaylandı' });
  } catch (error) {
    notifyApiError($q, error, 'Onay başarısız');
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
    await expense.rejectExpense(rejectTargetId.value, { reason: rejectReason.value });
    showRejectDialog.value = false;
    await loadExpenses();
    $q.notify({ type: 'positive', message: 'Harcama reddedildi' });
  } catch (error) {
    notifyApiError($q, error, 'Reddetme başarısız');
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
      async () => {
        await loadExpenses();
        $q.notify({ type: 'positive', message: 'Harcama silindi' });
      },
      (error) => notifyApiError($q, error, 'Silme başarısız'),
    );
  });
}

onMounted(async () => {
  await loadExpenses();
});
</script>

<style scoped>
.responsive-dialog {
  width: 520px;
  max-width: 92vw;
}

.detail-dialog {
  width: 680px;
}

.id-text {
  overflow-wrap: anywhere;
}

.action-cell {
  display: flex;
  flex-wrap: wrap;
  gap: 2px;
  justify-content: center;
  min-width: 132px;
  white-space: normal;
}
</style>
