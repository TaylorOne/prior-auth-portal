using Microsoft.EntityFrameworkCore;
using PriorAuthApi.Entities;

namespace PriorAuthApi.Data
{
    public static class PractitionerSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Practitioners.AnyAsync()) return;

            var orgs = await db.Organizations.ToListAsync();

            var valley      = orgs.First(o => o.Name.Contains("Orthopedic"));
            var keystone    = orgs.First(o => o.Name.Contains("Cancer"));
            var capital     = orgs.First(o => o.Name.Contains("Rheumatology"));
            var centralPa   = orgs.First(o => o.Name.Contains("Endocrinology"));
            var susquehanna = orgs.First(o => o.Name.Contains("Cardiology"));

            var practitioners = new List<Practitioner>
            {
                new()
                {
                    FirstName = "Michael",
                    LastName = "Torres",
                    Npi = "1427053801",
                    Specialty = "Orthopedic Surgery",
                    Phone = "717-555-0101",
                    FaxNumber = "717-555-0102",
                    Email = "mtorres@valleyortho.example.com",
                    UserId = "seed-practitioner-001",
                    OrganizationId = valley.Id
                },
                new()
                {
                    FirstName = "Sarah",
                    LastName = "Chen",
                    Npi = "1538164902",
                    Specialty = "Medical Oncology",
                    Phone = "717-555-0201",
                    FaxNumber = "717-555-0202",
                    Email = "schen@keystonecancer.example.com",
                    UserId = "seed-practitioner-002",
                    OrganizationId = keystone.Id
                },
                new()
                {
                    FirstName = "James",
                    LastName = "Patel",
                    Npi = "1649275003",
                    Specialty = "Rheumatology",
                    Phone = "717-555-0301",
                    FaxNumber = "717-555-0302",
                    Email = "jpatel@capitalrheum.example.com",
                    UserId = "seed-practitioner-003",
                    OrganizationId = capital.Id
                },
                new()
                {
                    FirstName = "Lisa",
                    LastName = "Nguyen",
                    Npi = "1750386104",
                    Specialty = "Endocrinology",
                    Phone = "717-555-0401",
                    FaxNumber = "717-555-0402",
                    Email = "lnguyen@centralpaendo.example.com",
                    UserId = "seed-practitioner-004",
                    OrganizationId = centralPa.Id
                },
                new()
                {
                    FirstName = "Robert",
                    LastName = "Harmon",
                    Npi = "1861497205",
                    Specialty = "Cardiology",
                    Phone = "717-555-0501",
                    FaxNumber = "717-555-0502",
                    Email = "rharmon@susquehannacard.example.com",
                    UserId = "seed-practitioner-005",
                    OrganizationId = susquehanna.Id
                }
            };

            await db.Practitioners.AddRangeAsync(practitioners);
            await db.SaveChangesAsync();
        }
    }
}