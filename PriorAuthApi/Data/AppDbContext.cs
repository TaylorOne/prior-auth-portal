using Microsoft.EntityFrameworkCore;
using PriorAuthApi.Entities;

namespace PriorAuthApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Practitioner> Practitioners { get; set; }
        public DbSet<PriorAuthRequest> PriorAuthRequests { get; set; }
        public DbSet<AuthRule> AuthRules { get; set; }
        public DbSet<MedicationRequest> MedicationRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Gender)
                .HasConversion<string>();

            modelBuilder.Entity<PriorAuthRequest>()
                .Property(p => p.Status)
                .HasConversion<string>();
        }
    }
}