namespace PriorAuthApi.Entities
{
    public class Patient
    {
        public int Id { get; set; }
        public string FirstName { get; set; }  = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public string MemberId { get; set; } = string.Empty;     // Insurance member ID
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }

    public enum Gender
    {
        Male,
        Female,
        Other,
        Unknown
    }
}