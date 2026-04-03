namespace PriorAuthApi.DTOs
{
    public record PriorAuthSummaryDto(
    int Id,
        string Status,
        string Priority,
        string ServiceCode,
        string ServiceCodeDisplay,
        string PatientName,
        string PractitionerName,
        string Specialty,
        DateTime CreatedAt,
        DateTime? DeterminationDate
    );
}