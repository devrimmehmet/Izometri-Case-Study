<template>
  <q-layout view="hHh LpR fFf" class="bg-dark-page">
    <q-header class="bg-transparent">
      <q-toolbar class="q-px-lg" style="backdrop-filter: blur(16px); background: rgba(19, 17, 28, 0.8); border-bottom: 1px solid rgba(99, 102, 241, 0.1);">
        <q-btn flat dense round icon="menu" @click="toggleDrawer" />

        <q-toolbar-title class="text-weight-bold" style="font-size: 18px;">
          <span style="background: linear-gradient(135deg, #6366f1, #8b5cf6); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;">
            İzometri
          </span>
          <span class="text-grey-5 text-caption q-ml-sm">Harcama Yönetimi</span>
        </q-toolbar-title>

        <q-space />

        <!-- User Info -->
        <div class="row items-center q-gutter-sm">
          <q-badge :color="roleColor" outline class="q-mr-sm">
            {{ translateRole(auth.displayRole) }}
          </q-badge>
          <span class="text-grey-4 text-caption">{{ auth.email }}</span>
          <q-btn flat dense round icon="logout" color="grey-5" @click="onLogout">
            <q-tooltip>Çıkış Yap</q-tooltip>
          </q-btn>
        </div>
      </q-toolbar>
    </q-header>

    <q-drawer
      v-model="drawer"
      show-if-above
      bordered
      :width="260"
      style="background: rgba(19, 17, 28, 0.95); border-right: 1px solid rgba(99, 102, 241, 0.1);"
    >
      <q-list dark class="q-pt-md">
        <!-- Brand -->
        <q-item class="q-mb-lg q-px-lg">
          <q-item-section>
            <div class="text-h6 text-weight-bold" style="background: linear-gradient(135deg, #6366f1, #8b5cf6); -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;">
              İzometri
            </div>
            <div class="text-grey-6 text-caption">
              {{ auth.tenantCode.toUpperCase() }} Tenant
            </div>
          </q-item-section>
        </q-item>

        <q-separator dark class="q-mb-sm" />

        <!-- Navigation -->
        <q-item
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          clickable
          active-class="nav-active"
          class="q-mx-sm rounded-borders q-mb-xs"
        >
          <q-item-section avatar>
            <q-icon :name="item.icon" :color="isActive(item.path) ? 'primary' : 'grey-5'" />
          </q-item-section>
          <q-item-section>
            <q-item-label :class="isActive(item.path) ? 'text-primary text-weight-bold' : 'text-grey-4'">
              {{ item.label }}
            </q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from 'stores/auth';
import { translateRole } from 'src/utils/tr';

const router = useRouter();
const route = useRoute();
const auth = useAuthStore();
const drawer = ref(false);

interface NavItem {
  label: string;
  icon: string;
  path: string;
  roles?: string[];
}

const allNavItems: NavItem[] = [
  { label: 'Gösterge Paneli', icon: 'dashboard', path: '/dashboard' },
  { label: 'Harcamalar', icon: 'receipt_long', path: '/expenses' },
  { label: 'Kullanıcılar', icon: 'people', path: '/admin/users', roles: ['Admin'] },
  { label: 'Sistem Ayarları', icon: 'settings', path: '/admin/settings', roles: ['Admin'] },
  { label: 'Bildirimler', icon: 'notifications', path: '/notifications' },
];

const navItems = computed(() =>
  allNavItems.filter((item) => {
    if (!item.roles) return true;
    return item.roles.some((r) => auth.roles.includes(r as 'Admin' | 'HR' | 'Personel'));
  }),
);

const roleColor = computed(() => {
  if (auth.isAdmin) return 'purple';
  if (auth.isHR) return 'blue';
  return 'teal';
});

function isActive(path: string): boolean {
  return route.path === path;
}

function toggleDrawer() {
  drawer.value = !drawer.value;
}

function onLogout() {
  auth.logout();
  void router.push('/login');
}
</script>

<style scoped>
.nav-active {
  background: rgba(99, 102, 241, 0.12) !important;
}
</style>
