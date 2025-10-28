

 import Inicio from "pages/inicio";
 import Login from './pages/Login';
 import Cadastro from './pages/Cadastro';
 import Dashboard from './pages/Dashboard';
 import Agendamento from './pages/Agendamento';
 import PrivateRoute from './PrivateRoute';
 import { BrowserRouter, Route, Routes } from "react-router-dom";
 import EditarPerfil from './pages/EditarPerfil';
 import ManagementDashboard from './pages/ManagementDashboard';
 import AdicionarBarbeiro from './pages/AdicionarBarbeiro';
 import EditarBarbeiro from './pages/EditarBarbeiro';
 import GerenciarServicos from './pages/GerenciarServicos';
 import ListarBarbeiros from './pages/ListarBarbeiros';

 function AppRoutes(){
     return (
         <BrowserRouter>
         <Routes>
             {}
             <Route path="/" element={<Inicio />}></Route>
             <Route path="/login" element={<Login/>}></Route>
             <Route path="/registrar" element={<Cadastro/>}></Route>

             {}
             <Route 
               path="/dashboard" 
               element={<PrivateRoute><Dashboard /></PrivateRoute>}
             />
             {}
             <Route 
               path="/agendamento" 
               element={<PrivateRoute><Agendamento /></PrivateRoute>}
             />
             <Route 
              path="/perfil" 
              element={<PrivateRoute><EditarPerfil /></PrivateRoute>}
            />
            <Route 
               path="/gestao"
               element={<PrivateRoute><ManagementDashboard /></PrivateRoute>} 
             />
             <Route 
               path="/adicionar-barbeiro" 
               element={<PrivateRoute><AdicionarBarbeiro /></PrivateRoute>}
             />
             <Route
                path="/editar-barbeiro"
                element={<PrivateRoute><EditarBarbeiro /></PrivateRoute>}
             />
             <Route
               path="/servicos"
               element={<PrivateRoute><GerenciarServicos /></PrivateRoute>}
             />
             <Route
               path="/barbeiros"
               element={<PrivateRoute><ListarBarbeiros /></PrivateRoute>}
             />
         </Routes>
         </BrowserRouter>
     )
 }

 export default AppRoutes;