// src/pages/Agendamento/index.js

import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../../services/api';
import styles from './Agendamento.module.css';

// --- Imports para o DatePicker ---
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import { registerLocale } from  "react-datepicker";
import ptBR from 'date-fns/locale/pt-BR';
registerLocale('pt-BR', ptBR); // Registra o idioma português para o calendário

function Agendamento() {
    // 1. STATES para guardar os dados e seleções
    const [barbers, setBarbers] = useState([]);
    const [services, setServices] = useState([]);
    const [selectedBarber, setSelectedBarber] = useState(null);
    const [selectedServices, setSelectedServices] = useState([]);
    const [selectedDate, setSelectedDate] = useState(null); // Alterado para null para o DatePicker
    const [availableSlots, setAvailableSlots] = useState([]);
    const [barberSchedule, setBarberSchedule] = useState([]); // <-- State para os horários do barbeiro

    // States de controle da UI
    const [loading, setLoading] = useState(true);
    const [scheduleLoading, setScheduleLoading] = useState(false); // <-- Loading para os horários
    const [slotsLoading, setSlotsLoading] = useState(false);
    const [error, setError] = useState('');
    const navigate = useNavigate();

    // 2. BUSCA INICIAL: Carrega barbeiros e serviços quando a página abre
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

    // 3. NOVO useEffect: Busca o horário de trabalho sempre que um barbeiro é selecionado
    useEffect(() => {
        const fetchBarberSchedule = async () => {
            if (!selectedBarber) {
                setBarberSchedule([]);
                setSelectedDate(null); // Limpa a data selecionada ao trocar de barbeiro
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

    // 4. BUSCA DE HORÁRIOS: Roda sempre que uma seleção (barbeiro, serviço, data) muda
    useEffect(() => {
        const fetchAvailability = async () => {
            if (selectedBarber && selectedServices.length > 0 && selectedDate) {
                setSlotsLoading(true);
                setAvailableSlots([]);
                try {
                    const serviceIdsQuery = selectedServices.map(id => `serviceIds=${id}`).join('&');
                    const formattedDate = selectedDate.toISOString().split('T')[0]; // Formata a data para YYYY-MM-DD
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

    // 5. FUNÇÕES DE EVENTO
    const handleServiceToggle = (serviceId) => {
        setSelectedServices(prev => 
            prev.includes(serviceId) 
            ? prev.filter(id => id !== serviceId) // Desmarca
            : [...prev, serviceId] // Marca
        );
    };

    const handleBookAppointment = async (slot) => {
        const [hour, minute] = slot.split(':');
        const startDateTime = new Date(selectedDate);
        startDateTime.setUTCHours(hour, minute, 0, 0); // Define a hora em UTC

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

    // Função que o DatePicker usará para desabilitar os dias
    const isWeekdayAvailable = (date) => {
        if (!barberSchedule || barberSchedule.length === 0) {
            return false;
        }
        const day = date.getDay(); // 0=Domingo, 1=Segunda, etc.
        return barberSchedule.some(scheduleDay => scheduleDay.dayOfWeek === day);
    };

    if (loading) return <p className={styles.loadingText}>Carregando...</p>;
    if (error) return <p style={{ color: 'red' }}>{error}</p>;

    // 6. RENDERIZAÇÃO (JSX)
    return (
        <div className={styles.page}>
            <h1>Faça seu Agendamento</h1>
            
            <div className={styles.selectionContainer}>
                {/* Coluna de Barbeiros */}
                <div className={styles.column}>
                    <h2>1. Escolha o Barbeiro</h2>
                    {barbers.map(barber => (
                        <div 
                            key={barber.userId} 
                            className={`${styles.item} ${selectedBarber === barber.barberId ? styles.selected : ''}`}
                            onClick={() => setSelectedBarber(barber.barberId)}
                        >
                            {barber.fullName}
                        </div>
                    ))}
                </div>
                {/* Coluna de Serviços */}
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

            {/* Calendário e Horários (aparecem após as seleções) */}
            {selectedBarber && selectedServices.length > 0 && (
                <div className={styles.datePicker}>
                    <h2>3. Escolha a Data</h2>
                    {scheduleLoading ? <p>Carregando horários...</p> : (
                        <DatePicker
                            locale="pt-BR"
                            selected={selectedDate}
                            onChange={(date) => setSelectedDate(date)}
                            filterDate={isWeekdayAvailable} // <-- AQUI USAMOS O FILTRO
                            minDate={new Date()} // Não permite selecionar datas passadas
                            dateFormat="dd/MM/yyyy"
                            placeholderText="Clique para selecionar uma data"
                            className={styles.datePickerInput}
                            inline // Mostra o calendário diretamente na página
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