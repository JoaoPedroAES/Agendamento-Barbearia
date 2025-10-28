import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api'; 
import styles from './GerenciarServicos.module.css';

import { FaPen, FaTrash, FaArrowLeft } from 'react-icons/fa'; 

function GerenciarServicos() {
    const navigate = useNavigate();
    

    const [services, setServices] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    
    const [isEditing, setIsEditing] = useState(null); 
    const [formData, setFormData] = useState({
        name: '',
        price: '',
        durationInMinutes: '',
        description: '' 
    });

    
    const fetchServices = async () => {
        try {
            setLoading(true);
            const response = await api.get('/api/Services'); 
            setServices(response.data);
            setError(null);
        } catch (err) {
            console.error("Erro ao buscar serviços:", err);
            setError("Não foi possível carregar os serviços.");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchServices();
    }, []);

    
    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    
    const handleCancel = () => {
        setIsEditing(null);
        setFormData({ name: '', price: '', durationInMinutes: '', description: '' }); 
    };

    
    const handleEditClick = (service) => {
        window.scrollTo(0, 0); 
        const serviceId = service.id || service._id; 
        setIsEditing(serviceId); 
        
        const displayPrice = String(service.price).replace('.', ',');

        setFormData({
            name: service.name,
            price: displayPrice, 
            durationInMinutes: service.durationInMinutes,
            description: service.description || '' 
        });
    };

    
    const handleDelete = async (id) => {
        if (window.confirm("Tem certeza que deseja excluir este serviço?")) {
            try {
                await api.delete(`/api/Services/${id}`);
                alert("Serviço excluído com sucesso!");
                fetchServices(); 
            } catch (err) {
                console.error("Erro ao excluir:", err);
                alert("Erro ao excluir o serviço.");
            }
        }
    };

    
    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(null);

        
        const dataPayload = {
            id: isEditing ? parseInt(isEditing, 10) : 0, 
            name: formData.name,
            description: formData.description, 
            price: parseFloat(String(formData.price).replace(',', '.')),
            durationInMinutes: parseInt(formData.durationInMinutes, 10)
            
        };

        
        if (isNaN(dataPayload.price) || dataPayload.price <= 0) {
             setError("O preço deve ser um número maior que zero.");
             return;
        }
         if (isNaN(dataPayload.durationInMinutes) || dataPayload.durationInMinutes <= 0) {
             setError("A duração deve ser um número maior que zero.");
             return;
        }

        console.log("Enviando para a API:", dataPayload); 

        
        try {
            if (isEditing) {
                
                await api.put(`/api/Services/${isEditing}`, dataPayload);
                alert("Serviço atualizado com sucesso!");
            } else {
                
                await api.post('/api/Services', dataPayload);
                alert("Serviço cadastrado com sucesso!");
            }
            handleCancel();  
            fetchServices(); 
        } catch (err) {
            console.error("Erro ao salvar:", err);
            const apiError = err.response?.data?.title || err.response?.data || "Erro ao salvar o serviço. Verifique os campos.";
            setError(apiError);
        }
    };

    return (
        <div className={styles.page}>
            <header className={styles.header}>
                <button onClick={() => navigate('/gestao')} className={styles.backButton}>
                    <FaArrowLeft /> Voltar
                </button>
                <h1>Gerenciar Serviços</h1>
                <div style={{width: '100px'}}></div> {}
            </header>

            <main className={styles.mainGrid}>
                {}
                <section className={styles.formSection}>
                    <div className={styles.formContainer}>
                        <h2>{isEditing ? 'Editar Serviço' : 'Adicionar Novo Serviço'}</h2>
                        <form onSubmit={handleSubmit}>
                            <div className={styles.inputGroup}>
                                <label htmlFor="name">Nome do Serviço</label>
                                <input type="text" name="name" value={formData.name} onChange={handleChange} required />
                            </div>

                            {}
                            <div className={styles.inputGroup}>
                                <label htmlFor="description">Descrição (Opcional)</label>
                                <textarea 
                                    name="description" 
                                    value={formData.description} 
                                    onChange={handleChange} 
                                    className={styles.textarea} 
                                    placeholder="Descreva o serviço..."
                                />
                            </div>

                            <div className={styles.row}>
                                <div className={styles.inputGroup}>
                                    <label htmlFor="price">Preço (R$)</label>
                                    <input type="text" inputMode="decimal" name="price" placeholder="Ex: 50,00" value={formData.price} onChange={handleChange} required />
                                </div>
                                <div className={styles.inputGroup}>
                                    <label htmlFor="durationInMinutes">Duração (minutos)</label>
                                    <input type="number" name="durationInMinutes" min="0" value={formData.durationInMinutes} onChange={handleChange} required />
                                </div>
                            </div>
                            
                            {error && <p className={styles.error}>{error}</p>}

                            <div className={styles.buttonContainer}>
                                {isEditing && (
                                    <button type="button" onClick={handleCancel} className={styles.cancelButton}>
                                        Cancelar
                                    </button>
                                )}
                                <button type="submit" className={styles.saveButton}>
                                    {isEditing ? 'Salvar Alterações' : 'Adicionar Serviço'}
                                </button>
                            </div>
                        </form>
                    </div>
                </section>

                {}
                <section className={styles.listSection}>
                    <h2>Serviços Cadastrados</h2>
                    {loading && <p style={{color: '#ccc'}}>Carregando...</p>}
                    {!loading && services.length === 0 && (
                        <p style={{color: '#ccc'}}>Nenhum serviço cadastrado ainda.</p>
                    )}
                    
                    <div className={styles.serviceList}>
                        {services.map(service => {
                            const serviceId = service.id || service._id; 
                            return (
                                <div key={serviceId} className={styles.serviceCard}>
                                    <div className={styles.serviceInfo}>
                                        <h3>{service.name}</h3>
                                        <p>Preço: R$ {Number(service.price).toLocaleString('pt-BR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</p>
                                        <p>Duração: {service.durationInMinutes} min</p>
                                        {}
                                        {service.description && <p style={{color: '#aaa', fontSize: '0.9rem'}}><em>{service.description}</em></p>}
                                    </div>
                                    <div className={styles.serviceActions}>
                                        <button onClick={() => handleEditClick(service)} className={styles.iconButton}>
                                            <FaPen />
                                        </button>
                                        <button onClick={() => handleDelete(serviceId)} className={`${styles.iconButton} ${styles.deleteButton}`}>
                                            <FaTrash />
                                        </button>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                </section>
            </main>
        </div>
    );
}

export default GerenciarServicos;