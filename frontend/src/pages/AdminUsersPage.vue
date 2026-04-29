<template>
  <q-page class="q-pa-lg">
    <div class="row items-center q-mb-lg">
      <div>
        <div class="text-h5 text-weight-bold">Kullanıcı Yönetimi</div>
        <div class="text-grey-5 text-body2">Tenant kullanıcılarını yönetin</div>
      </div>
      <q-space />
      <q-btn color="primary" icon="person_add" label="Yeni Kullanıcı" rounded unelevated @click="showCreate = true" />
    </div>

    <div class="glass-card q-pa-md">
      <q-banner v-if="errorMessage" rounded class="bg-negative text-white q-mb-md">
        {{ errorMessage }}
        <template #action>
          <q-btn flat color="white" icon="refresh" label="Tekrar dene" @click="loadUsers" />
        </template>
      </q-banner>
      <q-table
        :rows="admin.users"
        :columns="columns"
        :grid="$q.screen.xs"
        row-key="id"
        flat
        dark
        :loading="admin.loading"
        loading-label="Yükleniyor..."
        class="bg-transparent"
      >
        <template #no-data>
          <DataState
            :title="errorMessage ? 'Kullanıcılar yüklenemedi' : 'Kayıt bulunamadı'"
            :message="errorMessage || 'Bu tenant için kullanıcı kaydı bulunamadı.'"
            :icon="errorMessage ? 'error_outline' : 'inbox'"
            :color="errorMessage ? 'negative' : 'grey-5'"
            :retry-label="errorMessage ? 'Tekrar dene' : ''"
            dense
            @retry="loadUsers"
          />
        </template>
        <template #body-cell-roles="props">
          <q-td :props="props">
            <q-badge v-for="role in props.row.roles" :key="role" :color="roleColor(role)" class="q-mr-xs" outline>
              {{ translateRole(role) }}
            </q-badge>
          </q-td>
        </template>
        <template #body-cell-createdAt="props">
          <q-td :props="props">{{ formatDate(props.row.createdAt) }}</q-td>
        </template>
        <template #body-cell-actions="props">
          <q-td :props="props" class="action-cell">
            <q-btn flat dense round icon="edit" color="primary" @click="openEditRoles(props.row)">
              <q-tooltip>Rolleri düzenle</q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </div>

    <q-dialog v-model="showCreate" persistent>
      <q-card dark class="glass-card responsive-dialog">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Yeni Kullanıcı</div>
        </q-card-section>
        <q-card-section>
          <q-form @submit.prevent="onCreate" class="q-gutter-md">
            <q-input v-model="createForm.displayName" label="Ad Soyad" outlined dark dense :rules="[(v: string) => !!v || 'Zorunlu alan']" />
            <q-input v-model="createForm.email" label="E-posta" type="email" outlined dark dense :rules="emailRules" />
            <q-input v-model="createForm.phone" label="Telefon (opsiyonel)" outlined dark dense />
            <q-input
              v-model="createForm.password"
              label="Şifre"
              type="password"
              outlined
              dark
              dense
              :rules="[(v: string) => v.length >= 8 || 'Backend kuralı: en az 8 karakter']"
            />
            <q-select
              v-model="createForm.roles"
              :options="roleOptions"
              label="Roller"
              outlined
              dark
              dense
              multiple
              emit-value
              map-options
              hint="En az bir rol seçilmelidir."
              :rules="[(v: string[]) => v.length > 0 || 'En az bir rol seçiniz']"
            />
            <div class="row justify-end q-gutter-sm">
              <q-btn flat label="İptal" color="grey" @click="showCreate = false" />
              <q-btn type="submit" label="Oluştur" color="primary" rounded unelevated :loading="creating" />
            </div>
          </q-form>
        </q-card-section>
      </q-card>
    </q-dialog>

    <q-dialog v-model="showEditRoles" persistent>
      <q-card dark class="glass-card responsive-dialog">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Rolleri Düzenle</div>
          <div class="text-grey-5 text-caption">{{ editUser?.email }}</div>
        </q-card-section>
        <q-card-section class="q-gutter-md">
          <q-select v-model="editRoles" :options="roleOptions" label="Roller" outlined dark dense multiple emit-value map-options />
          <div class="row q-col-gutter-sm">
            <div class="col-12 col-sm-6">
              <div class="text-caption text-grey-5 q-mb-xs">Eklenecek roller</div>
              <q-badge v-for="role in addedRoles" :key="role" color="positive" outline class="q-mr-xs">
                {{ translateRole(role) }}
              </q-badge>
              <span v-if="addedRoles.length === 0" class="text-grey-6 text-caption">Yok</span>
            </div>
            <div class="col-12 col-sm-6">
              <div class="text-caption text-grey-5 q-mb-xs">Kaldırılacak roller</div>
              <q-badge v-for="role in removedRoles" :key="role" color="negative" outline class="q-mr-xs">
                {{ translateRole(role) }}
              </q-badge>
              <span v-if="removedRoles.length === 0" class="text-grey-6 text-caption">Yok</span>
            </div>
          </div>
        </q-card-section>
        <q-card-actions align="right">
          <q-btn flat label="İptal" color="grey" @click="showEditRoles = false" />
          <q-btn label="Kaydet" color="primary" rounded unelevated :loading="saving" @click="onSaveRoles" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </q-page>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useQuasar } from 'quasar';
import { useAdminStore } from 'stores/admin';
import DataState from 'components/DataState.vue';
import type { CreateUserRequest, UserDto } from 'src/types';
import { formatDate, translateRole } from 'src/utils/tr';
import { notifyApiError } from 'src/utils/errors';

const $q = useQuasar();
const admin = useAdminStore();
const errorMessage = ref('');

const roleOptions = [
  { label: 'Yönetici', value: 'Admin' },
  { label: 'İnsan Kaynakları', value: 'HR' },
  { label: 'Personel', value: 'Personel' },
];

const emailRules = [
  (v: string) => !!v || 'Zorunlu alan',
  (v: string) => /.+@.+\..+/.test(v) || 'Geçerli bir e-posta girin',
];

const columns = [
  { name: 'email', label: 'E-posta', field: 'email', align: 'left' as const },
  { name: 'displayName', label: 'Ad Soyad', field: 'displayName', align: 'left' as const },
  { name: 'roles', label: 'Roller', field: 'roles', align: 'left' as const },
  { name: 'createdAt', label: 'Kayıt Tarihi', field: 'createdAt', align: 'left' as const },
  { name: 'actions', label: 'İşlemler', field: '', align: 'center' as const },
];

const showCreate = ref(false);
const creating = ref(false);
const createForm = reactive<CreateUserRequest>({
  email: '',
  password: 'Pass123!',
  displayName: '',
  roles: ['Personel'],
  phone: '',
});

const showEditRoles = ref(false);
const saving = ref(false);
const editUser = ref<UserDto | null>(null);
const editRoles = ref<string[]>([]);

const addedRoles = computed(() => editRoles.value.filter((role) => !editUser.value?.roles.includes(role)));
const removedRoles = computed(() => editUser.value?.roles.filter((role) => !editRoles.value.includes(role)) ?? []);

function roleColor(role: string): string {
  switch (role) {
    case 'Admin':
      return 'purple';
    case 'HR':
      return 'blue';
    default:
      return 'teal';
  }
}

function openEditRoles(user: UserDto) {
  editUser.value = user;
  editRoles.value = [...user.roles];
  showEditRoles.value = true;
}

async function onCreate() {
  creating.value = true;
  try {
    const payload: CreateUserRequest = {
      email: createForm.email,
      password: createForm.password,
      displayName: createForm.displayName,
      roles: createForm.roles,
    };
    if (createForm.phone) payload.phone = createForm.phone;

    await admin.createUser(payload);
    showCreate.value = false;
    createForm.email = '';
    createForm.displayName = '';
    createForm.phone = '';
    $q.notify({ type: 'positive', message: 'Kullanıcı oluşturuldu ve Keycloak senkronizasyonu tamamlandı' });
  } catch (error) {
    notifyApiError($q, error, 'Kullanıcı oluşturulamadı');
  } finally {
    creating.value = false;
  }
}

async function onSaveRoles() {
  if (!editUser.value) return;
  if (editRoles.value.length === 0) {
    $q.notify({ type: 'warning', message: 'En az bir rol seçiniz' });
    return;
  }

  saving.value = true;
  try {
    await admin.updateRoles(editUser.value.id, { roles: editRoles.value });
    showEditRoles.value = false;
    $q.notify({ type: 'positive', message: 'Roller güncellendi' });
  } catch (error) {
    notifyApiError($q, error, 'Roller güncellenemedi');
  } finally {
    saving.value = false;
  }
}

async function loadUsers() {
  errorMessage.value = '';
  try {
    await admin.fetchUsers();
  } catch (error) {
    errorMessage.value = 'Kullanıcı listesi yüklenemedi.';
    notifyApiError($q, error, 'Kullanıcılar yüklenemedi');
  }
}

onMounted(async () => {
  await loadUsers();
});
</script>

<style scoped>
.responsive-dialog {
  width: 480px;
  max-width: 92vw;
}

.action-cell {
  display: flex;
  flex-wrap: wrap;
  gap: 2px;
  justify-content: center;
  min-width: 56px;
  white-space: normal;
}
</style>
