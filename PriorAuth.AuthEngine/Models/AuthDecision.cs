namespace PriorAuth.AuthEngine.Models;

public record AuthDecision
{
    public required AuthOutcome Outcome { get; init; }
    public List<RuleResult> RuleResults { get; init; } = [];

    public static AuthDecision From(List<RuleResult> results)
    {
        var outcome = results switch
        {
            _ when results.Any(r => !r.Passed && r.FailureReason == FailureReasons.MissingField) 
                => AuthOutcome.NeedsMoreInfo,
            _ when results.Any(r => !r.Passed) 
                => AuthOutcome.Denied,
            _ => AuthOutcome.Approved
        };

        return new AuthDecision
        {
            Outcome = outcome,
            RuleResults = results
        };
    }
}
