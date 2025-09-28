import axios from 'axios';

// Cria uma instância do axios com a URL base do seu backend .NET
const api = axios.create({
  baseURL: 'https://localhost:7275' // CONFIRA A PORTA NO SEU PROJETO BACKEND
});

// Isso é um "Interceptor". Ele "intercepta" todas as requisições ANTES de serem enviadas.
// A função dele é pegar o token que guardamos no navegador e anexá-lo ao cabeçalho.
api.interceptors.request.use(async config => {
  const token = localStorage.getItem('authToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default api;