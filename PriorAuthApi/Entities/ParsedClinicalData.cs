namespace PriorAuthApi.Entities
{
    public class ParsedClinicalData
    {
        public int Id { get; set; }
        public string? ClinicalJustification { get; set; }
        public string? DiagnosisCodes { get; set; }       // JSON array
        public string? PriorTreatments { get; set; }      // JSON array - key for step therapy
        public string? RelevantLabValues { get; set; }    // JSON array
        public string? Contraindications { get; set; }
        public DateTime ParsedAt { get; set; }
        public string RawNoteText { get; set; } = string.Empty;

        public int PriorAuthRequestId { get; set; }
        public PriorAuthRequest PriorAuthRequest { get; set; } = null!;
    }
}