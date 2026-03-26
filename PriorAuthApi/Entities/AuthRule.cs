namespace PriorAuthApi.Entities
{
    public class AuthRule
    {
        public int Id { get; set; }
        public RequestType RequestType { get; set; }
        public string CodeSystem { get; set; }  // e.g., "CPT", "HCPCS"
        public string Code { get; set; }
        public string IndicationCode { get; set; }  // ICD-10 code for the indication
        public string DisplayName { get; set; }
        public string FormDefinition { get; set; }       // JSON array
        public string RuleDefinition { get; set; }       // JSON array
        public bool IsActive { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}