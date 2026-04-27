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
          <q-card-section>
            <div class="text-h6">Döviz Kurları (Sabit)</div>
            <div class="text-caption text-grey-5">
              Eğer bu alanları boş bırakırsanız, sistem harcamalarda 5000 TL limitini hesaplarken otomatik olarak güncel TCMB kurunu kullanır. Sabit bir kur girmek isterseniz aşağıdan belirleyebilirsiniz.
            </div>
          </q-card-section>
          <q-card-section>
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
                :hint="currentRates.usd ? `Şu anki TCMB kuru: ${currentRates.usd} TL` : ''"
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
                :hint="currentRates.eur ? `Şu anki TCMB kuru: ${currentRates.eur} TL` : ''"
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
    </div>
  </q-page>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import { api } from 'src/boot/axios';

const $q = useQuasar();
const saving = ref(false);

const form = reactive({
  fixedUsdRate: null as number | null,
  fixedEurRate: null as number | null,
});

const currentRates = reactive({
  usd: null as number | null,
  eur: null as number | null,
});

async function loadRates() {
  try {
    const { data } = await api.get('/settings/exchange-rates');
    form.fixedUsdRate = data.fixedUsdRate;
    form.fixedEurRate = data.fixedEurRate;
    currentRates.usd = data.currentUsdRate;
    currentRates.eur = data.currentEurRate;
  } catch {
    $q.notify({ type: 'negative', message: 'Ayarlar yüklenemedi' });
  }
}

async function saveRates() {
  saving.value = true;
  try {
    await api.put('/settings/exchange-rates', {
      fixedUsdRate: form.fixedUsdRate || null,
      fixedEurRate: form.fixedEurRate || null,
    });
    $q.notify({ type: 'positive', message: 'Ayarlar başarıyla kaydedildi' });
  } catch {
    $q.notify({ type: 'negative', message: 'Kaydetme başarısız' });
  } finally {
    saving.value = false;
  }
}

onMounted(() => {
  void loadRates();
});
</script>
