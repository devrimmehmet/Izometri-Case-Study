import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  // Gate - lock screen
  {
    path: '/',
    component: () => import('pages/GatePage.vue'),
  },

  // Login
  {
    path: '/login',
    component: () => import('pages/LoginPage.vue'),
    meta: { requiresGate: true },
  },

  // Authenticated area
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    meta: { requiresAuth: true, requiresGate: true },
    children: [
      { path: 'dashboard', component: () => import('pages/DashboardPage.vue') },
      { path: 'expenses', component: () => import('pages/ExpensesPage.vue') },
      {
        path: 'admin/users',
        component: () => import('pages/AdminUsersPage.vue'),
        meta: { roles: ['Admin'] },
      },
      {
        path: 'admin/settings',
        component: () => import('pages/SettingsPage.vue'),
        meta: { roles: ['Admin'] },
      },
      { path: 'notifications', component: () => import('pages/NotificationsPage.vue') },
    ],
  },

  // Catch-all
  {
    path: '/:catchAll(.*)*',
    component: () => import('pages/ErrorNotFound.vue'),
  },
];

export default routes;
