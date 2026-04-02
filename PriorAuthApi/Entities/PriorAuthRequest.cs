namespace PriorAuthApi.Entities
{
    public class PriorAuthRequest
    {
        public int Id { get; set; }

        // ServiceRequest core
        public string ServiceCode { get; set; } = string.Empty;       // CPT or HCPCS code
        public string ServiceCodeSystem { get; set; } = string.Empty; // AMA or CMS system URI
        public string? ServiceCodeDisplay { get; set; }
        public string Priority { get; set; } = "routine";
        public Status Status { get; set; }

        // FormDefinition-keyed clinical responses — shape varies by AuthRule
        public string? ClinicalData { get; set; }

        // Reviewer output
        public string? ReviewerNotes { get; set; }
        public string? EvaluationReason { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeterminationDate { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public int PractitionerId { get; set; }
        public Practitioner Practitioner { get; set; } = null!;
        public int AuthRuleId { get; set; }
        public AuthRule AuthRule { get; set; } = null!;

        // Null for procedure/diagnostic PAs, populated for drug PAs
        public MedicationRequest? MedicationRequest { get; set; }
    }
}