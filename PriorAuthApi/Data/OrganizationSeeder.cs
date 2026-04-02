using Microsoft.EntityFrameworkCore;
using PriorAuthApi.Entities;

namespace PriorAuthApi.Data
{
    public static class OrganizationSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Organizations.AnyAsync()) return;

            var organizations = new List<Organization>
            {
                new()
                {
                    Name = "Valley Orthopedic & Sports Medicine",
                    Phone = "717-555-0101",
                    FaxNumber = "717-555-0102",
                    AddressLine = "1420 Cavalry Drive, Suite 200",
                    City = "Harrisburg",
                    State = "PA",
                    PostalCode = "17110"
                },
                new()
                {
                    Name = "Keystone Cancer & Genetics Center",
                    Phone = "717-555-0201",
                    FaxNumber = "717-555-0202",
                    AddressLine = "890 Penn Street, Suite 410",
                    City = "Lancaster",
                    State = "PA",
                    PostalCode = "17602"
                },
                new()
                {
                    Name = "Capital Rheumatology Associates",
                    Phone = "717-555-0301",
                    FaxNumber = "717-555-0302",
                    AddressLine = "3300 Trindle Road, Suite 115",
                    City = "Camp Hill",
                    State = "PA",
                    PostalCode = "17011"
                },
                new()
                {
                    Name = "Central PA Endocrinology & Metabolism",
                    Phone = "717-555-0401",
                    FaxNumber = "717-555-0402",
                    AddressLine = "205 Grandview Avenue, Suite 300",
                    City = "Camp Hill",
                    State = "PA",
                    PostalCode = "17011"
                },
                new()
                {
                    Name = "Susquehanna Cardiology Group",
                    Phone = "717-555-0501",
                    FaxNumber = "717-555-0502",
                    AddressLine = "2501 North Front Street, Suite 600",
                    City = "Harrisburg",
                    State = "PA",
                    PostalCode = "17110"
                }
            };

            await db.Organizations.AddRangeAsync(organizations);
            await db.SaveChangesAsync();
        }
    }
}