namespace PriorAuthApi.DTOs
{
    public record PractitionerSummaryDto(
        int Id,
        string FullName,
        string Npi,
        PriorAuth.Data.Entities.Specialty Specialty
    );
}
