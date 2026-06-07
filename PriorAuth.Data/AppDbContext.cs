using Microsoft.EntityFrameworkCore;
using PriorAuth.Data.Entities;

namespace PriorAuth.Data
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
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Gender)
                .HasConversion<string>();

            modelBuilder.Entity<PriorAuthRequest>()
                .Property(p => p.Status)
                .HasConversion<string>();

            modelBuilder.Entity<PriorAuthRequest>()
                .HasOne(r => r.AuthRule)
                .WithMany()
                .HasForeignKey(r => r.AuthRuleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuthRule>()
                .Property(a => a.RequiresManualReview)
                .HasDefaultValue(false);

            modelBuilder.Entity<AuditEvent>()
                .HasOne(a => a.Request)
                .WithMany()
                .HasForeignKey(a => a.PriorAuthRequestId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}