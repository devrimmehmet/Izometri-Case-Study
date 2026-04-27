import { defineBoot } from '#q-app/wrappers';
import axios, { type AxiosInstance } from 'axios';

declare module 'vue' {
  interface ComponentCustomProperties {
    $axios: AxiosInstance;
    $api: AxiosInstance;
    $notifyApi: AxiosInstance;
  }
}

const api = axios.create({ baseURL: '/api' });
const notifyApi = axios.create({ baseURL: '/notify-api' });

// Attach stored JWT so notifyApi uses the same token as api
notifyApi.interceptors.request.use((config) => {
  const token = localStorage.getItem('jwt');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default defineBoot(({ app }) => {
  app.config.globalProperties.$axios = axios;
  app.config.globalProperties.$api = api;
  app.config.globalProperties.$notifyApi = notifyApi;
});

export { api, notifyApi };
