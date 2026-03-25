import axios from 'axios';

const apiUrl = process.env.REACT_APP_API_URL || 'https://localhost:7275';

const api = axios.create({
  baseURL: apiUrl, 
});

api.interceptors.request.use(async config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;
