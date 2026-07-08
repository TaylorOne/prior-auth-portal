using DeterminationAgent.Contracts;
using DeterminationAgent.Core.Orchestration;
using DeterminationAgent.Core.Policies;

namespace DeterminationAgent.Core.Tests;

public class CitationGuardTests
{
    private static readonly PolicyDocument Policy = new(
        "TEST-POL-001", "DemoHealth", "12345", "X10.0", "Test Policy",
        [
            new PolicyClause("C1", "First", "First clause text."),
            new PolicyClause("C2", "Second", "Second clause text.")
        ]);

    [Fact]
    public void MetFindingWithoutValidCitation_IsDowngradedToInsufficient()
    {
        var findings = new[]
        {
            new CriterionFinding("C1", CriterionStatus.Met,
                new PolicyCitation("TEST-POL-001", "C2"), ["ev-1"], "cites the wrong clause"),
            new CriterionFinding("C2", CriterionStatus.NotMet, null, [], "no citation at all")
        };

        var result = CitationGuard.Enforce(findings, Policy);

        Assert.Equal(2, result.CitationDowngrades);
        Assert.All(result.Findings, f => Assert.Equal(CriterionStatus.InsufficientEvidence, f.Status));
        Assert.All(result.Findings, f => Assert.StartsWith("[citation-invalid]", f.Rationale));
    }

    [Fact]
    public void FindingForNonexistentClause_IsDroppedAndReported()
    {
        var findings = new[]
        {
            new CriterionFinding("C9", CriterionStatus.Met,
                new PolicyCitation("TEST-POL-001", "C9"), [], "hallucinated criterion")
        };

        var result = CitationGuard.Enforce(findings, Policy);

        Assert.Contains("C9", result.HallucinatedCriterionIds);
        Assert.DoesNotContain(result.Findings, f => f.CriterionId == "C9");
    }

    [Fact]
    public void UnassessedClauses_AreFilledAsInsufficient()
    {
        var findings = new[]
        {
            new CriterionFinding("C1", CriterionStatus.Met,
                new PolicyCitation("TEST-POL-001", "C1"), ["ev-1"], "valid")
        };

        var result = CitationGuard.Enforce(findings, Policy);

        Assert.Equal(2, result.Findings.Count);
        var filled = result.Findings.Single(f => f.CriterionId == "C2");
        Assert.Equal(CriterionStatus.InsufficientEvidence, filled.Status);
        Assert.StartsWith("[not-assessed]", filled.Rationale);
        Assert.Equal(0, result.CitationDowngrades);
    }

    [Fact]
    public void ValidMetFinding_PassesThroughUnchanged()
    {
        var findings = new[]
        {
            new CriterionFinding("C1", CriterionStatus.Met,
                new PolicyCitation("TEST-POL-001", "C1"), ["ev-1"], "valid"),
            new CriterionFinding("C2", CriterionStatus.NotMet,
                new PolicyCitation("TEST-POL-001", "C2"), ["ev-2"], "also valid")
        };

        var result = CitationGuard.Enforce(findings, Policy);

        Assert.Equal(0, result.CitationDowngrades);
        Assert.Empty(result.HallucinatedCriterionIds);
        Assert.Equal(CriterionStatus.Met, result.Findings[0].Status);
        Assert.Equal(CriterionStatus.NotMet, result.Findings[1].Status);
    }
}
