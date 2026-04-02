using Microsoft.EntityFrameworkCore;
using PriorAuthApi.Entities;

namespace PriorAuthApi.Data
{
    public static class PatientSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Patients.AnyAsync()) return;

            var patients = new List<Patient>
            {
                // MRI Knee — active middle-aged male, plausible sports/activity history
                new()
                {
                    FirstName = "Marcus",
                    LastName = "Webb",
                    DateOfBirth = new DateOnly(1979, 4, 12),
                    Gender = Gender.Male,
                    Phone = "717-555-1101",
                    Email = "mwebb@email.example.com",
                    AddressLine = "614 Locust Lane",
                    City = "Mechanicsburg",
                    State = "PA",
                    PostalCode = "17055",
                    UserId = "seed-patient-001"
                },

                // BRCA Genetic Testing — younger female, family history demographic
                new()
                {
                    FirstName = "Angela",
                    LastName = "Rossi",
                    DateOfBirth = new DateOnly(1988, 9, 23),
                    Gender = Gender.Female,
                    Phone = "717-555-1201",
                    Email = "arossi@email.example.com",
                    AddressLine = "2240 Fruitville Pike",
                    City = "Lancaster",
                    State = "PA",
                    PostalCode = "17601",
                    UserId = "seed-patient-002"
                },

                // Humira/RA — middle-aged female, RA skews 2:1 female
                new()
                {
                    FirstName = "Diane",
                    LastName = "Kowalski",
                    DateOfBirth = new DateOnly(1971, 1, 30),
                    Gender = Gender.Female,
                    Phone = "717-555-1301",
                    Email = "dkowalski@email.example.com",
                    AddressLine = "88 Market Street",
                    City = "Camp Hill",
                    State = "PA",
                    PostalCode = "17011",
                    UserId = "seed-patient-003"
                },

                // Wegovy/Obesity — middle-aged male, weight management demographic
                new()
                {
                    FirstName = "Calvin",
                    LastName = "Brooks",
                    DateOfBirth = new DateOnly(1975, 6, 8),
                    Gender = Gender.Male,
                    Phone = "717-555-1401",
                    Email = "cbrooks@email.example.com",
                    AddressLine = "319 Walnut Street",
                    City = "Harrisburg",
                    State = "PA",
                    PostalCode = "17101",
                    UserId = "seed-patient-004"
                },

                // Xarelto/AFib — older male, atrial fibrillation demographic
                new()
                {
                    FirstName = "Howard",
                    LastName = "Finley",
                    DateOfBirth = new DateOnly(1951, 11, 3),
                    Gender = Gender.Male,
                    Phone = "717-555-1501",
                    Email = "hfinley@email.example.com",
                    AddressLine = "47 Cumberland Road",
                    City = "Enola",
                    State = "PA",
                    PostalCode = "17025",
                    UserId = "seed-patient-005"
                }
            };

            await db.Patients.AddRangeAsync(patients);
            await db.SaveChangesAsync();
        }
    }
}