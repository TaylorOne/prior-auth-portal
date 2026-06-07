namespace PriorAuth.Contracts;

public record PriorAuthSubmittedMessage
{
    public int PriorAuthRequestId { get; init; }
}