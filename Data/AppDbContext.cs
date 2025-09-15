using barbearia.api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace barbearia.api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Service> Services { get; set; }
        public DbSet<Barber> Barbers { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>()
                .HasOne(a => a.Address)
                .WithOne(u => u.User)
                .HasForeignKey<Address>(a => a.ApplicationUserId);

        }

    }
}
