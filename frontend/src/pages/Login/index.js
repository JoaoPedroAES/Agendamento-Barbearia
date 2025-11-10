

import styles from './Login.module.css';
import { FaUser, FaLock } from "react-icons/fa";

import { useState, useEffect } from 'react'; 
import { useNavigate } from 'react-router-dom';

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
            
            setError('Falha no login. Verifique seu e-mail e senha.');
            console.error("Erro no login:", err);
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
                        <a href="/forgot-password">Esqueceu a senha?</a>
                    </div>
                    <button>Login</button>
                    {error && <p className={styles.error}>{error}</p>}
                    <div className={styles.registrar}>
                        <p>
                            <a href="/registrar">Registrar</a>
                        </p>
                    </div>
                </form>
            </div>
       </div> 
    )
}

export default Login;