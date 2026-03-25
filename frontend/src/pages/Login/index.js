import styles from './Login.module.css';
import { FaUser, FaLock } from "react-icons/fa";

import { useState, useEffect } from 'react'; 
// Adicionei o Link aqui na importação
import { useNavigate, Link } from 'react-router-dom'; 

import { useAuth } from '../../context/AuthContext'; 

function Login(){
    const [email, setEmail] = useState(""); 
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const navigate = useNavigate();
    
    const { login, isAuthenticated, user } = useAuth(); 

    useEffect(() => {
        if (isAuthenticated && user) {
            if (user.roles.includes('Admin') || user.roles.includes('Barbeiro')) {
                navigate('/gestao'); 
            } else {
                navigate('/dashboard'); 
            }
        }
    }, [isAuthenticated, user, navigate]); 

    const handleSubmit = async (event) => {
        event.preventDefault();
        setError(""); 

        try {
            const loggedInUser = await login(email, password); 
            
            if (loggedInUser) {
                 if (loggedInUser.roles.includes('Admin') || loggedInUser.roles.includes('Barbeiro')) {
                    navigate('/gestao'); 
                } else {
                    navigate('/dashboard'); 
                }
            }

        } catch (err) {
            console.error("Erro no login:", err);

            // --- LÓGICA DE TRATAMENTO DE ERRO MELHORADA ---
            
            if (!err.response) {
                // Caso 1: O servidor não respondeu (Offline ou erro de rede)
                setError('Não foi possível conectar ao servidor. Tente mais tarde.');
            } else if (err.response.status === 401 || err.response.status === 400) {
                // Caso 2: O servidor respondeu que a senha/email estão errados
                setError('Falha no login. Verifique seu e-mail e senha.');
            } else {
                // Caso 3: Outro erro qualquer do servidor (ex: 500)
                setError('Ocorreu um erro inesperado. Tente novamente.');
            }
        }
    };

    return(
        <div className={styles.tela}>
            <div className={styles.container}>
                <form onSubmit={handleSubmit} >
                    <h1>Acesso ao Sistema</h1>
                    <div className={styles.texto}>
                        <input type="email" placeholder="E-mail" onChange={(e) => setEmail(e.target.value)} required />
                        <FaUser className={styles.icon} />
                    </div>
                    <div className={styles.texto}>
                        <input type="password" placeholder='Senha' onChange={(e) => setPassword(e.target.value)} required/>
                        <FaLock className={styles.icon} />
                    </div>
                    
                    <div className={styles.recallforget}>
                        {/* Alterado de <a> para <Link> para navegação mais rápida */}
                        <Link to="/esqueceu-senha">Esqueceu a senha?</Link>
                    </div>
                    
                    <button>Login</button>
                    
                    {error && <p className={styles.error}>{error}</p>}
                    
                    <div className={styles.registrar}>
                        <p>
                            {/* Alterado de <a> para <Link> */}
                            <Link to="/registrar">Registrar</Link>
                        </p>
                    </div>
                </form>
            </div>
       </div> 
    )
}

export default Login;