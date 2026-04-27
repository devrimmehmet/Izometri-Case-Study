import { register } from 'register-service-worker';

// The ready(), registered(), cached(), updatefound() and updated()
// events pass a ServiceWorkerRegistration instance in their arguments.
// ServiceWorkerRegistration: https://developer.mozilla.org/en-US/docs/Web/API/ServiceWorkerRegistration

register(process.env.SERVICE_WORKER_FILE, {
  ready(/* registration */) {
    // Service worker is active.
  },

  registered(/* registration */) {
    // Service worker has been registered.
  },

  cached(/* registration */) {
    // Content has been cached for offline use.
  },

  updatefound(/* registration */) {
    // New content is downloading.
  },

  // New content is available: silently activate the incoming SW and reload.
  // workbox skipWaiting + clientsClaim in quasar.config.ts ensure the new SW
  // takes over immediately; the reload below picks up fresh assets without
  // any user prompt.
  updated(registration) {
    if (registration.waiting) {
      registration.waiting.postMessage({ type: 'SKIP_WAITING' });
    }
    setTimeout(() => {
      window.location.reload();
    }, 300);
  },

  offline() {
    // No internet connection found. App is running in offline mode.
  },

  error(err) {
    console.error('[SW] Registration error:', err);
  },
});
