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
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

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

            modelBuilder.Entity<AuthRule>()
                .Property(a => a.Code)
                .HasMaxLength(50);

            modelBuilder.Entity<AuthRule>()
                .Property(a => a.IndicationCode)
                .HasMaxLength(50);

            modelBuilder.Entity<AuthRule>()
                .HasIndex(a => new { a.Code, a.IndicationCode })
                .IsUnique();

            modelBuilder.Entity<AuditEvent>()
                .HasOne(a => a.Request)
                .WithMany()
                .HasForeignKey(a => a.PriorAuthRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Practitioner>()
                .HasIndex(p => p.EntraOid)
                .IsUnique()
                .HasFilter("[EntraOid] IS NOT NULL");

            // The dispatcher polls for unprocessed rows; a filtered index keeps that
            // scan cheap as delivered rows accumulate.
            modelBuilder.Entity<OutboxMessage>()
                .HasIndex(m => m.ProcessedAt)
                .HasFilter("[ProcessedAt] IS NULL");

            modelBuilder.Entity<OutboxMessage>()
                .Property(m => m.MessageType)
                .HasMaxLength(200);
        }
    }
}