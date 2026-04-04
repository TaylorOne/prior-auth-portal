namespace PriorAuthApi.Entities
{
    public class AuthRule
    {
        public int Id { get; set; }
        public RequestType RequestType { get; set; }
        public required string CodeSystem { get; set; }  // e.g., "CPT", "HCPCS"
        public required string Code { get; set; }
        public string? IndicationCode { get; set; }  // ICD-10 code for the indication
        public required string DisplayName { get; set; }
        public string? IndicationDisplayName { get; set; }
        public required string FormDefinition { get; set; }       // JSON array
        public required string RuleDefinition { get; set; }       // JSON array
        public bool IsActive { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    }
}