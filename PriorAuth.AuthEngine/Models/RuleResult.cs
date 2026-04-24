namespace PriorAuth.AuthEngine.Models;

public record RuleResult
{
    public required string Field { get; init; }
    public bool Passed { get; init; }
    public string? FailureReason { get; init; }
}
