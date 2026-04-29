import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  // Root → login
  {
    path: '/',
    redirect: '/login',
  },

  // Login
  {
    path: '/login',
    component: () => import('pages/LoginPage.vue'),
  },

  // Authenticated area
  {
    path: '/',
    component: () => import('layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
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
      {
        path: 'admin/operations',
        component: () => import('pages/AdminOperationsPage.vue'),
        meta: { roles: ['Admin'] },
      },
      {
        path: 'admin/docs',
        component: () => import('pages/DocsPage.vue'),
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
