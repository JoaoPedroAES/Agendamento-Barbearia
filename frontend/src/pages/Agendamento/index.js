import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import styles from './Agendamento.module.css';
import { FaArrowLeft } from 'react-icons/fa'; // <-- 1. IMPORTAR O ÍCONE

import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { registerLocale } from "react-datepicker";
import ptBR from 'date-fns/locale/pt-BR';
registerLocale('pt-BR', ptBR); 

function Agendamento() {
    
    const [barbers, setBarbers] = useState([]);
    const [services, setServices] = useState([]);
    const [selectedBarber, setSelectedBarber] = useState(null);
    const [selectedServices, setSelectedServices] = useState([]);
    const [selectedDate, setSelectedDate] = useState(null); 
    const [availableSlots, setAvailableSlots] = useState([]);
    const [barberSchedule, setBarberSchedule] = useState([]); 

    
    const [loading, setLoading] = useState(true);
    const [scheduleLoading, setScheduleLoading] = useState(false); 
    const [slotsLoading, setSlotsLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    
    useEffect(() => {
        const fetchInitialData = async () => {
            try {
                const [barbersResponse, servicesResponse] = await Promise.all([
                    api.get('/api/barber'),
                    api.get('/api/services')
                ]);
                setBarbers(barbersResponse.data);
                setServices(servicesResponse.data);
            } catch (err) {
                setError('Erro ao carregar dados. Tente novamente.');
                console.error(err);
            } finally {
                setLoading(false);
            }
        };
        fetchInitialData();
    }, []);

    
    useEffect(() => {
        const fetchBarberSchedule = async () => {
            if (!selectedBarber) {
                setBarberSchedule([]);
                setSelectedDate(null); 
                return;
            }
            
            setScheduleLoading(true);
            try {
                const response = await api.get(`/api/work-schedule/${selectedBarber}`);
                setBarberSchedule(response.data);
            } catch (err) {
                setError('Erro ao carregar os horários do barbeiro.');
            } finally {
                setScheduleLoading(false);
            }
        };
        fetchBarberSchedule();
    }, [selectedBarber]);

    
    useEffect(() => {
        const fetchAvailability = async () => {
            if (selectedBarber && selectedServices.length > 0 && selectedDate) {
                setSlotsLoading(true);
                setAvailableSlots([]);
                try {
                    const serviceIdsQuery = selectedServices.map(id => `serviceIds=${id}`).join('&');
                    const formattedDate = selectedDate.toISOString().split('T')[0]; 
                    const response = await api.get(`/api/availability?barberId=${selectedBarber}&${serviceIdsQuery}&date=${formattedDate}`);
                    setAvailableSlots(response.data);
                } catch (err) {
                    setError('Erro ao buscar horários disponíveis.');
                    console.error(err);
                } finally {
                    setSlotsLoading(false);
                }
            }
        };
        fetchAvailability();
    }, [selectedBarber, selectedServices, selectedDate]);

    
    const handleServiceToggle = (serviceId) => {
        setSelectedServices(prev => 
            prev.includes(serviceId) 
            ? prev.filter(id => id !== serviceId) 
            : [...prev, serviceId] 
        );
    };

    const handleBookAppointment = async (slot) => {
        const [hour, minute] = slot.split(':');
        const startDateTime = new Date(selectedDate);
        startDateTime.setUTCHours(hour, minute, 0, 0); 

        try {
            await api.post('/api/appointments', {
                barberId: selectedBarber,
                startDateTime: startDateTime.toISOString(),
                serviceIds: selectedServices
            });
            alert('Agendamento realizado com sucesso!');
            navigate('/dashboard');
        } catch (err) {
            setError('Falha ao criar agendamento. O horário pode ter sido ocupado.');
            console.error(err);
        }
    };

    
    const isWeekdayAvailable = (date) => {
        if (!barberSchedule || barberSchedule.length === 0) {
            return false;
        }
        const day = date.getDay(); 
        return barberSchedule.some(scheduleDay => scheduleDay.dayOfWeek === day);
    };

    if (loading) return <div className={styles.page}><p className={styles.loadingText}>Carregando...</p></div>;
    if (error) return <p style={{ color: 'red' }}>{error}</p>;

    
    return (
        <div className={styles.page}>
            {/* --- 2. CABEÇALHO ATUALIZADO (TEMA ESCURO) --- */}
            <header className={styles.header}>
                <button onClick={() => navigate('/dashboard')} className={styles.backButton}>
                    <FaArrowLeft /> Voltar
                </button>
                <h1>Faça seu Agendamento</h1>
                <div style={{width: '100px'}}></div> {/* Espaçador */}
            </header>
            
            <div className={styles.selectionContainer}>
                {}
                <div className={styles.column}>
                    <h2>1. Escolha o Barbeiro</h2>
                    {barbers.map(barber => (
                        <div 
                            // <-- 3. PEQUENA CORREÇÃO DE BUG (barber.barberId)
                            key={barber.barberId} 
                            className={`${styles.item} ${selectedBarber === barber.barberId ? styles.selected : ''}`}
                            onClick={() => setSelectedBarber(barber.barberId)}
                        >
                            {barber.fullName}
                        </div>
                    ))}
                </div>
                {}
                <div className={styles.column}>
                    <h2>2. Escolha o(s) Serviço(s)</h2>
                    {services.map(service => (
                        <div 
                            key={service.id} 
                            className={`${styles.item} ${selectedServices.includes(service.id) ? styles.selected : ''}`}
                            onClick={() => handleServiceToggle(service.id)}
                        >
                            {service.name} - R$ {service.price} ({service.durationInMinutes} min)
                        </div>
                    ))}
                </div>
            </div>

            {}
            {selectedBarber && selectedServices.length > 0 && (
                <div className={styles.datePicker}>
                    <h2>3. Escolha a Data</h2>
                    {scheduleLoading ? <p>Carregando horários...</p> : (
                        <DatePicker
                            locale="pt-BR"
                            selected={selectedDate}
                            onChange={(date) => setSelectedDate(date)}
                            filterDate={isWeekdayAvailable} 
                            minDate={new Date()} 
                            dateFormat="dd/MM/yyyy"
                            placeholderText="Clique para selecionar uma data"
                            className={styles.datePickerInput}
                            inline 
                        />
                    )}
                </div>
            )}

            {slotsLoading && <p className={styles.loadingText}>Buscando horários...</p>}

            {availableSlots.length > 0 && (
                <div className={styles.slotsContainer}>
                    <h2>4. Horários Disponíveis</h2>
                    <div className={styles.slotsGrid}>
                        {availableSlots.map(slot => (
                            <button key={slot} className={styles.slotButton} onClick={() => handleBookAppointment(slot)}>
                                {slot.substring(0, 5)}
                            </button>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}

export default Agendamento;