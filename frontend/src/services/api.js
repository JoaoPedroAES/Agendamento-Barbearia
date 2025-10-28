import axios from 'axios';


const api = axios.create({
  baseURL: 'https://localhost:7275' 
});



api.interceptors.request.use(async config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;