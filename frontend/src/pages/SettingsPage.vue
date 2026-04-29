<template>
  <q-page class="q-pa-lg">
    <div class="row items-center q-mb-lg">
      <div>
        <div class="text-h5 text-weight-bold">Sistem Ayarları</div>
        <div class="text-grey-5 text-body2">Döviz kurları ve diğer temel ayarlar</div>
      </div>
    </div>

    <div class="row q-col-gutter-md">
      <div class="col-12 col-md-6">
        <q-card dark class="glass-card">
          <q-inner-loading :showing="loading">
            <q-spinner color="primary" size="32px" />
          </q-inner-loading>
          <q-card-section>
            <div class="row items-center q-gutter-sm q-mb-sm">
              <div class="text-h6">Döviz Kurları</div>
              <q-badge :color="form.fixedUsdRate || form.fixedEurRate ? 'warning' : 'positive'" outline>
                {{ form.fixedUsdRate || form.fixedEurRate ? 'Sabit kur aktif' : 'TCMB canlı kur kullanılıyor' }}
              </q-badge>
            </div>
            <div class="text-caption text-grey-5">
              Alanları boş bırakırsanız sistem 5.000 TL limitini güncel TCMB kuruyla hesaplar.
              Sabit kur girmek isterseniz aşağıdan belirleyebilirsiniz.
            </div>
          </q-card-section>

          <q-card-section>
            <DataState
              v-if="errorMessage"
              title="Ayarlar yüklenemedi"
              :message="errorMessage"
              icon="error_outline"
              color="negative"
              retry-label="Tekrar dene"
              dense
              @retry="loadRates"
            />
            <q-form @submit.prevent="saveRates" class="q-gutter-md">
              <q-input
                v-model.number="form.fixedUsdRate"
                label="Sabit USD Kur"
                type="number"
                step="0.0001"
                outlined
                dark
                dense
                color="primary"
                clearable
                :hint="usdHint"
              >
                <template #prepend>
                  <q-icon name="attach_money" color="grey-5" />
                </template>
                <template #append v-if="currentRates.usd">
                  <q-btn flat round dense icon="content_copy" size="sm" @click="form.fixedUsdRate = currentRates.usd">
                    <q-tooltip>Canlı kuru kopyala</q-tooltip>
                  </q-btn>
                </template>
              </q-input>

              <q-input
                v-model.number="form.fixedEurRate"
                label="Sabit EUR Kur"
                type="number"
                step="0.0001"
                outlined
                dark
                dense
                color="primary"
                clearable
                :hint="eurHint"
              >
                <template #prepend>
                  <q-icon name="euro" color="grey-5" />
                </template>
                <template #append v-if="currentRates.eur">
                  <q-btn flat round dense icon="content_copy" size="sm" @click="form.fixedEurRate = currentRates.eur">
                    <q-tooltip>Canlı kuru kopyala</q-tooltip>
                  </q-btn>
                </template>
              </q-input>

              <div class="row justify-end q-mt-md">
                <q-btn
                  type="submit"
                  label="Kaydet"
                  color="primary"
                  rounded
                  unelevated
                  :loading="saving"
                />
              </div>
            </q-form>
          </q-card-section>
        </q-card>
      </div>

      <div class="col-12 col-md-6">
        <q-card dark class="glass-card">
          <q-card-section>
            <div class="text-h6">5.000 TL Onay Eşiği</div>
            <div class="text-caption text-grey-5">
              USD/EUR tutarı girerek TRY karşılığını ve admin onayı gerekip gerekmediğini kontrol edin.
            </div>
          </q-card-section>
          <q-card-section class="q-gutter-md">
            <div class="row q-col-gutter-sm">
              <div class="col-8">
                <q-input
                  v-model.number="calculator.amount"
                  label="Tutar"
                  type="number"
                  outlined
                  dark
                  dense
                  color="primary"
                />
              </div>
              <div class="col-4">
                <q-select
                  v-model="calculator.currency"
                  :options="currencyOptions"
                  label="Birim"
                  outlined
                  dark
                  dense
                  emit-value
                  map-options
                />
              </div>
            </div>
            <q-banner rounded class="bg-grey-10 text-grey-2">
              <div class="text-body2">TRY karşılığı: {{ calculatedTry.toLocaleString('tr-TR') }} TL</div>
              <div :class="calculatedTry > 5000 ? 'text-warning' : 'text-positive'">
                {{ calculatedTry > 5000 ? 'Admin onayı gerekir.' : 'Admin onayı gerekmez.' }}
              </div>
            </q-banner>
          </q-card-section>
        </q-card>
      </div>
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useQuasar } from 'quasar';
import { api } from 'src/services/http';
import DataState from 'components/DataState.vue';
import type { ExchangeRateResponse, UpdateRatesRequest } from 'src/types';
import { notifyApiError } from 'src/utils/errors';

const $q = useQuasar();
const saving = ref(false);
const loading = ref(false);
const errorMessage = ref('');

const form = reactive({
  fixedUsdRate: null as number | null,
  fixedEurRate: null as number | null,
});

const currentRates = reactive({
  usd: null as number | null,
  eur: null as number | null,
});

const calculator = reactive({
  amount: 100,
  currency: 'USD',
});

const currencyOptions = [
  { label: 'USD', value: 'USD' },
  { label: 'EUR', value: 'EUR' },
];

const effectiveUsdRate = computed(() => form.fixedUsdRate || currentRates.usd || 0);
const effectiveEurRate = computed(() => form.fixedEurRate || currentRates.eur || 0);

const calculatedTry = computed(() => {
  const rate = calculator.currency === 'USD' ? effectiveUsdRate.value : effectiveEurRate.value;
  return Number(((calculator.amount || 0) * rate).toFixed(2));
});

const usdHint = computed(() => {
  if (!currentRates.usd) return '';
  if (!form.fixedUsdRate) return `Şu anki TCMB kuru: ${currentRates.usd} TL`;
  const diff = form.fixedUsdRate - currentRates.usd;
  return `TCMB: ${currentRates.usd} TL, sabit kur farkı: ${diff.toFixed(4)} TL`;
});

const eurHint = computed(() => {
  if (!currentRates.eur) return '';
  if (!form.fixedEurRate) return `Şu anki TCMB kuru: ${currentRates.eur} TL`;
  const diff = form.fixedEurRate - currentRates.eur;
  return `TCMB: ${currentRates.eur} TL, sabit kur farkı: ${diff.toFixed(4)} TL`;
});

async function loadRates() {
  errorMessage.value = '';
  loading.value = true;
  try {
    const { data } = await api.get<ExchangeRateResponse>('/settings/exchange-rates');
    form.fixedUsdRate = data.fixedUsdRate ?? null;
    form.fixedEurRate = data.fixedEurRate ?? null;
    currentRates.usd = data.currentUsdRate;
    currentRates.eur = data.currentEurRate;
  } catch (error) {
    errorMessage.value = 'Döviz kuru bilgileri backend servisinden alınamadı.';
    notifyApiError($q, error, 'Ayarlar yüklenemedi');
  } finally {
    loading.value = false;
  }
}

async function saveRates() {
  saving.value = true;
  try {
    const payload: UpdateRatesRequest = {
      fixedUsdRate: form.fixedUsdRate || null,
      fixedEurRate: form.fixedEurRate || null,
    };
    await api.put('/settings/exchange-rates', payload);
    await loadRates();
    $q.notify({ type: 'positive', message: 'Ayarlar kaydedildi' });
  } catch (error) {
    notifyApiError($q, error, 'Kaydetme başarısız');
  } finally {
    saving.value = false;
  }
}

onMounted(() => {
  void loadRates();
});
</script>
