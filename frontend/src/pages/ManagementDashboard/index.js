import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import { Link } from 'react-router-dom'; 
import styles from './ManagementDashboard.module.css';
import { useAuth } from '../../context/AuthContext'; 

import { FaCog } from 'react-icons/fa'; 

const daysOfWeek = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'];
const initialDayState = { isActive: false, startTime: '09:00', endTime: '18:00', breakStartTime: '12:00', breakEndTime: '13:00' };

function ManagementDashboard() {
    const { user, logout } = useAuth(); 
    const navigate = useNavigate();

    
    const [agenda, setAgenda] = useState([]);
    const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
    const [allBarbers, setAllBarbers] = useState([]);
    const [filterBarberId, setFilterBarberId] = useState('');

    
    const [schedule, setSchedule] = useState({});
    const [editingSchedule, setEditingSchedule] = useState(false);

    
    const [loadingAgenda, setLoadingAgenda] = useState(true);
    const [loadingSchedule, setLoadingSchedule] = useState(true);
    const [loadingInitial, setLoadingInitial] = useState(true);
    const [error, setError] = useState('');
    const [menuOpen, setMenuOpen] = useState(false); 

    const isAdmin = user?.roles.includes('Admin');
    const isStaff = isAdmin || user?.roles.includes('Barbeiro');
    const ownBarberId = user?.barberId; 

    
    useEffect(() => {
        if (user && ownBarberId) {
            if (isAdmin) return; 
            
            const checkTerms = async () => {
                try {
                    const response = await api.get(`/api/Barber/${ownBarberId}`);
                    const barberProfile = response.data;
                    if (barberProfile.hasAcceptedTerms === false) {
                        navigate('/termos-barbeiro');
                    }
                } catch (err) {
                    console.error("Erro ao verificar aceite dos termos:", err);
                    logout(); 
                }
            };
            checkTerms();
        }
    }, [user, ownBarberId, navigate, logout, isAdmin]); 


    
    useEffect(() => {
        const fetchInitialData = async () => {
            if (!user) return;
            setLoadingInitial(true);
            try {
                if (isAdmin) {
                    const barbersResponse = await api.get('/api/barber');
                    setAllBarbers(barbersResponse.data);
                }
            } catch (err) {
                setError('Erro ao carregar lista de barbeiros.');
                console.error("Erro inicial:", err);
            } finally {
                setLoadingInitial(false);
            }
        };
        fetchInitialData();
    }, [user, isAdmin]);

    
    useEffect(() => {
        const fetchAgenda = async () => {
            if (!selectedDate || !user) return;
            setLoadingAgenda(true);
            setError('');
            try {
                const response = await api.get(`/api/appointments/agenda?date=${selectedDate}`);
                setAgenda(response.data);
            } catch (err) {
                setError('Erro ao carregar a agenda do dia.');
                console.error("Erro agenda:", err);
            } finally {
                setLoadingAgenda(false);
            }
        };
        fetchAgenda();
    }, [selectedDate, user]);

    
    useEffect(() => {
        const barberIdToFetch = isAdmin && filterBarberId ? filterBarberId : ownBarberId;
        if (!isStaff || !barberIdToFetch) {
            setSchedule({}); 
            setLoadingSchedule(false);
            return;
        }
        setLoadingSchedule(true);
        setError(''); 
        api.get(`/api/work-schedule/${barberIdToFetch}`)
            .then(res => {
                const scheduleObject = res.data.reduce((acc, day) => {
                    acc[day.dayOfWeek] = {
                        startTime: day.startTime.substring(0, 5),
                        endTime: day.endTime.substring(0, 5),
                        breakStartTime: day.breakStartTime ? day.breakStartTime.substring(0, 5) : '00:00',
                        breakEndTime: day.breakEndTime ? day.breakEndTime.substring(0, 5) : '00:00',
                        isActive: true 
                    };
                    return acc;
                }, {});
                daysOfWeek.forEach((_, index) => {
                    if (!scheduleObject[index]) {
                        scheduleObject[index] = { ...initialDayState };
                    }
                });
                setSchedule(scheduleObject);
            })
            .catch(err => {
                 setError('Erro ao carregar horários de trabalho.');
                 console.error("Erro horários:", err);
            })
            .finally(() => setLoadingSchedule(false));
    }, [ownBarberId, filterBarberId, isAdmin, isStaff]); 


    
    const handleLogout = () => {
        logout();
        setMenuOpen(false);
        navigate('/login');
    };

    
    const handleDayToggle = (dayIndex) => {
        setSchedule(prev => ({
            ...prev,
            [dayIndex]: {
                ...(prev[dayIndex] || initialDayState),
                isActive: !prev[dayIndex]?.isActive
            }
        }));
     };
    const handleTimeChange = (dayIndex, field, value) => {
         setSchedule(prev => ({
            ...prev,
            [dayIndex]: {
                ...(prev[dayIndex] || initialDayState),
                [field]: value
            }
        }));
    };

    
    const handleSaveSchedule = async (e) => {
         e.preventDefault();
         setError(''); 
         const barberIdToSave = isAdmin && filterBarberId ? filterBarberId : ownBarberId;
         if (!barberIdToSave) {
             setError("Nenhum barbeiro selecionado para salvar a agenda.");
             return;
         }
         try {
             const scheduleList = Object.entries(schedule)
                 .filter(([, dayData]) => dayData.isActive) 
                 .map(([dayIndex, dayData]) => ({
                     barberId: parseInt(barberIdToSave),
                     dayOfWeek: parseInt(dayIndex),
                     startTime: `${dayData.startTime || '00:00'}:00`,
                     endTime: `${dayData.endTime || '00:00'}:00`,
                     breakStartTime: `${dayData.breakStartTime || '00:00'}:00`,
                     breakEndTime: `${dayData.breakEndTime || '00:00'}:00`,
                 }));
             await api.post('/api/work-schedule/batch', scheduleList);
             alert('Horários salvos com sucesso!');
             setEditingSchedule(false); 
         } catch (err) {
              setError(err.response?.data?.title || err.response?.data || 'Erro ao salvar horários.');
              console.error("Erro ao salvar horários:", err);
         }
     };

    
    const navigateAndClose = (path) => {
        navigate(path);
        setMenuOpen(false);
    };

    
    const filteredAgenda = isAdmin && filterBarberId
        ? agenda.filter(app => app.barberId == filterBarberId)
        : agenda;
    
    
    if (loadingInitial || !user) return <div className={styles.page}><p>Carregando...</p></div>;

    return (
        <div className={styles.page}>
            
            <header className={styles.header}>
                <h1>Painel de Gestão</h1>
                <div style={{ display: 'flex', alignItems: 'center' }}> 
                    <span className={styles.welcomeMessage}>Bem-vindo(a), <strong>{user.fullName}!</strong></span>
                    <div className={styles.settingsMenu}>
                        <button onClick={() => setMenuOpen(!menuOpen)} className={styles.gearButton}>
                            <FaCog />
                        </button>
                        {menuOpen && (
                            <div className={styles.dropdown}>
                                
                                {/* 1. Botão de Barbeiros para Staff */}
                                {isStaff && ( <button onClick={() => navigateAndClose('/barbeiros')}>Barbeiros</button> )}
                                
                                {/* 2. Botão de Clientes SÓ para Admin (NOVO) */}
                                {isAdmin && ( <button onClick={() => navigateAndClose('/clientes')}>Clientes</button> )}

                                {/* 3. Botão Adicionar Barbeiro para Admin */}
                                {isAdmin && ( <button onClick={() => navigateAndClose('/adicionar-barbeiro')}>Adicionar Barbeiro</button> )}
                                
                                {/* 4. Editar Perfil (REMOVIDO SE FOR ADMIN) */}
                                {!isAdmin && (
                                    <button onClick={() => navigateAndClose(isStaff ? '/editar-barbeiro' : '/perfil')}>Editar Perfil</button>
                                )}
                                
                                <hr style={{borderColor: '#444', margin: '5px 0'}} />
                                <button onClick={handleLogout}>Sair</button>
                            </div>
                        )}
                    </div>
                </div>
            </header>
            
            {error && <p style={{ color: 'red', textAlign: 'center', marginBottom: '1rem' }}>{error}</p>}
            
            <div className={styles.mainGrid}>
                
                <section className={styles.agendaSection}>
                    <h2>Agenda do Dia</h2>
                    <div className={styles.agendaControls}>
                        <label htmlFor="date-picker">Dia:</label>
                        <input type="date" id="date-picker" value={selectedDate} onChange={e => setSelectedDate(e.target.value)} />
                        {isAdmin && ( 
                            <>
                                <label htmlFor="barber-filter">Barbeiro:</label>
                                <select id="barber-filter" value={filterBarberId} onChange={e => setFilterBarberId(e.target.value)}>
                                    <option value="">Todos</option>
                                    {allBarbers.map(barber => <option key={barber.barberId} value={barber.barberId}>{barber.fullName}</option>)}
                                </select>
                            </>
                        )}
                    </div>
                    {loadingAgenda ? <p>Carregando agenda...</p> : (
                        <div className={styles.appointmentList}>
                            {filteredAgenda.length > 0 ? filteredAgenda.map(app => (
                                <div key={app.id} className={styles.appointmentCard}>
                                    <h3>{new Date(app.startDateTime).toLocaleTimeString('pt-BR', {timeZone: 'UTC', hour: '2-digit', minute: '2-digit'})}</h3>
                                    <p><strong>Cliente:</strong> {app.customer.fullName}</p>
                                    {isAdmin && !filterBarberId && <p><strong>Barbeiro:</strong> {app.barber?.userAccount?.fullName || 'N/A'}</p>}
                                    <p><strong>Serviços:</strong> {app.services.map(s => s.name).join(', ')}</p>
                                    <p><strong>Status:</strong> {app.status === 0 ? 'Agendado' : 'Cancelado'}</p>
                                </div>
                            )) : <p>Nenhum agendamento para este dia{filterBarberId ? ' para este barbeiro' : ''}.</p>}
                        </div>
                    )}
                </section>
                
                <section className={styles.actionsSection}>
                    <h2>Ações Rápidas</h2>
                    
                    {((isStaff && !isAdmin) || (isAdmin && filterBarberId)) && (
                        <button className={styles.actionButton} onClick={() => setEditingSchedule(!editingSchedule)}>
                            {editingSchedule ? 'Fechar Edição de Horários' : (isAdmin && filterBarberId ? `Editar Horários (${allBarbers.find(b => b.barberId == filterBarberId)?.fullName || 'Selecionado'})` : 'Editar Meus Horários')}
                        </button>
                    )}
                    
                    {isAdmin && (
                         <button className={styles.actionButton} onClick={() => navigate('/adicionar-barbeiro')}>Adicionar Barbeiro</button>
                    )}
                      
                    {isStaff && (
                         <Link to="/servicos" className={styles.actionButton}>
                             Gerenciar Serviços
                         </Link>
                    )}
                    
                    {editingSchedule && ((isStaff && !isAdmin) || (isAdmin && filterBarberId)) && (
                        <form className={styles.scheduleContainer} onSubmit={handleSaveSchedule}>
                            <h3>
                                {isAdmin && filterBarberId
                                    ? `Editando horários de ${allBarbers.find(b => b.barberId == filterBarberId)?.fullName}`
                                    : "Meus Horários de Trabalho"
                                }
                            </h3>
                            {loadingSchedule ? <p>Carregando...</p> : (
                                
                                daysOfWeek.map((dayName, index) => {
                                    const dayData = schedule[index] || initialDayState; 
                                    const isActive = dayData.isActive;
                                    return (
                                        <div key={index} className={styles.dayRow}>
                                            <label className={styles.dayLabel}>
                                                <input type="checkbox" checked={isActive} onChange={() => handleDayToggle(index)} />
                                                {dayName}
                                            </label>
                                            
                                            <div className={styles.timeInputs}>
                                                <input type="time" value={dayData.startTime} disabled={!isActive} onChange={(e) => handleTimeChange(index, 'startTime', e.target.value)} />
                                                <span>-</span>
                                                <input type="time" value={dayData.endTime} disabled={!isActive} onChange={(e) => handleTimeChange(index, 'endTime', e.target.value)} />
                                            </div>
                                             
                                             <div className={styles.timeInputs} style={{marginLeft: 'auto'}}> 
                                                 <span>Pausa:</span>
                                                 <input type="time" value={dayData.breakStartTime} disabled={!isActive} onChange={(e) => handleTimeChange(index, 'breakStartTime', e.target.value)} />
                                                 <span>-</span>
                                                 <input type="time" value={dayData.breakEndTime} disabled={!isActive} onChange={(e) => handleTimeChange(index, 'breakEndTime', e.target.value)} />
                                             </div>
                                        </div>
                                    );
                                })
                            )}
                            
                            <button type="submit" className={styles.saveScheduleButton} disabled={loadingSchedule}>Salvar Horários</button>
                        </form>
                    )}
                </section>
            </div>
            
        </div>
    );
}

export default ManagementDashboard;