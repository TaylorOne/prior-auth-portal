namespace PriorAuthApi.Entities
{
    public class Practitioner
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Npi { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? FaxNumber { get; set; }

        public string Specialty { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        
        public string UserId { get; set; } = string.Empty;
    }
}