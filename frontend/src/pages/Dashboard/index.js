import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import api from '../../services/api';
import styles from './Dashboard.module.css'; 
import { useAuth } from '../../context/AuthContext';
import { FaCog } from 'react-icons/fa'; 

function Dashboard() {
    const { user, logout } = useAuth();
    const navigate = useNavigate();

    const [agendamentos, setAgendamentos] = useState([]);
    const [servicos, setServicos] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [menuOpen, setMenuOpen] = useState(false); 

    
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

    if (loading) {
        return <div className={styles.page}><p style={{color: 'white', textAlign: 'center'}}>Carregando...</p></div>;
    }

    return (
        <div className={styles.page}>
            {}
            <header className={styles.header}>
                <h1>Olá, <strong>{user?.fullName.split(' ')[0]}!</strong></h1>
                <div style={{ display: 'flex', alignItems: 'center' }}>
                    <span className={styles.welcomeMessage}>Bem-vindo(a)!</span>
                    <div className={styles.settingsMenu}>
                        <button onClick={() => setMenuOpen(!menuOpen)} className={styles.gearButton}>
                            <FaCog />
                        </button>
                        {menuOpen && (
                            <div className={styles.dropdown}>
                                {}
                                <button onClick={() => navigateAndClose('/barbeiros')}>Barbeiros</button>
                                <button onClick={() => navigateAndClose('/perfil')}>Editar Perfil</button>
                                <button onClick={handleDeleteAccount} style={{color: 'red'}}>Deletar Conta</button>
                                <hr style={{borderColor: '#444', margin: '5px 0'}} />
                                <button onClick={handleLogout}>Sair</button>
                            </div>
                        )}
                    </div>
                </div>
            </header>

            <main className={styles.content}>
                
                {error && <p className={styles.error}>{error}</p>}

                {}
                <section className={styles.section}>
                    <h2>Meus Próximos Agendamentos</h2>
                    <div className={styles.appointmentsGrid}>
                        {agendamentos.length > 0 ? agendamentos.map(app => (
                            <div key={app.id} className={styles.appointmentCard}>
                                <div className={styles.cardHeader}>
                                    <span className={styles.date}>{new Date(app.startDateTime).toLocaleDateString('pt-BR', {day: '2-digit', month: '2-digit'})}</span>
                                    <span className={styles.time}>{new Date(app.startDateTime).toLocaleTimeString('pt-BR', {hour: '2-digit', minute: '2-digit'})}</span>
                                </div>
                                <p><strong>Barbeiro:</strong> {app.barber?.userAccount?.fullName || 'N/A'}</p>
                                <p><strong>Serviços:</strong> {app.services.map(s => s.name).join(', ')}</p>
                                <p><strong>Status:</strong> <span className={styles.statusAgendado}>{app.status === 0 ? 'Agendado' : 'Cancelado'}</span></p>
                                {app.status === 0 && (
                                    <button onClick={() => handleCancelar(app.id)} className={styles.cancelButton}>
                                        Cancelar
                                    </button>
                                )}
                            </div>
                        )) : (
                            <p style={{color: '#ccc'}}>Você não possui agendamentos futuros.</p>
                        )}
                    </div>
                    <Link to="/agendamento" className={styles.agendarButton}>
                        + Agendar Novo Horário
                    </Link>
                </section>

                {}
                <section className={styles.section}>
                    <h2>Nossos Serviços</h2>
                    <div className={styles.servicesGrid}>
                        {servicos.map(service => (
                            <div key={service.id || service._id} className={styles.serviceTag}>
                                {service.name} <span>R$ {Number(service.price).toLocaleString('pt-BR', {minimumFractionDigits: 2})}</span>
                            </div>
                        ))}
                    </div>
                </section>

            </main>
        </div>
    );
}

export default Dashboard;