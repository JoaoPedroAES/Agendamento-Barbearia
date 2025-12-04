import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../../services/api';
import styles from './Dashboard.module.css'; 
import { useAuth } from '../../context/AuthContext';
import { FaCog, FaTimes } from 'react-icons/fa'; 

function Dashboard() {
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const [agendamentos, setAgendamentos] = useState([]);
    const [servicos, setServicos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [menuOpen, setMenuOpen] = useState(false); 
    
    // Filtro de data simples (String YYYY-MM-DD)
    const [filterDate, setFilterDate] = useState('');

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            try {
                const agendaResponse = await api.get('/api/appointments/my-appointments');
                const sortedAgenda = agendaResponse.data.sort((a, b) => new Date(a.startDateTime) - new Date(b.startDateTime));
                setAgendamentos(sortedAgenda);

                const servicosResponse = await api.get('/api/Services'); 
                setServicos(servicosResponse.data);
                
                setError(null);
            } catch (err) {
                console.error("Erro ao buscar dados:", err);
                setError("Não foi possível carregar os dados do painel.");
            } finally {
                setLoading(false);
            }
        };

        if (user) {
            fetchData();
        }
    }, [user]);

    const navigateAndClose = (path) => {
        navigate(path);
        setMenuOpen(false);
    };

    const handleLogout = () => {
        logout();
        setMenuOpen(false);
        navigate('/login');
    };

    const handleDeleteAccount = async () => {
        if (window.confirm('Você tem certeza que deseja deletar sua conta? Esta ação é irreversível.')) {
            try {
                await api.delete('/api/users/me');
                alert('Sua conta foi deletada com sucesso.');
                handleLogout();
            } catch (err) {
                setError('Não foi possível deletar sua conta.');
            }
        }
    };

    const handleCancelar = async (appointmentId) => {
        if (window.confirm('Tem certeza que deseja cancelar este agendamento?')) {
            try {
                await api.put(`/api/appointments/${appointmentId}/cancel`); 
                alert('Agendamento cancelado com sucesso!');
                
                const agendaResponse = await api.get('/api/appointments/my-appointments');
                const sortedAgenda = agendaResponse.data.sort((a, b) => new Date(a.startDateTime) - new Date(b.startDateTime));
                setAgendamentos(sortedAgenda);
            } catch (err) {
                console.error("Erro ao cancelar:", err);
                alert('Erro ao cancelar o agendamento.');
            }
        }
    };

    // --- LÓGICA DO FILTRO SIMPLES ---
    const displayedAppointments = filterDate 
        ? agendamentos.filter(app => app.startDateTime.startsWith(filterDate))
        : agendamentos; 

    if (loading) {
        return <div className={styles.page}><p style={{color: 'white', textAlign: 'center'}}>Carregando...</p></div>;
    }

    return (
        <div className={styles.page}>
            
            <header className={styles.header}>
                <h1>Painel do Cliente</h1>
                <div style={{ display: 'flex', alignItems: 'center' }}>
                    <span className={styles.welcomeMessage}>Bem-vindo(a), <strong>{user?.fullName.split(' ')[0]}!</strong></span>
                    <div className={styles.settingsMenu}>
                        <button onClick={() => setMenuOpen(!menuOpen)} className={styles.gearButton}>
                            <FaCog />
                        </button>
                        {menuOpen && (
                            <div className={styles.dropdown}>
                                <button onClick={() => navigateAndClose('/perfil')}>Editar Perfil</button>
                                <button onClick={handleDeleteAccount} style={{color: 'red'}}>Deletar Conta</button>
                                <hr style={{borderColor: '#444', margin: '5px 0'}} />
                                <button onClick={handleLogout}>Sair</button>
                            </div>
                        )}
                    </div>
                </div>
            </header>

            {error && <p className={styles.error}>{error}</p>}

            <div className={styles.mainGrid}>
                
                {/* COLUNA ESQUERDA: AGENDAMENTOS */}
                <section className={styles.agendaSection}>
                    <div className={styles.sectionHeader}>
                        <h2>Meus Agendamentos</h2>
                    </div>

                    {/* --- FILTRO SIMPLES (Igual ao Barbeiro) --- */}
                    <div className={styles.filterControls}>
                        <label htmlFor="date-filter">Filtrar por Data:</label>
                        <div className={styles.inputWrapper}>
                            <input 
                                type="date" 
                                id="date-filter"
                                className={styles.dateInput}
                                value={filterDate} 
                                onChange={(e) => setFilterDate(e.target.value)} 
                            />
                            {filterDate && (
                                <button onClick={() => setFilterDate('')} className={styles.clearButton} title="Limpar Filtro">
                                    <FaTimes />
                                </button>
                            )}
                        </div>
                    </div>

                    <div className={styles.appointmentList}>
                        {displayedAppointments.length > 0 ? displayedAppointments.map(app => (
                            <div key={app.id} className={styles.appointmentCard}>
                                <div className={styles.cardHeader}>
                                    <span className={styles.date}>
                                        {new Date(app.startDateTime).toLocaleDateString('pt-BR', {day: '2-digit', month: '2-digit', timeZone: 'UTC'})}
                                    </span>
                                    <span className={styles.time}>
                                        {new Date(app.startDateTime).toLocaleTimeString('pt-BR', {hour: '2-digit', minute: '2-digit', timeZone: 'UTC'})}
                                    </span>
                                </div>
                                <p><strong>Barbeiro:</strong> {app.barber?.userAccount?.fullName || 'N/A'}</p>
                                <p><strong>Serviços:</strong> {app.services.map(s => s.name).join(', ')}</p>
                                <p><strong>Status:</strong> <span className={app.status === 0 ? styles.statusAgendado : styles.statusCancelado}>{app.status === 0 ? 'Agendado' : 'Cancelado'}</span></p>
                                {app.status === 0 && (
                                    <button onClick={() => handleCancelar(app.id)} className={styles.cancelButton}>
                                        Cancelar
                                    </button>
                                )}
                            </div>
                        )) : (
                            <div className={styles.emptyState}>
                                <p>Nenhum agendamento encontrado.</p>
                                {filterDate && (
                                    <button onClick={() => setFilterDate('')} className={styles.linkButton}>
                                        Ver todos os agendamentos
                                    </button>
                                )}
                            </div>
                        )}
                    </div>
                </section>

                {/* COLUNA DIREITA: AÇÕES RÁPIDAS E SERVIÇOS */}
                <section className={styles.actionsSection}>
                    <h2>Ações Rápidas</h2>
                    
                    <Link to="/agendamento" className={styles.actionButton}>
                        Agendar Novo Horário
                    </Link>

                    <div className={styles.servicesContainer}>
                        <h3>Nossos Serviços</h3>
                        <div className={styles.servicesList}>
                            {servicos.map(service => (
                                <div key={service.id || service._id} className={styles.serviceItem}>
                                    <span className={styles.serviceName}>{service.name}</span>
                                    <span className={styles.servicePrice}>R$ {Number(service.price).toLocaleString('pt-BR', {minimumFractionDigits: 2})}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                </section>

            </div>
        </div>
    );
}

export default Dashboard;