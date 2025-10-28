import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import styles from './ListarBarbeiros.module.css';
import { FaArrowLeft } from 'react-icons/fa'; 

function ListarBarbeiros() {
    const navigate = useNavigate();
    const [barbers, setBarbers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    
    useEffect(() => {
        const fetchBarbers = async () => {
            try {
                setLoading(true);
                
                const response = await api.get('/api/barber'); 
                setBarbers(response.data);
                setError(null);
            } catch (err) {
                console.error("Erro ao buscar barbeiros:", err);
                setError("Não foi possível carregar a lista de barbeiros.");
            } finally {
                setLoading(false);
            }
        };

        fetchBarbers();
    }, []);

    
    const handleVoltar = () => {
        navigate(-1); 
    };

    return (
        <div className={styles.page}>
            <header className={styles.header}>
                <button onClick={handleVoltar} className={styles.backButton}>
                    <FaArrowLeft /> Voltar
                </button>
                <h1>Nossos Barbeiros</h1>
                <div style={{width: '100px'}}></div> {}
            </header>

            <main className={styles.content}>
                {loading && <p style={{color: '#ccc', textAlign: 'center'}}>Carregando...</p>}
                
                {error && <p className={styles.error}>{error}</p>}
                
                {!loading && !error && (
                    <div className={styles.cardGrid}>
                        {barbers.length > 0 ? barbers.map(barber => (
                            <div key={barber.barberId} className={styles.barberCard}>
                                <h3>{barber.fullName}</h3>
                                
                                <p className={styles.infoItem}>
                                    <strong>Email:</strong> {barber.email || 'N/A'}
                                </p>
                                <p className={styles.infoItem}>
                                    <strong>Telefone:</strong> {barber.phoneNumber || 'N/A'}
                                </p>
                                
                                {barber.bio && (
                                    <p className={styles.bio}>
                                        {barber.bio}
                                    </p>
                                )}
                            </div>
                        )) : (
                            <p style={{color: '#ccc'}}>Nenhum barbeiro encontrado.</p>
                        )}
                    </div>
                )}
            </main>
        </div>
    );
}

export default ListarBarbeiros;