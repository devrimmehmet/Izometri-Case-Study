import axios, { type AxiosInstance } from 'axios';

const api = axios.create({ baseURL: '/api' });
const notifyApi = axios.create({ baseURL: '/notify-api' });

function clearSession() {
  localStorage.removeItem('jwt');
  localStorage.removeItem('tenantCode');
  localStorage.removeItem('authProvider');
  delete api.defaults.headers.common['Authorization'];
  delete notifyApi.defaults.headers.common['Authorization'];
}

function configureClient(client: AxiosInstance) {
  client.interceptors.request.use((config) => {
    const token = localStorage.getItem('jwt');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });

  client.interceptors.response.use(
    (response) => response,
    (error) => {
      if (error?.response?.status === 401) {
        clearSession();
        if (window.location.pathname !== '/login' && !window.location.hash.includes('/login')) {
          window.location.href = '/#/login';
        }
      }

      throw error;
    },
  );
}

configureClient(api);
configureClient(notifyApi);

export { api, notifyApi };
