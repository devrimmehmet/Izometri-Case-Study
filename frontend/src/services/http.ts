import axios from 'axios';

const api = axios.create({ baseURL: '/api' });
const notifyApi = axios.create({ baseURL: '/notify-api' });

notifyApi.interceptors.request.use((config) => {
  const token = localStorage.getItem('jwt');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export { api, notifyApi };
