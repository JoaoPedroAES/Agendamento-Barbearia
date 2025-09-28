// src/pages/EditarPerfil/index.js

import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import api from '../../services/api';
import styles from './EditarPerfil.module.css';

function EditarPerfil() {
    // 1. STATE ÚNICO PARA O FORMULÁRIO
    const [formData, setFormData] = useState({
        fullName: '',
        email: '',
        phoneNumber: '',
        cep: '',
        street: '',
        number: '',
        complement: '',
        neighborhood: '',
        city: '',
        state: ''
    });
    
    // States de controle da UI
    const [loading, setLoading] = useState(true);
    const [cepLoading, setCepLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    // 2. BUSCA DADOS ATUAIS PARA PREENCHER O FORMULÁRIO
    useEffect(() => {
        const fetchUserData = async () => {
            try {
                const response = await api.get('/api/users/me');
                const userData = response.data;
                
                setFormData({
                    fullName: userData.fullName || '',
                    email: userData.email || '',
                    phoneNumber: userData.phoneNumber || '',
                    cep: userData.address?.cep || '',
                    street: userData.address?.street || '',
                    number: userData.address?.number || '',
                    complement: userData.address?.complement || '',
                    neighborhood: userData.address?.neighborhood || '',
                    city: userData.address?.city || '',
                    state: userData.address?.state || ''
                });

            } catch (err) {
                setError('Erro ao carregar seus dados. Tente novamente mais tarde.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };
        fetchUserData();
    }, []); // Array vazio garante que rode apenas uma vez
    
    // 3. FUNÇÃO GENÉRICA PARA ATUALIZAR O STATE DO FORMULÁRIO
    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    // 4. LÓGICA DO ViaCEP
    const handleCepBlur = async (e) => {
        const currentCep = e.target.value.replace(/\D/g, '');
        if (currentCep.length !== 8) return;
        
        setCepLoading(true);
        setError('');
        try {
            const response = await axios.get(`https://viacep.com.br/ws/${currentCep}/json/`);
            if (response.data.erro) {
                setError('CEP não encontrado.');
            } else {
                setFormData(prevData => ({
                    ...prevData,
                    street: response.data.logradouro,
                    neighborhood: response.data.bairro,
                    city: response.data.localidade,
                    state: response.data.uf
                }));
            }
        } catch (err) {
            setError('Erro ao buscar o CEP.');
        } finally {
            setCepLoading(false);
        }
    };
    
    // 5. LÓGICA DE SUBMISSÃO PARA ATUALIZAR
    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');
        try {
            await api.put('/api/users/me', formData);
            alert('Perfil atualizado com sucesso!');
            navigate('/dashboard');
        } catch (err) {
            setError('Erro ao atualizar o perfil. Verifique os dados e tente novamente.');
            console.error(err);
        }
    };

    if (loading) return <p style={{color: 'white', textAlign: 'center'}}>Carregando perfil...</p>;

    // 6. ESTRUTURA JSX COMPLETA
    return (
        <div className={styles.tela}>
            <div className={styles.container}>
                <form onSubmit={handleSubmit}>
                    <h1>Editar Perfil</h1>
                    
                    <div className={styles.inputGroup}>
                        <label>Nome Completo</label>
                        <input type="text" name="fullName" value={formData.fullName} onChange={handleChange} required />
                    </div>
                    <div className={styles.inputGroup}>
                        <label>E-mail (não pode ser alterado)</label>
                        <input type="email" name="email" value={formData.email} disabled />
                    </div>
                    <div className={styles.inputGroup}>
                        <label>Celular (Opcional)</label>
                        <input type="tel" name="phoneNumber" value={formData.phoneNumber} onChange={handleChange} />
                    </div>

                    <hr style={{margin: '20px 0', borderColor: 'rgba(255,255,255,0.2)'}} />
                    
                    <div className={styles.inputGroup}>
                        <label>CEP {cepLoading && <span>(Buscando...)</span>}</label>
                        <input type="text" name="cep" value={formData.cep} onChange={handleChange} onBlur={handleCepBlur} required />
                    </div>
                     <div className={styles.inputGroup}>
                        <label>Rua / Logradouro</label>
                        <input type="text" name="street" value={formData.street} onChange={handleChange} required disabled={cepLoading} />
                    </div>

                    <div className={styles.addressFields}>
                        <div className={`${styles.inputGroup} ${styles.numberField}`}>
                             <label>Nº</label>
                            <input type="text" name="number" value={formData.number} onChange={handleChange} required disabled={cepLoading} />
                        </div>
                        <div className={styles.inputGroup}>
                            <label>Complemento</label>
                            <input type="text" name="complement" value={formData.complement} onChange={handleChange} disabled={cepLoading} />
                        </div>
                    </div>

                    <div className={styles.inputGroup}>
                        <label>Bairro</label>
                        <input type="text" name="neighborhood" value={formData.neighborhood} onChange={handleChange} required disabled={cepLoading} />
                    </div>
                    
                    <div className={styles.addressFields}>
                        <div className={styles.inputGroup}>
                            <label>Cidade</label>
                            <input type="text" name="city" value={formData.city} onChange={handleChange} required disabled={cepLoading} />
                        </div>
                        <div className={`${styles.inputGroup} ${styles.numberField}`}>
                            <label>UF</label>
                            <input type="text" name="state" value={formData.state} onChange={handleChange} required disabled={cepLoading} />
                        </div>
                    </div>

                    {error && <p className={styles.error}>{error}</p>}
                    <button type="submit" className={styles.button}>Salvar Alterações</button>
                </form>
            </div>
        </div>
    );
}

export default EditarPerfil;