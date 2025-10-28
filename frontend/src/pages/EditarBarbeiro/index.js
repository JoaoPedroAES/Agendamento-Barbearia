import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api'; 
import styles from './EditarBarbeiro.module.css'; 
import { useAuth } from '../../context/AuthContext'; 

function EditarBarbeiro() {
    const { user } = useAuth(); 
    const navigate = useNavigate();

    
    const [formData, setFormData] = useState({
        fullName: '',
        email: '',
        phoneNumber: '',
        bio: '' 
    });
    
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    
    useEffect(() => {
        
        if (user && user.barberId) {
            const fetchBarberData = async () => {
                setLoading(true);
                setError(''); 
                try {
                    
                    const response = await api.get(`/api/Barber/${user.barberId}`); 
                    const data = response.data;
                    
                    setFormData({
                        fullName: data.fullName || '',
                        email: data.email || user.email, 
                        phoneNumber: data.phoneNumber || '',
                        bio: data.bio || '' 
                    });

                } catch (err) {
                    setError('Erro ao carregar seus dados. Tente novamente mais tarde.'); 
                    console.error("Erro ao buscar dados:", err);
                } finally {
                    setLoading(false);
                }
            };
            fetchBarberData();
        } else if (user) {
            
            setLoading(false);
            setError("Não foi possível encontrar o ID de barbeiro associado a esta conta.");
        }
        
    }, [user]); 
    
    
    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    
    const handleVoltar = (e) => {
        e.preventDefault();
        navigate(-1); 
    };
    
    
    const handleSubmit = async (e) => {
        e.preventDefault();
        setError('');

        const barberId = user?.barberId;
        if (!barberId) {
            setError("Erro: ID do barbeiro não encontrado. Faça login novamente.");
            return;
        }

        try {
            
            await api.put(`/api/Barber/${barberId}`, formData); 
            alert('Perfil atualizado com sucesso!');
            navigate('/gestao'); 
        } catch (err) {
            console.error("Erro ao salvar perfil:", err.response);
            setError('Erro ao atualizar o perfil. Verifique os dados e tente novamente.');
        }
    };

    if (loading && !error) return <p style={{color: 'white', textAlign: 'center'}}>Carregando perfil...</p>;

    
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

                    {}
                    <div className={styles.inputGroup}>
                        <label>Bio / Descrição (Opcional)</label>
                        <textarea
                            name="bio"
                            className={styles.textarea} 
                            value={formData.bio}
                            onChange={handleChange}
                            placeholder="Fale um pouco sobre você..."
                        />
                    </div>
                    
                    <div className={styles.inputGroup}>
                        <label>Celular / WhatsApp</label>
                        <input type="tel" name="phoneNumber" value={formData.phoneNumber} onChange={handleChange} />
                    </div>

                    {error && <p className={styles.error}>{error}</p>}

                    {}
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

export default EditarBarbeiro;