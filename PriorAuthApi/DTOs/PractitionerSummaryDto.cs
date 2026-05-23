namespace PriorAuthApi.DTOs
{
    public record PractitionerSummaryDto(
        int Id,
        string FullName,
        string Npi,
        string Specialty
    );
}
