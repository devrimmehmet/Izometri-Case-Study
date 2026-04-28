<template>
  <div class="gate-bg flex flex-center">
    <div class="gate-card q-pa-xl" style="width: 460px; max-width: 92vw">
      <div class="text-center q-mb-lg">
        <div
          class="text-h5 text-weight-bold"
          style="
            background: linear-gradient(135deg, #6366f1, #8b5cf6);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            background-clip: text;
          "
        >
          İzometri
        </div>
        <div class="text-grey-5 text-body2 q-mt-xs">
          Kurumsal Harcama Yönetim Sistemi
        </div>
      </div>

      <q-form @submit.prevent="onLogin" class="q-gutter-md">
        <q-select
          v-model="form.tenantCode"
          :options="tenantOptions"
          label="Şirket (Tenant)"
          outlined
          dark
          dense
          emit-value
          map-options
          color="primary"
          :rules="[(v: string) => !!v || 'Şirket seçiniz']"
        >
          <template #prepend>
            <q-icon name="business" color="grey-5" />
          </template>
        </q-select>

        <q-input
          v-model="form.email"
          label="E-posta"
          type="email"
          outlined
          dark
          dense
          color="primary"
          :rules="[(v: string) => !!v || 'E-posta gerekli']"
        >
          <template #prepend>
            <q-icon name="email" color="grey-5" />
          </template>
        </q-input>

        <q-input
          v-model="form.password"
          label="Şifre"
          :type="showPwd ? 'text' : 'password'"
          outlined
          dark
          dense
          color="primary"
          :rules="[(v: string) => !!v || 'Şifre gerekli']"
        >
          <template #prepend>
            <q-icon name="lock" color="grey-5" />
          </template>
          <template #append>
            <q-icon
              :name="showPwd ? 'visibility_off' : 'visibility'"
              class="cursor-pointer"
              color="grey-5"
              @click="showPwd = !showPwd"
            />
          </template>
        </q-input>

        <q-btn
          type="submit"
          label="Giriş Yap"
          color="primary"
          class="full-width q-mt-md"
          :loading="loading"
          rounded
          unelevated
          size="md"
        />

        <div v-if="errorMsg" class="text-negative text-center text-caption q-mt-sm">
          {{ errorMsg }}
        </div>
      </q-form>

      <q-separator dark class="q-my-md" />

      <!-- Tıklanabilir demo hesaplar -->
      <div class="text-grey-5 text-caption q-mb-sm">
        Hızlı giriş için bir hesaba tıklayın <span class="text-grey-7">(Şifre: Pass123!)</span>
      </div>

      <div v-for="group in demoGroups" :key="group.tenant" class="q-mb-sm">
        <div
          class="text-caption text-weight-bold q-mb-xs"
          :style="{ color: group.color }"
        >
          {{ group.label }}
        </div>
        <div class="row q-gutter-xs">
          <q-chip
            v-for="user in group.users"
            :key="user.email"
            clickable
            dense
            dark
            outline
            :color="selectedEmail === user.email ? 'primary' : 'grey-7'"
            :text-color="selectedEmail === user.email ? 'white' : 'grey-4'"
            size="sm"
            class="demo-chip"
            @click="fillUser(group.tenant, user.email)"
          >
            <q-icon :name="roleIcon(user.role)" size="14px" class="q-mr-xs" />
            {{ user.label }}
          </q-chip>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from 'stores/auth';

const router = useRouter();
const auth = useAuthStore();

const tenantOptions = [
  { label: 'İzometri Bilişim', value: 'izometri' },
  { label: 'TEST1', value: 'test1' },
  { label: 'TEST2', value: 'test2' },
];

interface DemoUser {
  email: string;
  role: string;
  label: string;
}

interface DemoGroup {
  tenant: string;
  label: string;
  color: string;
  users: DemoUser[];
}

const demoGroups: DemoGroup[] = [
  {
    tenant: 'izometri',
    label: 'İzometri',
    color: '#6366f1',
    users: [
      { email: 'admin@izometri.com', role: 'Admin', label: 'Yönetici' },
      { email: 'hr@izometri.com', role: 'HR', label: 'İK' },
      { email: 'personel@izometri.com', role: 'Personel', label: 'Personel 1' },
      { email: 'personel2@izometri.com', role: 'Personel', label: 'Personel 2' },
    ],
  },
  {
    tenant: 'test1',
    label: 'TEST1',
    color: '#8b5cf6',
    users: [
      { email: 'pattabanoglu@devrimmehmet.com', role: 'Admin', label: 'Yönetici' },
      { email: 'devrimmehmet@gmail.com', role: 'HR', label: 'İK' },
      { email: 'devrimmehmet@msn.com', role: 'Personel', label: 'Personel 1' },
      { email: 'personel2@test1.com', role: 'Personel', label: 'Personel 2' },
    ],
  },
  {
    tenant: 'test2',
    label: 'TEST2',
    color: '#06b6d4',
    users: [
      { email: 'admin@test2.com', role: 'Admin', label: 'Yönetici' },
      { email: 'hr@test2.com', role: 'HR', label: 'İK' },
      { email: 'personel@test2.com', role: 'Personel', label: 'Personel 1' },
      { email: 'personel2@test2.com', role: 'Personel', label: 'Personel 2' },
    ],
  },
];

const form = reactive({
  email: '',
  password: '',
  tenantCode: 'test1',
});

const showPwd = ref(false);
const loading = ref(false);
const errorMsg = ref('');
const selectedEmail = ref('');

function fillUser(tenant: string, email: string) {
  form.tenantCode = tenant;
  form.email = email;
  form.password = 'Pass123!';
  selectedEmail.value = email;
}

function roleIcon(role: string): string {
  switch (role) {
    case 'Admin': return 'admin_panel_settings';
    case 'HR': return 'badge';
    default: return 'person';
  }
}

async function onLogin() {
  loading.value = true;
  errorMsg.value = '';
  try {
    await auth.login(form);
    void router.push('/dashboard');
  } catch (err: unknown) {
    const error = err as { response?: { data?: { detail?: string } }; message?: string };
    errorMsg.value =
      error.response?.data?.detail ?? error.message ?? 'Giriş başarısız';
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.demo-chip {
  transition: all 0.2s ease;
  cursor: pointer;
}
.demo-chip:hover {
  border-color: #6366f1 !important;
  transform: translateY(-1px);
}
</style>
