<template>
  <div class="gate-bg flex flex-center">
    <div class="gate-card q-pa-xl" style="width: 400px; max-width: 90vw">
      <div class="text-center q-mb-lg">
        <q-icon
          name="lock"
          size="48px"
          color="grey-6"
          class="q-mb-md"
        />
        <div class="text-grey-5 text-body2">
          Bu sayfa korumalıdır
        </div>
      </div>

      <q-form @submit.prevent="onSubmit" class="q-gutter-md">
        <q-input
          v-model="password"
          type="password"
          label="Erişim Kodu"
          outlined
          dark
          dense
          :error="showError"
          error-message="Geçersiz erişim kodu"
          color="primary"
          @update:model-value="showError = false"
        >
          <template #prepend>
            <q-icon name="vpn_key" color="grey-5" />
          </template>
        </q-input>

        <q-btn
          type="submit"
          label="Giriş"
          color="primary"
          class="full-width"
          :loading="loading"
          rounded
          unelevated
        />
      </q-form>

      <div class="text-center q-mt-lg">
        <div class="text-grey-7 text-caption">
          © {{ new Date().getFullYear() }} — Güvenli Erişim
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from 'stores/auth';

const GATE_PASSWORD = 'D19M23P';

const password = ref('');
const showError = ref(false);
const loading = ref(false);
const router = useRouter();
const auth = useAuthStore();

function onSubmit() {
  loading.value = true;
  setTimeout(() => {
    if (password.value === GATE_PASSWORD) {
      auth.unlockGate();
      void router.push('/login');
    } else {
      showError.value = true;
    }
    loading.value = false;
  }, 500);
}
</script>
