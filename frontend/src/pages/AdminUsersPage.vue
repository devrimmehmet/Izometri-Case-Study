<template>
  <q-page class="q-pa-lg">
    <div class="row items-center q-mb-lg">
      <div>
        <div class="text-h5 text-weight-bold">Kullanıcı Yönetimi</div>
        <div class="text-grey-5 text-body2">Tenant kullanıcılarını yönetin</div>
      </div>
      <q-space />
      <q-btn
        color="primary"
        icon="person_add"
        label="Yeni Kullanıcı"
        rounded
        unelevated
        @click="showCreate = true"
      />
    </div>

    <div class="glass-card q-pa-md">
      <q-table
        :rows="admin.users"
        :columns="columns"
        row-key="id"
        flat
        dark
        :loading="admin.loading"
        class="bg-transparent"
        no-data-label="Kayıt bulunamadı"
      >
        <template #body-cell-roles="props">
          <q-td :props="props">
            <q-badge
              v-for="role in props.row.roles"
              :key="role"
              :color="roleColor(role)"
              class="q-mr-xs"
              outline
            >
              {{ translateRole(role) }}
            </q-badge>
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
              icon="edit"
              color="primary"
              @click="openEditRoles(props.row)"
            >
              <q-tooltip>Rolleri Düzenle</q-tooltip>
            </q-btn>
          </q-td>
        </template>
      </q-table>
    </div>

    <!-- Yeni Kullanıcı Dialogu -->
    <q-dialog v-model="showCreate" persistent>
      <q-card dark style="min-width: 440px" class="glass-card">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Yeni Kullanıcı</div>
        </q-card-section>
        <q-card-section>
          <q-form @submit.prevent="onCreate" class="q-gutter-md">
            <q-input v-model="createForm.displayName" label="Ad Soyad" outlined dark dense
              :rules="[(v: string) => !!v || 'Zorunlu alan']" />
            <q-input v-model="createForm.email" label="E-posta" type="email" outlined dark dense
              :rules="[(v: string) => !!v || 'Zorunlu alan']" />
            <q-input v-model="createForm.password" label="Şifre" type="password" outlined dark dense
              :rules="[(v: string) => v.length >= 6 || 'En az 6 karakter']" />
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

    <!-- Rol Düzenleme Dialogu -->
    <q-dialog v-model="showEditRoles" persistent>
      <q-card dark style="min-width: 400px" class="glass-card">
        <q-card-section>
          <div class="text-h6 text-weight-bold">Rolleri Düzenle</div>
          <div class="text-grey-5 text-caption">{{ editUser?.email }}</div>
        </q-card-section>
        <q-card-section>
          <q-select
            v-model="editRoles"
            :options="roleOptions"
            label="Roller"
            outlined
            dark
            dense
            multiple
            emit-value
            map-options
          />
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
import { ref, reactive, onMounted } from 'vue';
import { useQuasar } from 'quasar';
import { useAdminStore } from 'stores/admin';
import type { UserDto, CreateUserRequest } from 'src/types';
import { translateRole, formatDate } from 'src/utils/tr';

const $q = useQuasar();
const admin = useAdminStore();

const roleOptions = [
  { label: 'Yönetici', value: 'Admin' },
  { label: 'İnsan Kaynakları', value: 'HR' },
  { label: 'Personel', value: 'Personel' },
];

const columns = [
  { name: 'email', label: 'E-posta', field: 'email', align: 'left' as const },
  { name: 'displayName', label: 'Ad Soyad', field: 'displayName', align: 'left' as const },
  { name: 'roles', label: 'Roller', field: 'roles', align: 'left' as const },
  { name: 'createdAt', label: 'Kayıt Tarihi', field: 'createdAt', align: 'left' as const },
  { name: 'actions', label: 'İşlemler', field: '', align: 'center' as const },
];

// Oluşturma
const showCreate = ref(false);
const creating = ref(false);
const createForm = reactive<CreateUserRequest>({
  email: '',
  password: 'Pass123!',
  displayName: '',
  roles: ['Personel'],
});

// Rol düzenleme
const showEditRoles = ref(false);
const saving = ref(false);
const editUser = ref<UserDto | null>(null);
const editRoles = ref<string[]>([]);

function roleColor(role: string): string {
  switch (role) {
    case 'Admin': return 'purple';
    case 'HR': return 'blue';
    default: return 'teal';
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
    await admin.createUser(createForm);
    showCreate.value = false;
    createForm.email = '';
    createForm.displayName = '';
    $q.notify({ type: 'positive', message: 'Kullanıcı başarıyla oluşturuldu' });
  } catch (err: unknown) {
    const error = err as { response?: { data?: { detail?: string } }; message?: string };
    $q.notify({ type: 'negative', message: error.response?.data?.detail ?? 'Bir hata oluştu' });
  } finally {
    creating.value = false;
  }
}

async function onSaveRoles() {
  if (!editUser.value) return;
  saving.value = true;
  try {
    await admin.updateRoles(editUser.value.id, { roles: editRoles.value });
    showEditRoles.value = false;
    $q.notify({ type: 'positive', message: 'Roller başarıyla güncellendi' });
  } catch (err: unknown) {
    const error = err as { response?: { data?: { detail?: string } }; message?: string };
    $q.notify({ type: 'negative', message: error.response?.data?.detail ?? 'Bir hata oluştu' });
  } finally {
    saving.value = false;
  }
}

onMounted(async () => {
  await admin.fetchUsers();
});
</script>
