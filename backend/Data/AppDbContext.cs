using barbearia.api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace barbearia.api.Data
{
    // Classe que representa o contexto do banco de dados da aplicação
    // Herda de IdentityDbContext para incluir suporte ao Identity
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // Construtor que recebe as opções de configuração do DbContext
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSet para a tabela de serviços
        public DbSet<Service> Services { get; set; }

        // DbSet para a tabela de barbeiros
        public DbSet<Barber> Barbers { get; set; }

        // DbSet para a tabela de endereços
        public DbSet<Address> Addresses { get; set; }

        // DbSet para a tabela de horários de trabalho
        public DbSet<WorkSchedule> WorkSchedules { get; set; }

        // DbSet para a tabela de agendamentos
        public DbSet<Appointment> Appointments { get; set; }

        // Configurações adicionais do modelo
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuração de relacionamento 1:1 entre ApplicationUser e Address
            builder.Entity<ApplicationUser>()
                .HasOne(a => a.Address) // Um usuário tem um endereço
                .WithOne(u => u.User) // Um endereço pertence a um usuário
                .HasForeignKey<Address>(a => a.ApplicationUserId); // Chave estrangeira

            // Configuração para a tabela de serviços
            builder.Entity<Service>(entity =>
            {
                // Define que o ID do serviço é gerado automaticamente
                entity.Property(s => s.Id)
                      .ValueGeneratedOnAdd();
            });
        }
    }
}
