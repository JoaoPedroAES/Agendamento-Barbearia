import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import api from '../../services/api';
import styles from './Cadastro.module.css';

/**
 * Componente de Cadastro de Clientes.
 * Responsável por coletar dados pessoais e de endereço, validar as entradas
 * e enviar as informações para a API de criação de conta.
 */
function Cadastro() {

    // --- ESTADOS DE DADOS PESSOAIS ---
    const [fullName, setFullName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');

    // --- ESTADOS DE ENDEREÇO ---
    const [cep, setCep] = useState('');
    const [street, setStreet] = useState('');
    const [number, setNumber] = useState('');
    const [complement, setComplement] = useState('');
    const [neighborhood, setNeighborhood] = useState('');
    const [city, setCity] = useState('');
    const [state, setState] = useState(''); // UF

    // --- ESTADOS DE CONTROLE ---
    const [termsAccepted, setTermsAccepted] = useState(false);
    const [error, setError] = useState('');
    const [cepLoading, setCepLoading] = useState(false);
    
    const navigate = useNavigate();

    // =========================================================================
    // MÁSCARAS E FORMATAÇÃO DE DADOS
    // =========================================================================

    const formatTextOnly = (value) => {
        return value.replace(/[^A-Za-zÀ-ÖØ-öø-ÿ\s]/g, '');
    };

    const formatAlphaNumeric = (value) => {
        return value.replace(/[^A-Za-z0-9\s]/g, '');
    };

    const formatNumberOnly = (value) => {
        return value.replace(/\D/g, '').slice(0, 4);
    };

    const handleUfChange = (e) => {
        let value = e.target.value;
        value = value.replace(/[^a-zA-Z]/g, ''); 
        value = value.toUpperCase(); 
        value = value.slice(0, 2); 
        setState(value);
    };

    const handlePhoneChange = (e) => {
        let value = e.target.value;
        value = value.replace(/\D/g, ""); 
        value = value.slice(0, 11); 
        value = value.replace(/^(\d{2})(\d)/g, "($1) $2"); 
        value = value.replace(/(\d{5})(\d)/, "$1-$2"); 
        setPhoneNumber(value);
    };

    const handleCepChange = (e) => {
        let value = e.target.value;
        value = value.replace(/\D/g, ""); 
        value = value.slice(0, 8);
        value = value.replace(/^(\d{5})(\d)/, "$1-$2"); 
        setCep(value);
    };

    // =========================================================================
    // VALIDAÇÕES LÓGICAS
    // =========================================================================

    const isValidEmail = (email) => {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    };

    // =========================================================================
    // INTEGRAÇÕES COM APIs (ViaCEP e Backend)
    // =========================================================================

    /**
     * Consulta automática de CEP na API ViaCEP.
     */
    const handleCepBlur = async (e) => {
        const currentCep = e.target.value.replace(/\D/g, ''); 
        
        if (currentCep.length !== 8) {
            return;
        }
        
        setCepLoading(true);
        setError('');
        
        try {
            const response = await axios.get(`https://viacep.com.br/ws/${currentCep}/json/`);
            
            if (response.data.erro) {
                // Caso o CEP seja válido (formato), mas não exista na base dos Correios
                setError('CEP não encontrado na base de dados. Digite o endereço manualmente.');
                setStreet(''); setNeighborhood(''); setCity(''); setState('');
            } else {
                setStreet(formatTextOnly(response.data.logradouro));
                setNeighborhood(formatTextOnly(response.data.bairro));
                setCity(formatTextOnly(response.data.localidade));
                setState(response.data.uf); 
            }
        } catch (err) {
            console.error("Erro ViaCEP:", err);
            
            // --- TRATAMENTO DE ERRO DE CONEXÃO/VIACEP FORA DO AR ---
            if (!err.response) {
                // Se não há resposta, é erro de rede ou serviço caído
                setError('Serviço de busca indisponível no momento. Por favor, preencha o endereço manualmente.');
            } else {
                // Outros erros (400, 500)
                setError('Erro ao buscar o CEP. Tente novamente.');
            }
            
        } finally {
            setCepLoading(false);
        }
    };

    /**
     * Envio do formulário para o Backend.
     */
    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (fullName.trim().split(' ').length < 2) {
            setError('Por favor, preencha seu nome completo (Nome e Sobrenome).');
            return;
        }

        if (!isValidEmail(email)) {
            setError('Por favor, insira um endereço de e-mail válido.');
            return;
        }

        const cleanPhone = phoneNumber.replace(/\D/g, ''); 
        if (cleanPhone.length < 10 || cleanPhone.length > 11) {
            setError('Por favor, insira um telefone válido com DDD.');
            return;
        }

        if (password !== confirmPassword) {
            setError('As senhas não conferem.');
            return;
        }
        if (password.length < 6) {
            setError('A senha deve ter pelo menos 6 caracteres.');
            return;
        }

        if (state.length !== 2) {
             setError('O estado (UF) deve ter 2 letras.');
             return;
        }

        try {
            await api.post('/api/auth/register-customer', {
                fullName,
                email,
                password,
                phoneNumber: cleanPhone,
                cep: cep.replace(/\D/g, ''),
                street,
                number,
                complement,
                neighborhood,
                city,
                state
            });

            alert('Cadastro realizado com sucesso! Você será redirecionado para a página de login.');
            navigate('/login');

        } catch (err) {
            // Tratamento de erro do Backend (Criação de Conta)
            if (err.response && err.response.data) {
                const errorMessage = typeof err.response.data === 'string'
                    ? err.response.data
                    : 'Erro ao realizar o cadastro. Verifique seus dados.';
                setError(errorMessage);
            } else {
                // Caso o nosso servidor Backend esteja fora do ar
                setError('Não foi possível conectar ao servidor da Barbearia. Tente mais tarde.');
            }
            console.error(err);
        }
    };

    return (
        <div className={styles.tela}>
            <div className={styles.container}>
                <form onSubmit={handleSubmit}>
                    <h1>Criar Conta</h1>

                    {/* GRUPO: DADOS PESSOAIS */}
                    <div className={styles.inputGroup}>
                        <input 
                            type="text" 
                            placeholder="Nome e Sobrenome" 
                            value={fullName} 
                            onChange={e => setFullName(formatTextOnly(e.target.value))} 
                            required 
                        />
                    </div>
                    <div className={styles.inputGroup}>
                        <input 
                            type="email" 
                            placeholder="E-mail" 
                            value={email} 
                            onChange={e => setEmail(e.target.value)} 
                            required 
                        />
                    </div>
                    <div className={styles.inputGroup}>
                        <input 
                            type="password" 
                            placeholder="Senha" 
                            value={password} 
                            onChange={e => setPassword(e.target.value)} 
                            required 
                        />
                    </div>
                    <div className={styles.inputGroup}>
                        <input 
                            type="password" 
                            placeholder="Confirmar Senha" 
                            value={confirmPassword} 
                            onChange={e => setConfirmPassword(e.target.value)} 
                            required 
                        />
                    </div>
                    <div className={styles.inputGroup}>
                        <input 
                            type="tel" 
                            placeholder="Celular (com DDD)" 
                            value={phoneNumber} 
                            onChange={handlePhoneChange} 
                        />
                    </div>

                    <hr style={{ margin: '20px 0', borderColor: 'rgba(255,255,255,0.2)' }} />

                    {/* GRUPO: ENDEREÇO */}
                    <div className={styles.inputGroup}>
                        <input 
                            type="text" 
                            placeholder="CEP" 
                            value={cep} 
                            onChange={handleCepChange} 
                            onBlur={handleCepBlur} 
                            required 
                        />
                        {cepLoading && <p style={{ color: 'white', fontSize: '12px' }}>Buscando...</p>}
                    </div>

                    <div className={styles.inputGroup}>
                        <input 
                            type="text" 
                            placeholder="Rua / Logradouro" 
                            value={street} 
                            onChange={e => setStreet(formatTextOnly(e.target.value))} 
                            required 
                            disabled={cepLoading} 
                        />
                    </div>

                    <div className={styles.addressFields}>
                        {/* CAMPO NÚMERO (Nº) */}
                        <div className={`${styles.inputGroup} ${styles.numberField}`}>
                            <input 
                                type="text" 
                                placeholder="Nº" 
                                value={number} 
                                onChange={e => setNumber(formatNumberOnly(e.target.value))} 
                                required 
                                disabled={cepLoading} 
                                inputMode="numeric"
                            />
                        </div>
                        
                        {/* CAMPO COMPLEMENTO */}
                        <div className={styles.inputGroup}>
                            <input 
                                type="text" 
                                placeholder="Complemento" 
                                value={complement} 
                                onChange={e => setComplement(formatAlphaNumeric(e.target.value))} 
                                disabled={cepLoading} 
                            />
                        </div>
                    </div>

                    <div className={styles.inputGroup}>
                        <input 
                            type="text" 
                            placeholder="Bairro" 
                            value={neighborhood} 
                            onChange={e => setNeighborhood(formatTextOnly(e.target.value))} 
                            required 
                            disabled={cepLoading} 
                        />
                    </div>

                    <div className={styles.addressFields}>
                        <div className={styles.inputGroup}>
                            <input 
                                type="text" 
                                placeholder="Cidade" 
                                value={city} 
                                onChange={e => setCity(formatTextOnly(e.target.value))} 
                                required 
                                disabled={cepLoading} 
                            />
                        </div>
                        
                        <div className={`${styles.inputGroup} ${styles.numberField}`}>
                            <input 
                                type="text" 
                                placeholder="UF" 
                                value={state} 
                                onChange={handleUfChange} 
                                required 
                                disabled={cepLoading}
                                maxLength={2} 
                            />
                        </div>
                    </div>

                    {error && <p className={styles.error}>{error}</p>}
                    
                    <div className={styles.termsGroup}>
                        <input
                            type="checkbox"
                            id="terms"
                            checked={termsAccepted}
                            onChange={(e) => setTermsAccepted(e.target.checked)}
                        />
                        <label htmlFor="terms">
                            Eu li e aceito os <Link to="/termos-de-uso" className={styles.termsLink} target="_blank" rel="noopener noreferrer">Termos de Uso</Link>
                        </label>
                    </div>

                    <button type="submit" className={styles.button} disabled={!termsAccepted}>Cadastrar</button>

                    <div className={styles.loginRedirect}>
                        Já tem uma conta?
                        <Link to="/login" className={styles.loginLink}>
                            Faça Login
                        </Link>
                    </div>

                </form>
            </div>
        </div>
    );
}

export default Cadastro;