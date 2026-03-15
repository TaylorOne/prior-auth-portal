namespace PriorAuthApi.Entities
{
    public class PriorAuthRequest
    {
        public int Id { get; set; }
        public string ClaimId { get; set; } = string.Empty;
        public string? ClaimResponseId { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public RequestType RequestType { get; set; }
        public DateTime RequestDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public DateTime? DeterminationDate { get; set; }
        public Status Status { get; set; }
        public string? ReviewerNotes { get; set; }

        // public int AuthRulesId { get; set; }
        // public AuthRules AuthRules { get; set; } = null!;
        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public int PractitionerId { get; set; }
        public Practitioner Practitioner { get; set; } = null!;
    }

    public enum Status
    {
        Draft,
        Pending,
        Approved,
        Denied,
        NeedsMoreInfo
    }

    public enum RequestType
    {
        Medication,
        Procedure
    }
}