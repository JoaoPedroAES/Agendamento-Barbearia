import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api'; 
import styles from './AdicionarBarbeiro.module.css'; 

function AdicionarBarbeiro() {
    const [fullName, setFullName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [bio, setBio] = useState('');
    
    const [error, setError] = useState('');
    const navigate = useNavigate();

    // --- FUNÇÕES AUXILIARES ---

    const handlePhoneChange = (e) => {
        let value = e.target.value;
        value = value.replace(/\D/g, ""); 
        value = value.slice(0, 11); 
        value = value.replace(/^(\d{2})(\d)/g, "($1) $2"); 
        value = value.replace(/(\d{5})(\d)/, "$1-$2"); 
        setPhoneNumber(value);
    };

    const isValidEmail = (email) => {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    };

    // --- ENVIO DO FORMULÁRIO ---

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        // 1. Validação: Apenas letras e acentos no nome
        // Regex explicado:
        // ^ e $ = Início e fim do texto
        // A-Za-z = Letras normais
        // À-ÖØ-öø-ÿ = Caracteres acentuados (ã, é, ç, etc.)
        // \s = Espaços
        const nameRegex = /^[A-Za-zÀ-ÖØ-öø-ÿ\s]+$/;

        if (!nameRegex.test(fullName)) {
            setError('O nome não pode conter números ou caracteres especiais.');
            return;
        }

        // 2. Validação: Nome Completo (mínimo 2 palavras)
        if (fullName.trim().split(' ').length < 2) {
            setError('Por favor, preencha o nome completo (Nome e Sobrenome).');
            return;
        }

        // 3. Validação: Email
        if (!isValidEmail(email)) {
            setError('Por favor, insira um endereço de e-mail válido.');
            return;
        }

        // 4. Validação: Telefone
        const cleanPhone = phoneNumber.replace(/\D/g, ''); 
        if (cleanPhone.length < 10 || cleanPhone.length > 11) {
            setError('Por favor, insira um telefone válido com DDD.');
            return;
        }

        // 5. Validação: Senha
        if (password.length < 6) {
            setError('A senha deve ter pelo menos 6 caracteres.');
            return;
        }

        try {
            await api.post('/api/barber', { 
                fullName,
                email,
                password,
                phoneNumber: cleanPhone, 
                bio
            });
            
            alert('Barbeiro cadastrado com sucesso!');
            navigate('/gestao'); 

        } catch (err) {
            if (err.response && err.response.data) {
                const errorMessage = typeof err.response.data === 'string' 
                    ? err.response.data 
                    : (err.response.data.errors ? err.response.data.errors[0].description : 'Erro ao cadastrar. Verifique os dados.');
                setError(errorMessage);
            } else {
                setError('Não foi possível conectar ao servidor.');
            }
            console.error("Erro ao cadastrar barbeiro:", err);
        }
    };

    return (
        <div className={styles.tela}> 
            <div className={styles.container}> 
                <form onSubmit={handleSubmit}>
                    <h1>Adicionar Novo Barbeiro</h1>

                    <div className={styles.inputGroup}>
                        <input 
                            type="text" 
                            placeholder="Nome e Sobrenome" 
                            value={fullName} 
                            onChange={e => setFullName(e.target.value)} 
                            required 
                        />
                    </div>
                    <div className={styles.inputGroup}>
                        <input 
                            type="email" 
                            placeholder="E-mail de Login" 
                            value={email} 
                            onChange={e => setEmail(e.target.value)} 
                            required 
                        />
                    </div>
                    <div className={styles.inputGroup}>
                        <input 
                            type="password" 
                            placeholder="Senha Provisória" 
                            value={password} 
                            onChange={e => setPassword(e.target.value)} 
                            required 
                        />
                    </div>
                    
                    <div className={styles.inputGroup}>
                        <input 
                            type="tel" 
                            placeholder="Celular (com DDD)" 
                            value={phoneNumber} 
                            onChange={handlePhoneChange} 
                            required 
                        />
                    </div>

                    <div className={styles.inputGroup}>
                        <input 
                            type="text" 
                            placeholder="Bio / Especialidade (Opcional)" 
                            value={bio} 
                            onChange={e => setBio(e.target.value)} 
                        />
                    </div>
                    
                    {error && <p className={styles.error}>{error}</p>}
                    
                    <button type="submit" className={styles.button}>Cadastrar Barbeiro</button>

                    <div className={styles.loginLink}> 
                       <button type="button" onClick={() => navigate(-1)} className={styles.backLinkButton}>Voltar</button>
                    </div>

                </form>
            </div>
        </div>
    );
}

export default AdicionarBarbeiro;