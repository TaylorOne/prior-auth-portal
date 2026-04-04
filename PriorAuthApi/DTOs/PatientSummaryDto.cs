namespace PriorAuthApi.DTOs
{
    public record PatientSummaryDto(
        int Id,
        string FullName,
        int Age,
        string Gender
    );
}