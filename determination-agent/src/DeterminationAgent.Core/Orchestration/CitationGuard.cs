using DeterminationAgent.Contracts;
using DeterminationAgent.Core.Policies;

namespace DeterminationAgent.Core.Orchestration;

public record GuardResult(
    IReadOnlyList<CriterionFinding> Findings,
    IReadOnlyList<string> HallucinatedCriterionIds,
    int CitationDowngrades);

/// <summary>
/// Enforces the grounding rule: no citation → not asserted. This guard is what makes
/// the draft auditable in a regulated setting, so it runs in code, after the model —
/// it is never delegated to the prompt.
/// </summary>
public static class CitationGuard
{
    public static GuardResult Enforce(IEnumerable<CriterionFinding> findings, PolicyDocument policy)
    {
        var kept = new List<CriterionFinding>();
        var hallucinated = new List<string>();
        var downgrades = 0;

        foreach (var finding in findings)
        {
            // A finding about a clause that does not exist is a hallucinated criterion:
            // drop it entirely and surface it as a diagnostic for the eval harness.
            if (policy.FindClause(finding.CriterionId) is null)
            {
                hallucinated.Add(finding.CriterionId);
                continue;
            }

            if (finding.Status is CriterionStatus.Met or CriterionStatus.NotMet)
            {
                var citationValid =
                    finding.Citation is not null &&
                    string.Equals(finding.Citation.PolicyId, policy.PolicyId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(finding.Citation.ClauseId, finding.CriterionId, StringComparison.OrdinalIgnoreCase) &&
                    policy.FindClause(finding.Citation.ClauseId) is not null;

                if (!citationValid)
                {
                    downgrades++;
                    kept.Add(finding with
                    {
                        Status = CriterionStatus.InsufficientEvidence,
                        Citation = null,
                        Rationale = $"[citation-invalid] {finding.Rationale}"
                    });
                    continue;
                }
            }

            kept.Add(finding);
        }

        // Every clause gets a finding: a clause the model skipped is unresolved
        // evidence, not silent approval.
        foreach (var clause in policy.Clauses)
        {
            if (!kept.Any(k => string.Equals(k.CriterionId, clause.Id, StringComparison.OrdinalIgnoreCase)))
            {
                kept.Add(new CriterionFinding(
                    clause.Id,
                    CriterionStatus.InsufficientEvidence,
                    null,
                    [],
                    "[not-assessed] The model did not return a finding for this clause."));
            }
        }

        var ordered = kept.OrderBy(k => policy.ClauseIndex(k.CriterionId)).ToList();
        return new GuardResult(ordered, hallucinated, downgrades);
    }
}
