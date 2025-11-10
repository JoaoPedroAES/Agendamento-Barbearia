using Xunit;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using barbearia.api.Data;
using barbearia.api.Models;
using barbearia.api.Services;
using barbearia.api.Dtos;
using System;
using System.Collections.Generic;

namespace barbearia.api.tests
{
    public class AvailabilityServiceTests
    {
        private readonly AppDbContext _context;
        private readonly AvailabilityService _service;
        
        // --- IDs Constantes para Teste ---
        private const int BARBER_ID = 1;
        private const int SERVICE_30_MIN_ID = 1;
        private const int SERVICE_15_MIN_ID = 2;
        private const string CUSTOMER_ID = "customer1";

        // --- SETUP (Construtor) ---
        public AvailabilityServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);

            // Instancia o serviço real que queremos testar
            _service = new AvailabilityService(_context);
        }

        // --- MÉTODO HELPER (Auxiliar) ---
        private async Task SeedDatabaseAsync(DayOfWeek dia, string startTime, string endTime, string breakStart, string breakEnd, Appointment appointment = null)
        {
            // 1. Criar Barbeiro e Usuário
            var user = new ApplicationUser { Id = "barberUser1", FullName = "Barbeiro Teste", Email = "barber@test.com", UserName = "barber@test.com" };
            var barber = new Barber { Id = BARBER_ID, ApplicationUserId = "barberUser1", UserAccount = user, IsActive = true };
            _context.Users.Add(user);
            _context.Barbers.Add(barber);

            // 2. Criar Serviços (COM A CORREÇÃO)
            var service30min = new Service { Id = SERVICE_30_MIN_ID, Name = "Corte", Description = "Desc Teste 1", DurationInMinutes = 30, Price = 50 };
            var service15min = new Service { Id = SERVICE_15_MIN_ID, Name = "Sobrancelha", Description = "Desc Teste 2", DurationInMinutes = 15, Price = 20 };
            _context.Services.AddRange(service30min, service15min);
            
            // 3. Criar Horário de Trabalho
            var schedule = new WorkSchedule
            {
                BarberId = BARBER_ID,
                DayOfWeek = dia, 
                StartTime = TimeSpan.Parse(startTime), 
                EndTime = TimeSpan.Parse(endTime),     
                BreakStartTime = TimeSpan.Parse(breakStart), 
                BreakEndTime = TimeSpan.Parse(breakEnd)      
            };
            _context.WorkSchedules.Add(schedule);

            // 4. (Opcional) Adicionar um agendamento existente
            if (appointment != null)
            {
                // Adiciona o cliente do agendamento ao banco também
                if (await _context.Users.FindAsync(CUSTOMER_ID) == null)
                {
                     _context.Users.Add(new ApplicationUser { Id = CUSTOMER_ID, FullName = "Cliente Teste", Email = "cliente@test.com", UserName = "cliente@test.com" });
                }
                _context.Appointments.Add(appointment);
            }
            
            await _context.SaveChangesAsync(); 
        }

        // --- TESTE 1: Cenário Feliz (Slots básicos) ---
        [Fact]
        public async Task GetAvailableSlotsAsync_DeveRetornarSlotsCorretos_EmDiaUtil()
        {
            // 1. Arrange
            var dataTeste = new DateTime(2025, 11, 10); // Uma Segunda-feira
            await SeedDatabaseAsync(DayOfWeek.Monday, "09:00", "11:00", "00:00", "00:00");
            var serviceIds = new List<int> { SERVICE_30_MIN_ID }; 

            // 2. Act
            var resultado = await _service.GetAvailableSlotsAsync(BARBER_ID, serviceIds, dataTeste);
            var slots = resultado.Select(ts => ts.ToString(@"hh\:mm")).ToList();

            // 3. Assert
            Assert.NotNull(resultado);
            // CORREÇÃO (Linha 91): Esperamos 7 slots (09:00, 09:15, 09:30, 09:45, 10:00, 10:15, 10:30)
            Assert.Equal(7, slots.Count); // <-- CORRIGIDO DE 4 PARA 7
            Assert.Contains("09:00", slots);
            Assert.Contains("10:30", slots);
            Assert.DoesNotContain("11:00", slots);
        }

        // --- TESTE 2: Deve Pular Horário de Pausa ---
        [Fact]
        public async Task GetAvailableSlotsAsync_DevePularHorarioDePausa()
        {
            // 1. Arrange
            var dataTeste = new DateTime(2025, 11, 11); // Uma Terça-feira
            await SeedDatabaseAsync(DayOfWeek.Tuesday, "11:00", "13:00", "12:00", "12:30");
            var serviceIds = new List<int> { SERVICE_30_MIN_ID }; 

            // 2. Act
            var resultado = await _service.GetAvailableSlotsAsync(BARBER_ID, serviceIds, dataTeste);
            var slots = resultado.Select(ts => ts.ToString(@"hh\:mm")).ToList();

            // 3. Assert
            Assert.NotNull(resultado);
            // CORREÇÃO (Linha 114): Esperamos 4 slots (11:00, 11:15, 11:30, 12:30)
            Assert.Equal(4, slots.Count); // <-- CORRIGIDO DE 3 PARA 4
            Assert.Contains("11:30", slots);
            Assert.DoesNotContain("12:00", slots); 
            Assert.Contains("12:30", slots);
        }

        // --- TESTE 3: Deve Pular Agendamento Existente ---
        [Fact]
        public async Task GetAvailableSlotsAsync_DevePularSlotsOcupadosPorAgendamentos()
        {
            // 1. Arrange
            var dataTeste = new DateTime(2025, 11, 12); // Uma Quarta-feira
            
            var appointmentExistente = new Appointment
            {
                Id = 1, 
                BarberId = BARBER_ID,
                CustomerId = CUSTOMER_ID,
                StartDateTime = new DateTime(2025, 11, 12, 10, 0, 0, DateTimeKind.Utc),
                EndDateTime = new DateTime(2025, 11, 12, 10, 30, 0, DateTimeKind.Utc),
                Status = AppointmentStatus.Scheduled
            };
            
            await SeedDatabaseAsync(DayOfWeek.Wednesday, "09:00", "11:00", "00:00", "00:00", appointmentExistente);
            var serviceIds = new List<int> { SERVICE_30_MIN_ID }; 

            // 2. Act
            var resultado = await _service.GetAvailableSlotsAsync(BARBER_ID, serviceIds, dataTeste);
            var slots = resultado.Select(ts => ts.ToString(@"hh\:mm")).ToList();

            // 3. Assert
            Assert.NotNull(resultado);
            // CORREÇÃO (Linha 150): Esperamos 4 slots (09:00, 09:15, 09:30, 10:30)
            Assert.Equal(4, slots.Count); // <-- CORRIGIDO DE 3 PARA 4
            Assert.Contains("09:30", slots);
            Assert.DoesNotContain("10:00", slots); 
            Assert.Contains("10:30", slots);
        }
    }
}