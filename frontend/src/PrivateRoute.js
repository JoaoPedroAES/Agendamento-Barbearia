
import { Navigate } from 'react-router-dom';

// Este componente funciona como um "porteiro" para as nossas rotas.
const PrivateRoute = ({ children }) => {
  // Ele verifica no localStorage se o nosso 'authToken' (que salvamos no login) existe.
  const isAuthenticated = !!localStorage.getItem('authToken'); // '!!' transforma a string (ou null) em um booleano (true/false)

  // Se estiver autenticado (true), ele permite o acesso à página solicitada (children).
  // Se não (false), ele redireciona o usuário para a página de login.
  return isAuthenticated ? children : <Navigate to="/login" />;
};

export default PrivateRoute;