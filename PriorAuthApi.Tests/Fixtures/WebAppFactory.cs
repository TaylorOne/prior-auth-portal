using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Azure.Messaging.ServiceBus;
using Moq;
using PriorAuthApi.Data;
using PriorAuthApi.Entities;

namespace PriorAuthApi.Tests
{
    public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private static readonly string TestConnectionString =
            Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
            ?? "Server=(localdb)\\mssqllocaldb;Database=PriorAuthDb_Test;Trusted_Connection=True;TrustServerCertificate=True;";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove the dev DbContext registration
                var descriptor = services.SingleOrDefault(d => 
                    d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Register test DbContext pointing at test DB
                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(TestConnectionString));

                // Register a no-op mock sender so the endpoint can resolve it
                var mockSender = new Mock<ServiceBusSender>();
                services.AddSingleton(mockSender.Object);
            });
        }

        public async Task InitializeAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync();
            await SeedTestDataAsync(db);
        }

        private static async Task SeedTestDataAsync(AppDbContext db)
        {
            db.AuthRules.Add(new AuthRule
            {
                Code = "J0135",
                CodeSystem = "CPT",
                IndicationCode = "M06.9",
                DisplayName = "Adalimumab (Humira)",
                IndicationDisplayName = "Rheumatoid Arthritis",
                IsActive = true,
                FormDefinition = """
                {
                    "fields": [
                        { "name": "priorDMARDTrial", "type": "boolean", "validation": { "required": true } },
                        { "name": "dmardName", "type": "select", "validation": { "required": true, "allowedValues": ["Methotrexate", "Hydroxychloroquine", "Sulfasalazine", "Leflunomide"] } },
                        { "name": "dmardDurationWeeks", "type": "number", "validation": { "required": true, "min": 0, "max": 104, "integer": true } },
                        { "name": "notes", "type": "text", "validation": { "required": false, "maxLength": 1000 } }
                    ],
                    "medicationFields": []
                }
                """,
                RuleDefinition = """
                {
                    "priorDMARDRequired": true,
                    "minDMARDWeeks": 12
                }
                """,
            });

            var org = new Organization
            {
                Name = "Test Organization",
            };
            db.Organizations.Add(org);
            await db.SaveChangesAsync(); // save first so org gets its Id

            db.Patients.Add(new Patient
            {
                FirstName = "Test",
                LastName = "Patient",
                DateOfBirth = new DateOnly(1980, 1, 1)
            });

            db.Practitioners.Add(new Practitioner
            {
                FirstName = "Test",
                LastName = "Practitioner",
                OrganizationId = org.Id
            });

            await db.SaveChangesAsync();
            AuthRule Ar = await db.AuthRules.FirstAsync();
            Console.WriteLine("AuthRule fields: " + Ar.Code + " " + Ar.IndicationCode + " " + Ar.DisplayName);
        }

        public new async Task DisposeAsync()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureDeletedAsync();
        }
    }
}