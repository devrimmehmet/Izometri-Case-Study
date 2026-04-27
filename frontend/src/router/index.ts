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

    // Restore session on first load
    if (!auth.isAuthenticated) {
      auth.restoreSession();
    }

    // Check gate
    if (!auth.gateUnlocked) {
      auth.checkGate();
    }

    // Gate required but not unlocked
    if (to.meta.requiresGate && !auth.gateUnlocked) {
      return '/';
    }

    // Auth required but not authenticated
    if (to.meta.requiresAuth && !auth.isAuthenticated) {
      return '/login';
    }

    // Role check
    const requiredRoles = to.meta.roles;
    if (requiredRoles && requiredRoles.length > 0) {
      const hasRole = requiredRoles.some((r) =>
        auth.roles.includes(r as 'Admin' | 'HR' | 'Personnel'),
      );
      if (!hasRole) {
        return '/dashboard';
      }
    }

    // Already authenticated, redirect gate/login to dashboard
    if (
      auth.isAuthenticated &&
      auth.gateUnlocked &&
      (to.path === '/' || to.path === '/login')
    ) {
      return '/dashboard';
    }

    // Allow navigation
    return true;
  });

  return Router;
});
