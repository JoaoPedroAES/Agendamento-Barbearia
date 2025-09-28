// src/routes.js

 import Inicio from "pages/inicio";
 import Login from './pages/Login';
 import Cadastro from './pages/Cadastro';
 import Dashboard from './pages/Dashboard';
 import Agendamento from './pages/Agendamento';
 import PrivateRoute from './PrivateRoute';
 import { BrowserRouter, Route, Routes } from "react-router-dom";
 import EditarPerfil from './pages/EditarPerfil';

 function AppRoutes(){
     return (
         <BrowserRouter>
         <Routes>
             {/* --- Rotas Públicas --- */}
             <Route path="/" element={<Inicio />}></Route>
             <Route path="/login" element={<Login/>}></Route>
             <Route path="/registrar" element={<Cadastro/>}></Route>

             {/* --- Rotas Privadas/Protegidas --- */}
             <Route 
               path="/dashboard" 
               element={<PrivateRoute><Dashboard /></PrivateRoute>}
             />
             {/* 2. ADICIONE A NOVA ROTA PROTEGIDA ABAIXO */}
             <Route 
               path="/agendamento" 
               element={<PrivateRoute><Agendamento /></PrivateRoute>}
             />
             <Route 
              path="/perfil" 
              element={<PrivateRoute><EditarPerfil /></PrivateRoute>}
            />
         </Routes>
         </BrowserRouter>
     )
 }

 export default AppRoutes;