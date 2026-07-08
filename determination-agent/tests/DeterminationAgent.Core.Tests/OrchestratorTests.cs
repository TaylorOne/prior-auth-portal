using DeterminationAgent.Contracts;
using DeterminationAgent.Core.Cases;
using DeterminationAgent.Core.Gateway;
using DeterminationAgent.Core.Orchestration;
using DeterminationAgent.Core.Policies;

namespace DeterminationAgent.Core.Tests;

public class OrchestratorTests
{
    private static readonly PolicyDocument Policy = new(
        "TEST-POL-001", "DemoHealth", "12345", "X10.0", "Test Policy",
        [
            new PolicyClause("C1", "Conservative therapy completed",
                "The member must have completed conservative physical therapy including structured exercise treatment.")
        ]);

    private static DeterminationOrchestrator BuildOrchestrator(AuthorizationCase authCase) => new(
        new MarkdownPolicyStore([Policy]),
        new InMemoryCaseClient([authCase]),
        new HeuristicStubGateway());

    [Fact]
    public async Task StrongEvidence_YieldsApproveOnTriagePath()
    {
        var authCase = new AuthorizationCase("REQ-1", "DemoHealth", "12345", "X10.0",
            [new EvidenceItem("ev-1", "clinical-note",
                "Member completed conservative physical therapy with structured exercise treatment over recent months.")]);

        var outcome = await BuildOrchestrator(authCase).EvaluateAsync("REQ-1");

        Assert.Equal(Recommendation.Approve, outcome.Draft.Recommendation);
        Assert.Equal(ModelPath.TriageOnly, outcome.Draft.ModelPath);
        Assert.Equal("TEST-POL-001", outcome.Draft.PolicyId);
        var finding = Assert.Single(outcome.Draft.Criteria);
        Assert.Equal(CriterionStatus.Met, finding.Status);
        Assert.NotNull(finding.Citation);
        Assert.Empty(outcome.Draft.Gaps);
    }

    [Fact]
    public async Task IrrelevantEvidence_YieldsPendWithGapAndEscalation()
    {
        var authCase = new AuthorizationCase("REQ-2", "DemoHealth", "12345", "X10.0",
            [new EvidenceItem("ev-1", "clinical-note", "Blood pressure normal today.")]);

        var outcome = await BuildOrchestrator(authCase).EvaluateAsync("REQ-2");

        Assert.Equal(Recommendation.PendForInfo, outcome.Draft.Recommendation);
        Assert.Equal(ModelPath.Escalated, outcome.Draft.ModelPath);
        var finding = Assert.Single(outcome.Draft.Criteria);
        Assert.Equal(CriterionStatus.InsufficientEvidence, finding.Status);
        Assert.Single(outcome.Draft.Gaps);
    }

    [Fact]
    public async Task NoGoverningPolicy_PendsForHumanInsteadOfGuessing()
    {
        var authCase = new AuthorizationCase("REQ-3", "DemoHealth", "99999", "Z00.0",
            [new EvidenceItem("ev-1", "clinical-note", "Completed conservative physical therapy.")]);

        var outcome = await BuildOrchestrator(authCase).EvaluateAsync("REQ-3");

        Assert.Equal(Recommendation.PendForInfo, outcome.Draft.Recommendation);
        Assert.Null(outcome.Draft.PolicyId);
        Assert.Empty(outcome.Draft.Criteria);
        Assert.Contains(outcome.Draft.Gaps, g => g.Contains("no governing policy"));
        Assert.Equal(ConfidenceLevel.Low, outcome.Draft.Confidence);
    }

    [Theory]
    [InlineData(CriterionStatus.Met, CriterionStatus.Met, Recommendation.Approve)]
    [InlineData(CriterionStatus.Met, CriterionStatus.NotMet, Recommendation.Deny)]
    [InlineData(CriterionStatus.Met, CriterionStatus.InsufficientEvidence, Recommendation.PendForInfo)]
    [InlineData(CriterionStatus.NotMet, CriterionStatus.InsufficientEvidence, Recommendation.Deny)]
    public void Aggregate_DerivesRecommendationFromFindings(
        CriterionStatus first, CriterionStatus second, Recommendation expected)
    {
        var findings = new List<CriterionFinding>
        {
            new("C1", first, null, [], ""),
            new("C2", second, null, [], "")
        };

        Assert.Equal(expected, DeterminationOrchestrator.Aggregate(findings));
    }
}
