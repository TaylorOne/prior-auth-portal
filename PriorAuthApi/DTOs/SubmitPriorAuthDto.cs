using System.Text.Json;

namespace PriorAuthApi.DTOs
{
    public record SubmitPriorAuthDto(
        string Priority,                              // "routine" | "urgent"
        CodeableConceptDto Code,                      // CPT or HCPCS
        int PatientId,
        int PractitionerId,
        IList<CodeableConceptDto> ReasonCode,         // ICD-10 indication(s)
        Dictionary<string, JsonElement>? ClinicalData, // FormDefinition-keyed responses
        MedicationRequestDto? MedicationRequest       // null for procedure/diagnostic PAs
    );
}