import React, { useState, useEffect, useCallback } from 'react'; // <-- Alterado
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import styles from './ListarBarbeiros.module.css';
import { useAuth } from '../../context/AuthContext';
import { FaArrowLeft, FaTrash } from 'react-icons/fa'; // <-- Alterado

function ListarBarbeiros() {
    const navigate = useNavigate();
    const { user } = useAuth();
    const [barbers, setBarbers] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const isAdmin = user?.roles.includes('Admin');

    
    // Movida para useCallback para ser reutilizável
    const fetchBarbers = useCallback(async () => {
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
    }, []); // <-- Array vazio, a função não muda

    useEffect(() => {
        fetchBarbers();
    }, [fetchBarbers]); // <-- Chama a função

    // --- NOVA FUNÇÃO ADICIONADA ---
    const handleDelete = async (barberId, barberName) => {
        // Pede confirmação
        if (window.confirm(`Tem certeza que deseja desativar o barbeiro "${barberName}"? Esta ação é irreversível.`)) {
            try {
                // Chama a API de delete (que no seu backend deve ser o 'DeactivateBarberAsync')
                await api.delete(`/api/barber/${barberId}`);
                alert('Barbeiro desativado com sucesso!');
                fetchBarbers(); // Atualiza a lista após a exclusão
            } catch (err) {
                console.error("Erro ao desativar barbeiro:", err);
                setError("Erro ao tentar desativar o barbeiro.");
            }
        }
    };
    
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
                                
                                {/* --- BOTÃO DE EXCLUIR ADICIONADO AQUI --- */}
                                {isAdmin && (
                                    <button 
                                        className={styles.deleteButton} 
                                        onClick={() => handleDelete(barber.barberId, barber.fullName)}
                                        title="Desativar Barbeiro"
                                    >
                                        <FaTrash />
                                    </button>
                                )}
                                {/* --- Fim do Botão --- */}

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