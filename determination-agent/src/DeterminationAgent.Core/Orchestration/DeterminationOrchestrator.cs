using DeterminationAgent.Contracts;
using DeterminationAgent.Core.Cases;
using DeterminationAgent.Core.Gateway;
using DeterminationAgent.Core.Policies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DeterminationAgent.Core.Orchestration;

public record DeterminationOptions
{
    /// <summary>When true, non-High-confidence triage results are re-run on the reasoning model.</summary>
    public bool EscalationEnabled { get; init; } = true;
}

/// <summary>Draft plus diagnostics the eval harness scores but reviewers never see.</summary>
public record DeterminationOutcome(
    DeterminationDraft Draft,
    IReadOnlyList<string> HallucinatedCriterionIds,
    int CitationDowngrades,
    ConfidenceLevel? TriageConfidence,
    string? ModelUsed);

public class DeterminationOrchestrator(
    IPolicyStore policies,
    IPriorAuthCaseClient cases,
    IChatGateway gateway,
    DeterminationOptions? options = null,
    ILogger<DeterminationOrchestrator>? logger = null)
{
    private readonly DeterminationOptions _options = options ?? new();
    private readonly ILogger<DeterminationOrchestrator> _logger =
        logger ?? NullLogger<DeterminationOrchestrator>.Instance;

    public async Task<DeterminationOutcome> EvaluateAsync(string requestId, CancellationToken ct = default)
    {
        // Step 3 of the ADR pipeline runs first mechanically: evidence comes from the
        // tool boundary (PriorAuth API in production, files in eval), never free text.
        var authCase = await cases.GetCaseAsync(requestId, ct)
            ?? throw new InvalidOperationException($"Case '{requestId}' was not found.");

        // Steps 1–2: resolve the governing policy. No policy → never guess; pend for a human.
        var policy = await policies.ResolveAsync(authCase.ServiceCode, authCase.IndicationCode, ct);
        if (policy is null)
        {
            _logger.LogWarning("No governing policy for service {Service} / indication {Indication}.",
                authCase.ServiceCode, authCase.IndicationCode);

            var pend = new DeterminationDraft(
                requestId, null, Recommendation.PendForInfo, [],
                [$"no governing policy found for service {authCase.ServiceCode} with indication {authCase.IndicationCode}"],
                ConfidenceLevel.Low, ModelPath.TriageOnly, DateTimeOffset.UtcNow);

            return new DeterminationOutcome(pend, [], 0, null, null);
        }

        // Step 4: criteria-vs-evidence reasoning, cheap model first.
        var triage = await AssessAsync(TaskClass.Triage, policy, authCase, ct);
        var final = triage;
        var path = ModelPath.TriageOnly;

        if (_options.EscalationEnabled && triage.Confidence != ConfidenceLevel.High)
        {
            _logger.LogInformation("Escalating request {RequestId} to the reasoning model (triage confidence: {Confidence}).",
                requestId, triage.Confidence);
            final = await AssessAsync(TaskClass.Reasoning, policy, authCase, ct);
            path = ModelPath.Escalated;
        }

        // Step 5: the recommendation is derived in code from the per-criterion findings —
        // the model assesses criteria; it does not get to pick the outcome directly.
        var recommendation = Aggregate(final.Findings);
        var gaps = final.Findings
            .Where(f => f.Status == CriterionStatus.InsufficientEvidence)
            .Select(f => $"missing or insufficient evidence for {f.CriterionId}: {policy.FindClause(f.CriterionId)?.Title}")
            .ToList();

        var draft = new DeterminationDraft(
            requestId, policy.PolicyId, recommendation, final.Findings, gaps,
            final.Confidence, path, DateTimeOffset.UtcNow);

        return new DeterminationOutcome(draft, final.Hallucinated, final.CitationDowngrades,
            triage.Confidence, final.Model);
    }

    public static Recommendation Aggregate(IReadOnlyList<CriterionFinding> findings) =>
        findings.Any(f => f.Status == CriterionStatus.NotMet) ? Recommendation.Deny
        : findings.Any(f => f.Status == CriterionStatus.InsufficientEvidence) ? Recommendation.PendForInfo
        : Recommendation.Approve;

    private sealed record Assessment(
        IReadOnlyList<CriterionFinding> Findings,
        ConfidenceLevel Confidence,
        IReadOnlyList<string> Hallucinated,
        int CitationDowngrades,
        string Model);

    private async Task<Assessment> AssessAsync(
        TaskClass taskClass, PolicyDocument policy, AuthorizationCase authCase, CancellationToken ct)
    {
        var request = new ChatRequest(taskClass,
        [
            new ChatMessage("system", PromptBuilder.SystemPrompt),
            new ChatMessage("user", PromptBuilder.BuildUserPrompt(policy, authCase))
        ]);

        var response = await gateway.CompleteAsync(request, ct);
        var payload = ModelResponseParser.TryParse(response.Content);

        if (payload is null)
        {
            _logger.LogWarning("Unparseable {TaskClass} response for policy {PolicyId}; failing closed.",
                taskClass, policy.PolicyId);
        }

        var findings = new List<CriterionFinding>();
        foreach (var criterion in payload?.Criteria ?? [])
        {
            var status = ModelResponseParser.MapStatus(criterion.Status);
            if (status is null || string.IsNullOrWhiteSpace(criterion.CriterionId)) continue;

            findings.Add(new CriterionFinding(
                criterion.CriterionId,
                status.Value,
                criterion.CitationClauseId is null ? null : new PolicyCitation(policy.PolicyId, criterion.CitationClauseId),
                criterion.EvidenceRefs ?? [],
                criterion.Rationale ?? string.Empty));
        }

        var guarded = CitationGuard.Enforce(findings, policy);
        var confidence = payload is null ? ConfidenceLevel.Low : ModelResponseParser.MapConfidence(payload.Confidence);

        return new Assessment(guarded.Findings, confidence, guarded.HallucinatedCriterionIds,
            guarded.CitationDowngrades, response.Model);
    }
}
