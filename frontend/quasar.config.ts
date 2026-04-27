// Configuration for your app
// https://v2.quasar.dev/quasar-cli-vite/quasar-config-file

import { defineConfig } from '#q-app/wrappers';
import { fileURLToPath } from 'node:url';

export default defineConfig((ctx) => {
  return {
    // Boot files (/src/boot) – part of "main.js"
    boot: ['i18n', 'axios'],

    css: ['app.scss'],

    extras: [
      'roboto-font',
      'material-icons',
      'material-symbols-outlined',
    ],

    build: {
      target: {
        browser: 'baseline-widely-available',
        node: 'node22',
      },

      typescript: {
        strict: true,
        vueShim: true,
      },

      vueRouterMode: 'hash',

      vitePlugins: [
        [
          '@intlify/unplugin-vue-i18n/vite',
          {
            ssr: ctx.modeName === 'ssr',
            include: [fileURLToPath(new URL('./src/i18n', import.meta.url))],
          },
        ],
        [
          'vite-plugin-checker',
          {
            vueTsc: true,
            eslint: {
              lintCommand: 'eslint -c ./eslint.config.js "./src*/**/*.{ts,js,mjs,cjs,vue}"',
              useFlatConfig: true,
            },
          },
          { server: false },
        ],
      ],
    },

    devServer: {
      open: true,
      proxy: {
        '/api': {
          target: 'http://localhost:5001',
          changeOrigin: true,
        },
        '/notify-api': {
          target: 'http://localhost:5002',
          changeOrigin: true,
          rewrite: (path: string) => path.replace(/^\/notify-api/, '/api'),
        },
        '/health/expense': {
          target: 'http://localhost:5001',
          changeOrigin: true,
          rewrite: (path: string) => path.replace(/^\/health\/expense/, '/health'),
        },
        '/health/notification': {
          target: 'http://localhost:5002',
          changeOrigin: true,
          rewrite: (path: string) => path.replace(/^\/health\/notification/, '/health'),
        },
      },
    },

    framework: {
      config: {
        dark: true,
        notify: {
          position: 'top-right',
          timeout: 3000,
        },
        loading: {},
      },
      plugins: ['Notify', 'Loading', 'Dialog', 'LocalStorage', 'SessionStorage'],
    },

    animations: ['fadeIn', 'fadeOut', 'slideInLeft', 'slideInRight', 'slideInUp'],

    // ─── PWA ────────────────────────────────────────────────────────────────
    pwa: {
      workboxMode: 'GenerateSW',

      workboxOptions: {
        // Silently activate and take control without waiting for old SW to die.
        skipWaiting: true,
        clientsClaim: true,
        cleanupOutdatedCaches: true,
      },

      // Web-app manifest — all Quasar branding replaced with DMP
      manifest: {
        name: 'Case Study',
        short_name: 'Case Study',
        description: 'İzometri Case Study',
        display: 'standalone',
        orientation: 'portrait',
        background_color: '#1a1a2e',
        theme_color: '#1a1a2e',
        icons: [
          {
            src: 'icons/icon-128x128.png',
            sizes: '128x128',
            type: 'image/png',
          },
          {
            src: 'icons/icon-192x192.png',
            sizes: '192x192',
            type: 'image/png',
          },
          {
            src: 'icons/icon-256x256.png',
            sizes: '256x256',
            type: 'image/png',
          },
          {
            src: 'icons/icon-384x384.png',
            sizes: '384x384',
            type: 'image/png',
          },
          {
            src: 'icons/icon-512x512.png',
            sizes: '512x512',
            type: 'image/png',
          },
          {
            src: 'icons/maskable_icon_x512.png',
            sizes: '512x512',
            type: 'image/png',
            purpose: 'maskable',
          },
        ],
      },

      metaVariables: {
        appleMobileWebAppCapable: 'yes',
        appleMobileWebAppStatusBarStyle: 'black-translucent',
        appleTouchIcon120x120: 'icons/apple-icon-120x120.png',
        msapplicationTileImage: 'icons/ms-icon-310x310.png',
        msapplicationTileColor: '#1a1a2e',
      },
    },

    ssr: {
      prodPort: 3000,
      middlewares: ['render'],
      pwa: false,
    },

    cordova: {},

    capacitor: {
      hideSplashscreen: true,
    },

    electron: {
      preloadScripts: ['electron-preload'],
      inspectPort: 5858,
      bundler: 'packager',
      packager: {},
      builder: {
        appId: 'izometri-project',
      },
    },

    bex: {
      extraScripts: [],
    },
  };
});
