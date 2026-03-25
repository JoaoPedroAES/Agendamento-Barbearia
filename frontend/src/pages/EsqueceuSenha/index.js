import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../../services/api'; 
import styles from './Esqueceusenha.module.css'; 
import { FaUser, FaPhone, FaLock, FaArrowLeft, FaCheckCircle } from "react-icons/fa";

function EsqueceuSenha() {
    const navigate = useNavigate();
    
    // Controle das etapas: 1 = Validar Dados, 2 = Nova Senha
    const [step, setStep] = useState(1);
    
    const [email, setEmail] = useState('');
    const [phone, setPhone] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [confirmNewPassword, setConfirmNewPassword] = useState('');
    
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);

    // Máscara para o telefone (Visual)
    const handlePhoneChange = (e) => {
        let value = e.target.value;
        value = value.replace(/\D/g, ""); 
        value = value.slice(0, 11); 
        value = value.replace(/^(\d{2})(\d)/g, "($1) $2"); 
        value = value.replace(/(\d{5})(\d)/, "$1-$2"); 
        setPhone(value);
    };

    // --- ETAPA 1: Validar se é o Cliente ---
    const handleVerifyUser = async (e) => {
        e.preventDefault();
        setError('');
        setLoading(true);

        const cleanPhone = phone.replace(/\D/g, ''); 

        try {
            await api.post('/api/auth/validate-reset-customer', { 
                email, 
                phoneNumber: cleanPhone 
            });
            
            setStep(2); // Tudo certo, avança para trocar a senha

        } catch (err) {
            console.error("Erro validação:", err);

            // --- LÓGICA DE ERRO CORRIGIDA ---
            if (!err.response) {
                // Servidor desligado ou erro de rede
                setError('Não foi possível conectar ao servidor. Tente mais tarde.');
            } else if (err.response.status === 404 || err.response.status === 400) {
                // Servidor respondeu que não encontrou (404) ou dados inválidos (400)
                setError('Dados não encontrados. Verifique se o e-mail e celular estão corretos.');
            } else {
                // Outro erro qualquer (500, etc)
                setError('Ocorreu um erro inesperado. Tente novamente.');
            }
        } finally {
            setLoading(false);
        }
    };

    // --- ETAPA 2: Salvar Nova Senha ---
    const handleSavePassword = async (e) => {
        e.preventDefault();
        setError('');

        if (newPassword !== confirmNewPassword) {
            setError('As senhas não conferem.');
            return;
        }

        if (newPassword.length < 6) {
            setError('A senha deve ter no mínimo 6 caracteres.');
            return;
        }

        setLoading(true);

        try {
            await api.post('/api/auth/reset-password-customer', {
                email, 
                newPassword
            });

            alert('Sua senha foi alterada com sucesso! Faça login.');
            navigate('/login');

        } catch (err) {
            console.error("Erro ao salvar senha:", err);

            // --- LÓGICA DE ERRO CORRIGIDA ---
            if (!err.response) {
                setError('Não foi possível conectar ao servidor. Tente mais tarde.');
            } else {
                // Tenta pegar a mensagem específica do backend, se houver
                const msg = err.response.data && typeof err.response.data === 'string' 
                    ? err.response.data 
                    : 'Erro ao salvar a nova senha. Tente novamente.';
                setError(msg);
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className={styles.tela}>
            <div className={styles.container}>
                
                {/* --- CONTEÚDO DA ETAPA 1 --- */}
                {step === 1 && (
                    <form onSubmit={handleVerifyUser}>
                        <h1>Recuperar Conta</h1>
                        <p className={styles.description}>
                            Informe seus dados de cadastro para validar sua identidade.
                        </p>

                        <div className={styles.texto}>
                            <input 
                                type="email" 
                                placeholder="E-mail" 
                                value={email}
                                onChange={(e) => setEmail(e.target.value)} 
                                required 
                            />
                            <FaUser className={styles.icon} />
                        </div>
                        
                        <div className={styles.texto}>
                            <input 
                                type="tel" 
                                placeholder="Celular (com DDD)" 
                                value={phone}
                                onChange={handlePhoneChange} 
                                required 
                            />
                            <FaPhone className={styles.icon} />
                        </div>

                        <button disabled={loading} className={styles.button}>
                            {loading ? 'Verificando...' : 'Continuar'}
                        </button>

                        {error && <p className={styles.error}>{error}</p>}

                        <div className={styles.backLink}>
                            <Link to="/login">
                                <FaArrowLeft /> Voltar para Login
                            </Link>
                        </div>
                    </form>
                )}

                {/* --- CONTEÚDO DA ETAPA 2 --- */}
                {step === 2 && (
                    <form onSubmit={handleSavePassword}>
                        <h1>Nova Senha</h1>
                        <p className={styles.description}>
                            <FaCheckCircle style={{color: '#4caf50', marginRight: '5px'}}/>
                            Dados confirmados! Crie sua nova senha.
                        </p>

                        <div className={styles.texto}>
                            <input 
                                type="password" 
                                placeholder="Nova Senha" 
                                value={newPassword}
                                onChange={(e) => setNewPassword(e.target.value)} 
                                required 
                            />
                            <FaLock className={styles.icon} />
                        </div>
                        
                        <div className={styles.texto}>
                            <input 
                                type="password" 
                                placeholder="Confirmar Senha" 
                                value={confirmNewPassword}
                                onChange={(e) => setConfirmNewPassword(e.target.value)} 
                                required 
                            />
                            <FaLock className={styles.icon} />
                        </div>

                        <button disabled={loading} className={styles.button}>
                            {loading ? 'Salvando...' : 'Alterar Senha'}
                        </button>

                        {error && <p className={styles.error}>{error}</p>}
                        
                        <div className={styles.backLink}>
                             <button type="button" onClick={() => setStep(1)} className={styles.cancelButton}>
                                Cancelar
                             </button>
                        </div>
                    </form>
                )}

            </div>
        </div>
    );
}

export default EsqueceuSenha;