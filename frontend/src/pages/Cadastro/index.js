

import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios'; 
import api from '../../services/api'; 
import styles from './Cadastro.module.css';

function Cadastro() {
    
    const [fullName, setFullName] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [phoneNumber, setPhoneNumber] = useState('');
    const [cep, setCep] = useState('');
    const [street, setStreet] = useState('');
    const [number, setNumber] = useState('');
    const [complement, setComplement] = useState('');
    const [neighborhood, setNeighborhood] = useState('');
    const [city, setCity] = useState('');
    const [state, setState] = useState('');

    
    const [error, setError] = useState('');
    const [cepLoading, setCepLoading] = useState(false);
    const navigate = useNavigate();

    
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
                setError('CEP não encontrado.');
                setStreet(''); setNeighborhood(''); setCity(''); setState('');
            } else {
                setStreet(response.data.logradouro);
                setNeighborhood(response.data.bairro);
                setCity(response.data.localidade);
                setState(response.data.uf);
            }
        } catch (err) {
            setError('Erro ao buscar o CEP. Tente novamente.');
            console.error(err);
        } finally {
            setCepLoading(false);
        }
    };

    
    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        if (password !== confirmPassword) {
            setError('As senhas não conferem.');
            return;
        }

        try {
            await api.post('/api/auth/register-customer', {
                fullName,
                email,
                password,
                phoneNumber,
                cep,
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
            if (err.response && err.response.data) {
                
                const errorMessage = typeof err.response.data === 'string' 
                    ? err.response.data 
                    : 'Erro ao realizar o cadastro. Verifique seus dados.';
                setError(errorMessage);
            } else {
                setError('Não foi possível conectar ao servidor. Tente mais tarde.');
            }
            console.error(err);
        }
    };

    
    return (
        <div className={styles.tela}> {}
            <div className={styles.container}>
                <form onSubmit={handleSubmit}>
                    <h1>Criar Conta</h1>

                    <div className={styles.inputGroup}>
                        <input type="text" placeholder="Nome Completo" value={fullName} onChange={e => setFullName(e.target.value)} required />
                    </div>
                    <div className={styles.inputGroup}>
                        <input type="email" placeholder="E-mail" value={email} onChange={e => setEmail(e.target.value)} required />
                    </div>
                    <div className={styles.inputGroup}>
                        <input type="password" placeholder="Senha" value={password} onChange={e => setPassword(e.target.value)} required />
                    </div>
                    <div className={styles.inputGroup}>
                        <input type="password" placeholder="Confirmar Senha" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} required />
                    </div>
                    <div className={styles.inputGroup}>
                        <input type="tel" placeholder="Celular (Opcional)" value={phoneNumber} onChange={e => setPhoneNumber(e.target.value)} />
                    </div>

                    <hr style={{margin: '20px 0', borderColor: 'rgba(255,255,255,0.2)'}} />

                    <div className={styles.inputGroup}>
                        <input type="text" placeholder="CEP" value={cep} onChange={e => setCep(e.target.value)} onBlur={handleCepBlur} required />
                        {cepLoading && <p style={{color: 'white', fontSize: '12px'}}>Buscando...</p>}
                    </div>

                    <div className={styles.inputGroup}>
                        <input type="text" placeholder="Rua / Logradouro" value={street} onChange={e => setStreet(e.target.value)} required disabled={cepLoading} />
                    </div>

                    <div className={styles.addressFields}>
                        <div className={`${styles.inputGroup} ${styles.numberField}`}>
                            <input type="text" placeholder="Nº" value={number} onChange={e => setNumber(e.target.value)} required disabled={cepLoading} />
                        </div>
                        <div className={styles.inputGroup}>
                            <input type="text" placeholder="Complemento" value={complement} onChange={e => setComplement(e.target.value)} disabled={cepLoading} />
                        </div>
                    </div>

                    <div className={styles.inputGroup}>
                        <input type="text" placeholder="Bairro" value={neighborhood} onChange={e => setNeighborhood(e.target.value)} required disabled={cepLoading} />
                    </div>
                    
                    <div className={styles.addressFields}>
                        <div className={styles.inputGroup}>
                            <input type="text" placeholder="Cidade" value={city} onChange={e => setCity(e.target.value)} required disabled={cepLoading} />
                        </div>
                        <div className={`${styles.inputGroup} ${styles.numberField}`}>
                            <input type="text" placeholder="UF" value={state} onChange={e => setState(e.target.value)} required disabled={cepLoading} />
                        </div>
                    </div>
                    
                    {error && <p className={styles.error}>{error}</p>}
                    
                    <button type="submit" className={styles.button}>Cadastrar</button>

                    <div className={styles.loginLink}>
                        <p>Já tem uma conta? <a href="/login">Faça Login</a></p>
                    </div>

                </form>
            </div>
        </div>
    );
}

export default Cadastro;