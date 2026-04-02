namespace PriorAuthApi.DTOs
{
    public record CodeableConceptDto(
        string System,   // e.g., "http://www.ama-assn.org/go/cpt"
        string Code,     // e.g., "73721"
        string? Display  // e.g., "MRI any joint of lower extremity"
    );
}