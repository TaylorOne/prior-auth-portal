namespace PriorAuthApi.Entities
{
    public class PriorAuthRequest
    {
        public int Id { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public RequestType RequestType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeterminationDate { get; set; }
        public Status Status { get; set; }
        public string? ClinicalJustification { get; set; }
        public string? DiagnosisCodes { get; set; }       // JSON array
        public string? PriorTreatments { get; set; }      // JSON array
        public string? RelevantLabValues { get; set; }    // JSON array
        public string? Contraindications { get; set; }
        public string? ReviewerNotes { get; set; }
        public string? EvaluationReason { get; set; }

        public int PatientId { get; set; }
        public Patient Patient { get; set; } = null!;
        public int PractitionerId { get; set; }
        public Practitioner Practitioner { get; set; } = null!;
    }

}