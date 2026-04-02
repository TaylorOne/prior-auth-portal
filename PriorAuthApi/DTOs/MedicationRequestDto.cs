namespace PriorAuthApi.DTOs
{
    public record MedicationRequestDto(
        
        // R! medication - CodeableReference; use RxNorm for clinical, HCPCS surfaced via ServiceRequest.code
        CodeableConceptDto Medication,

        // intent: always "order" for PA submissions
        string Intent,

        // Dosage narrative - keeping flat for the portal (no full Dosage[] complexity)
        string? DosageInstructionText,

        // Dispense basics
        decimal? QuantityValue,
        string? QuantityUnit,         // e.g., "mL", "tablet"
        int? NumberOfRepeatsAllowed,
        int? ExpectedSupplyDurationDays,

        // substitution - the step therapy signal
        bool? SubstitutionAllowed,
        string? SubstitutionReason,

        string? Note
    );
}