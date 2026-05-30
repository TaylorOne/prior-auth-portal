namespace PriorAuth.Data.Entities
{
    public class AuthRule
    {
        public int Id { get; set; }
        public RequestType RequestType { get; set; }
        public required string CodeSystem { get; set; }  // e.g., "CPT", "HCPCS"
        public required string Code { get; set; }
        public required string IndicationCode { get; set; }  // ICD-10 code for the indication
        public required string DisplayName { get; set; }
        public string? IndicationDisplayName { get; set; }
        public required string FormDefinition { get; set; }       // JSON array
        public required string RuleDefinition { get; set; }       // JSON array
        public bool RequiresManualReview { get; set; }
        public bool IsActive { get; set; } = true;
        public DateOnly EffectiveDate { get; set; } = new DateOnly(2026, 1, 1);
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}