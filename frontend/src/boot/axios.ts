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

export default defineBoot(({ app }) => {
  app.config.globalProperties.$axios = axios;
  app.config.globalProperties.$api = api;
  app.config.globalProperties.$notifyApi = notifyApi;
});

export { api, notifyApi };
