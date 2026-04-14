namespace PriorAuthApi.DTOs
{
    public record IndicationDto(
        string IndicationCode,
        string? IndicationDisplayName
    );
}