using Microsoft.EntityFrameworkCore;

namespace PriorAuthApi.Entities
{
    public class MedicationRequest
    {
        public int Id { get; set; }

        // Medication identity (RxNorm for clinical precision)
        public string MedicationCode { get; set; } = string.Empty;
        public string MedicationSystem { get; set; } = string.Empty;
        public string? MedicationDisplay { get; set; }

        // Step therapy signal — load-bearing for rules evaluation
        public bool? SubstitutionAllowed { get; set; }
        public string? SubstitutionReason { get; set; }

        // Dispense / dosage detail — reviewer context only
        public string? DosageInstructionText { get; set; }
        [Precision(18, 2)]
        public decimal? QuantityValue { get; set; }
        public string? QuantityUnit { get; set; }
        public int? NumberOfRepeatsAllowed { get; set; }
        public int? ExpectedSupplyDurationDays { get; set; }

        public string? Note { get; set; }

        // FK
        public int PriorAuthRequestId { get; set; }
        public PriorAuthRequest PriorAuthRequest { get; set; } = null!;
    }
}