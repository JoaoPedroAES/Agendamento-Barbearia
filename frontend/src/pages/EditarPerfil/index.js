import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import api from '../../services/api';
import styles from './EditarPerfil.module.css'; 

function EditarPerfil() {
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
    
    const [loading, setLoading] = useState(true);
    const [cepLoading, setCepLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    // =========================================================================
    // MÁSCARAS E FORMATAÇÃO (Igual ao Cadastro)
    // =========================================================================

    const formatTextOnly = (value) => value.replace(/[^A-Za-zÀ-ÖØ-öø-ÿ\s]/g, '');
    const formatAlphaNumeric = (value) => value.replace(/[^A-Za-z0-9\s]/g, '');
    const formatNumberOnly = (value) => value.replace(/\D/g, '').slice(0, 4);

    // =========================================================================
    // CARREGAMENTO INICIAL
    // =========================================================================

    useEffect(() => {
        const fetchUserData = async () => {
            try {
                const response = await api.get('/api/users/me');
                const userData = response.data;
                
                // Formata dados iniciais (ex: coloca máscara no telefone se vier limpo)
                let phone = userData.phoneNumber || '';
                if (phone) {
                    phone = phone.replace(/\D/g, "").replace(/^(\d{2})(\d)/g, "($1) $2").replace(/(\d{5})(\d)/, "$1-$2");
                }

                let cep = userData.address?.cep || '';
                if (cep) {
                    cep = cep.replace(/\D/g, "").replace(/^(\d{5})(\d)/, "$1-$2");
                }

                setFormData({
                    fullName: userData.fullName || '',
                    email: userData.email || '',
                    phoneNumber: phone,
                    cep: cep,
                    street: userData.address?.street || '',
                    number: userData.address?.number || '',
                    complement: userData.address?.complement || '',
                    neighborhood: userData.address?.neighborhood || '',
                    city: userData.address?.city || '',
                    state: userData.address?.state || ''
                });

            } catch (err) {
                setError('Erro ao carregar seus dados. Tente novamente mais tarde.');
            } finally {
                setLoading(false);
            }
        };
        fetchUserData();
    }, []); 
    
    // =========================================================================
    // HANDLERS DE MUDANÇA (Com validação)
    // =========================================================================

    const handleChange = (e) => {
        const { name, value } = e.target;
        let newValue = value;

        // Aplica a regra dependendo do campo
        if (name === 'fullName' || name === 'street' || name === 'neighborhood' || name === 'city') {
            newValue = formatTextOnly(value);
        } else if (name === 'number') {
            newValue = formatNumberOnly(value);
        } else if (name === 'complement') {
            newValue = formatAlphaNumeric(value);
        } else if (name === 'state') {
            newValue = value.replace(/[^a-zA-Z]/g, '').toUpperCase().slice(0, 2);
        } else if (name === 'phoneNumber') {
            // Máscara de Telefone
            newValue = value.replace(/\D/g, "").slice(0, 11).replace(/^(\d{2})(\d)/g, "($1) $2").replace(/(\d{5})(\d)/, "$1-$2");
        } else if (name === 'cep') {
            // Máscara de CEP
            newValue = value.replace(/\D/g, "").slice(0, 8).replace(/^(\d{5})(\d)/, "$1-$2");
        }

        setFormData({ ...formData, [name]: newValue });
    };

    const handleCepBlur = async (e) => {
        const currentCep = e.target.value.replace(/\D/g, '');
        if (currentCep.length !== 8) return;
        
        setCepLoading(true);
        setError('');
        try {
            const response = await axios.get(`https://viacep.com.br/ws/${currentCep}/json/`);
            
            if (response.data.erro) {
                setError('CEP não encontrado na base de dados.');
                setFormData(prev => ({...prev, street: '', neighborhood: '', city: '', state: ''}));
            } else {
                setFormData(prevData => ({
                    ...prevData,
                    street: formatTextOnly(response.data.logradouro),
                    neighborhood: formatTextOnly(response.data.bairro),
                    city: formatTextOnly(response.data.localidade),
                    state: response.data.uf, 
                    complement: '', 
                }));
                
                // Foca no campo número após preencher
                setTimeout(() => document.getElementsByName('number')[0]?.focus(), 100);
            }
        } catch (err) {
            if (!err.response) {
                setError('Serviço de busca indisponível. Preencha manualmente.');
            } else {
                setError('Erro ao buscar o CEP.');
            }
        } finally {
            setCepLoading(false);
        }
    };

    const handleVoltar = (e) => {
        e.preventDefault();
        navigate(-1); 
    };
    
    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        // Validações antes de enviar
        if (formData.fullName.trim().split(' ').length < 2) {
            setError('Por favor, preencha seu nome completo.');
            return;
        }
        
        // Limpa máscaras para envio
        const dataToSend = {
            ...formData,
            phoneNumber: formData.phoneNumber.replace(/\D/g, ''),
            cep: formData.cep.replace(/\D/g, '')
        };

        try {
            await api.put('/api/users/me', dataToSend);
            alert('Perfil atualizado com sucesso!');
            navigate('/dashboard');
        } catch (err) {
            setError('Erro ao atualizar o perfil. Verifique os dados e tente novamente.');
        }
    };

    if (loading) return <div className={styles.page}><p style={{color: 'white', textAlign: 'center'}}>Carregando perfil...</p></div>;

    return (
        <div className={styles.page}>
            <div className={styles.formContainer}>
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
                        <label>Celular / WhatsApp</label>
                        <input type="tel" name="phoneNumber" value={formData.phoneNumber} onChange={handleChange} />
                    </div>

                    <hr className={styles.divider} />
                    
                    <div className={styles.inputGroup}>
                        <label>CEP {cepLoading && <span>(Buscando...)</span>}</label>
                        <input type="text" name="cep" value={formData.cep} onChange={handleChange} onBlur={handleCepBlur} required />
                    </div>
                      <div className={styles.inputGroup}>
                        <label>Rua / Logradouro</label>
                        <input type="text" name="street" value={formData.street} onChange={handleChange} required disabled={cepLoading} />
                    </div>

                    <div className={styles.row}>
                        <div className={styles.inputGroup}>
                            <label>Nº</label>
                            <input 
                                type="text" 
                                name="number" 
                                value={formData.number} 
                                onChange={handleChange} 
                                required 
                                disabled={cepLoading} 
                                inputMode="numeric"
                            />
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
                    
                    <div className={styles.row}>
                        <div className={styles.inputGroup}>
                            <label>Cidade</label>
                            <input type="text" name="city" value={formData.city} onChange={handleChange} required disabled={cepLoading} />
                        </div>
                        <div className={styles.inputGroup} style={{ flex: '0.5' }}>
                            <label>UF</label>
                            <input 
                                type="text" 
                                name="state" 
                                value={formData.state} 
                                onChange={handleChange} 
                                maxLength="2" 
                                required 
                                disabled={cepLoading} 
                            />
                        </div>
                    </div>

                    {error && <p className={styles.error}>{error}</p>}
                    
                    <div className={styles.buttonContainer}>
                        <button type="button" onClick={handleVoltar} className={styles.backButton}>
                            Voltar
                        </button>
                        <button type="submit" className={styles.saveButton}>
                            Salvar Alterações
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default EditarPerfil;