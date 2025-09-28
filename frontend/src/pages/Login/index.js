import styles from './Login.module.css';
import {FaUser, FaLock} from "react-icons/fa";
import { useState } from 'react';

// 1. IMPORTE O 'api' E O 'useNavigate'
import api from '../../services/api'; // (Ajuste o caminho se necessário)
import { useNavigate } from 'react-router-dom';

function Login(){

    // Renomeei para 'email' para ficar mais claro, mas mantive a lógica
    const [email, setEmail] = useState(""); 
    const [password, setPassword] = useState(""); // Corrigido de setPassaword

    // 2. ADICIONE UM STATE PARA LIDAR COM ERROS
    const [error, setError] = useState("");
    
    const navigate = useNavigate(); // Hook para navegar entre páginas

    // 3. ATUALIZE A LÓGICA DE SUBMISSÃO
    const handleSubmit = async (event) => {
        event.preventDefault();
        setError(""); // Limpa erros anteriores

        try {
            // Chama o endpoint /login do seu backend
            const response = await api.post('/login', {
                email: email, // Usa o state 'email'
                password: password,
            });

            // Se o login der certo, pega o token da resposta
            const { accessToken } = response.data;

            // Guarda o token no navegador para ser usado em outras páginas
            localStorage.setItem('authToken', accessToken);

            // ATUALIZA o cabeçalho padrão do axios para incluir o token em requisições futuras
            api.defaults.headers.common['Authorization'] = `Bearer ${accessToken}`;

            // Redireciona o usuário para a página de dashboard (ou outra página)
            navigate('/dashboard'); // Mude '/dashboard' para sua rota de destino

        } catch (err) {
            // Se der erro (ex: 401 Unauthorized), mostra uma mensagem para o usuário
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
                            {/* O input de email agora usa setEmail */}
                            <input type="email" placeholder="E-mail" onChange={(e) => setEmail(e.target.value)} required />
                            <FaUser className={styles.icon} />
                        </div>
                        <div className={styles.texto}>
                            {/* O input de senha agora usa setPassword (corrigido) */}
                            <input type="password" placeholder='Senha' onChange={(e) => setPassword(e.target.value)} required/>
                            <FaLock className={styles.icon} />
                        </div>

                        <div className={styles.recallforget}>
                            <label>
                                <input type="checkbox" />
                                Lembrar Senha
                            </label>
                            <a href="/forgot-password">Esqueceu a senha?</a>
                        </div>

                        <button>Login</button>

                        {/* 4. ADICIONE UM LOCAL PARA MOSTRAR A MENSAGEM DE ERRO */}
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