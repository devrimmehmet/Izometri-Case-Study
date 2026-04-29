import { defineRouter } from '#q-app/wrappers';
import {
  createMemoryHistory,
  createRouter,
  createWebHashHistory,
  createWebHistory,
} from 'vue-router';
import routes from './routes';
import { useAuthStore } from 'stores/auth';

export default defineRouter((/* { store, ssrContext } */) => {
  const createHistory = process.env.SERVER
    ? createMemoryHistory
    : process.env.VUE_ROUTER_MODE === 'history'
      ? createWebHistory
      : createWebHashHistory;

  const Router = createRouter({
    scrollBehavior: () => ({ left: 0, top: 0 }),
    routes,
    history: createHistory(process.env.VUE_ROUTER_BASE),
  });

  Router.beforeEach((to) => {
    const auth = useAuthStore();

    if (!auth.isAuthenticated) {
      auth.restoreSession();
    }

    // Auth required but not authenticated → login
    if (to.meta.requiresAuth && !auth.isAuthenticated) {
      return '/login';
    }

    // Role check
    const requiredRoles = to.meta.roles;
    if (requiredRoles && requiredRoles.length > 0) {
      const hasRole = requiredRoles.some((r) =>
        auth.roles.includes(r as 'Admin' | 'HR' | 'Personel'),
      );
      if (!hasRole) {
        return { path: '/dashboard', query: { denied: 'role' } };
      }
    }

    // Already authenticated → skip login/root, go to dashboard
    if (auth.isAuthenticated && (to.path === '/' || to.path === '/login')) {
      return '/dashboard';
    }

    return true;
  });

  return Router;
});
