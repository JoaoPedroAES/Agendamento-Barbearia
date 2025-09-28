// src/pages/Dashboard/index.js

import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import styles from './Dashboard.module.css';
import { FaCog } from 'react-icons/fa'; // Ícone de engrenagem

function Dashboard() {
    // 1. STATES: Para armazenar dados e controlar a UI
    const [user, setUser] = useState(null);
    const [appointments, setAppointments] = useState([]);
    const [services, setServices] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [menuOpen, setMenuOpen] = useState(false); // Controla o menu de configurações
    const navigate = useNavigate();

    // 2. useEffect: Roda quando o componente carrega para buscar os dados
    useEffect(() => {
        const fetchData = async () => {
            try {
                // Busca os dados em paralelo para mais performance
                const [userResponse, appointmentsResponse, servicesResponse] = await Promise.all([
                    api.get('/api/users/me'),
                    api.get('/api/appointments/my-appointments'),
                    api.get('/api/services')
                ]);

                setUser(userResponse.data);
                setAppointments(appointmentsResponse.data);
                setServices(servicesResponse.data);

            } catch (err) {
                setError('Não foi possível carregar os dados do painel.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, []); // O array vazio [] garante que isso rode apenas uma vez

    // 3. FUNÇÕES DE AÇÃO
    const handleLogout = () => {
        localStorage.removeItem('authToken'); // Remove o token
        api.defaults.headers.common['Authorization'] = null; // Limpa o cabeçalho
        navigate('/login'); // Redireciona para o login
    };

    const handleDeleteAccount = async () => {
        if (window.confirm('Você tem certeza que deseja deletar sua conta? Esta ação é irreversível.')) {
            try {
                await api.delete('/api/users/me');
                alert('Sua conta foi deletada com sucesso.');
                handleLogout(); // Desloga o usuário após deletar
            } catch (err) {
                setError('Não foi possível deletar sua conta. Tente novamente.');
                console.error(err);
            }
        }
    };

    const handleCancelAppointment = async (appointmentId) => {
        if (window.confirm('Tem certeza que deseja cancelar este agendamento?')) {
            try {
                await api.put(`/api/appointments/${appointmentId}/cancel`);
                // Atualiza a lista de agendamentos para refletir a mudança
                const updatedAppointments = appointments.map(app => 
                    app.id === appointmentId ? { ...app, status: 2 } : app // 2 = CancelledByCustomer
                );
                setAppointments(updatedAppointments);
            } catch (err) {
                setError('Não foi possível cancelar o agendamento.');
                console.error(err);
            }
        }
    };


    // 4. RENDERIZAÇÃO (O que aparece na tela)
    if (loading) {
        return <p>Carregando seu painel...</p>;
    }

    if (error) {
        return <p style={{ color: 'red' }}>{error}</p>;
    }

    return (
        <div className={styles.dashboard}>
            <header className={styles.header}>
                <h1>Olá, {user?.fullName}!</h1>
                <div className={styles.settingsMenu}>
                    <button onClick={() => setMenuOpen(!menuOpen)} className={styles.gearButton}>
                        <FaCog />
                    </button>
                    {menuOpen && (
                        <div className={styles.dropdown}>
                            <button onClick={() => navigate('/perfil')}>Editar Perfil</button>
                            <button onClick={handleDeleteAccount} style={{color: '#ff4d4d'}}>Deletar Conta</button>
                            <hr />
                            <button onClick={handleLogout}>Sair</button>
                        </div>
                    )}
                </div>
            </header>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Meus Próximos Agendamentos</h2>
                {appointments.length > 0 ? (
                    <div className={styles.appointmentList}>
                        {appointments.filter(a => a.status === 0).map(app => ( // Filtra apenas agendamentos 'Scheduled'
                            <div key={app.id} className={styles.appointmentCard}>
                                <h3>
                                    {new Date(app.startDateTime).toLocaleDateString('pt-BR', { timeZone: 'UTC' })} - 
                                    {new Date(app.startDateTime).toLocaleTimeString('pt-BR', {
                                        hour: '2-digit', 
                                        minute:'2-digit', 
                                        timeZone: 'UTC' // <-- A MÁGICA ACONTECE AQUI
                                    })}
                                </h3>
                                <p><strong>Barbeiro:</strong> {app.barber?.userAccount?.fullName || 'N/A'}</p>
                                <p><strong>Serviços:</strong> {app.services.map(s => s.name).join(', ')}</p>
                                <p><strong>Status:</strong> Agendado</p>
                                <button onClick={() => handleCancelAppointment(app.id)}>Cancelar</button>
                            </div>
                        ))}
                    </div>
                ) : (
                    <p>Você não tem agendamentos futuros.</p>
                )}
                <br/>
                <button className={styles.newAppointmentButton} onClick={() => navigate('/agendamento')}>
                 + Agendar Novo Horário
                </button>
            </section>

            <section className={styles.section}>
                <h2 className={styles.sectionTitle}>Nossos Serviços</h2>
                <div className={styles.serviceList}>
                    {services.map(service => (
                        <div key={service.id} className={styles.serviceCard}>
                            <strong>{service.name}</strong> - R$ {service.price}
                        </div>
                    ))}
                </div>
            </section>
        </div>
    );
}

export default Dashboard;