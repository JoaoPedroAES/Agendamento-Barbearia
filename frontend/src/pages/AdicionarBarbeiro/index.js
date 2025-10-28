

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

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        try {
            await api.post('/api/barber', { 
                fullName,
                email,
                password,
                phoneNumber,
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
                        <input type="text" placeholder="Nome Completo" value={fullName} onChange={e => setFullName(e.target.value)} required />
                    </div>
                    <div className={styles.inputGroup}>
                        <input type="email" placeholder="E-mail de Login" value={email} onChange={e => setEmail(e.target.value)} required />
                    </div>
                    <div className={styles.inputGroup}>
                        <input type="password" placeholder="Senha Provisória" value={password} onChange={e => setPassword(e.target.value)} required />
                    </div>
                    <div className={styles.inputGroup}>
                        {}
                        <input type="tel" placeholder="Celular" value={phoneNumber} onChange={e => setPhoneNumber(e.target.value)} required />
                    </div>
                    <div className={styles.inputGroup}>
                        <input type="text" placeholder="Bio / Especialidade (Opcional)" value={bio} onChange={e => setBio(e.target.value)} />
                    </div>
                    
                    {error && <p className={styles.error}>{error}</p>}
                    
                    <button type="submit" className={styles.button}>Cadastrar Barbeiro</button>

                    {}
                    <div className={styles.loginLink}> 
                       <button type="button" onClick={() => navigate(-1)} className={styles.backLinkButton}>Voltar</button>
                    </div>

                </form>
            </div>
        </div>
    );
}

export default AdicionarBarbeiro;