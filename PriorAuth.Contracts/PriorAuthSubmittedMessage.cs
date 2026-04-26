namespace PriorAuth.Contracts;

public record PriorAuthSubmittedMessage
{
    public int PriorAuthRequestId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
}