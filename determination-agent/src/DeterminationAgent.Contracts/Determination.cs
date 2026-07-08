using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeterminationAgent.Contracts;

public enum Recommendation { Approve, Deny, PendForInfo }

public enum CriterionStatus { Met, NotMet, InsufficientEvidence }

public enum ConfidenceLevel { High, Medium, Low }

public enum ModelPath { TriageOnly, Escalated }

/// <summary>Grounding reference: the specific policy clause a finding rests on.</summary>
public record PolicyCitation(string PolicyId, string ClauseId);

/// <summary>
/// One policy criterion assessed against the submitted evidence.
/// A Met/NotMet status is only valid with a citation — CitationGuard enforces this.
/// </summary>
public record CriterionFinding(
    string CriterionId,
    CriterionStatus Status,
    PolicyCitation? Citation,
    IReadOnlyList<string> EvidenceRefs,
    string Rationale);

/// <summary>
/// The agent's structured output. Always a draft: only a human reviewer's action
/// writes a binding determination.
/// </summary>
public record DeterminationDraft(
    string RequestId,
    string? PolicyId,
    Recommendation Recommendation,
    IReadOnlyList<CriterionFinding> Criteria,
    IReadOnlyList<string> Gaps,
    ConfidenceLevel Confidence,
    ModelPath ModelPath,
    DateTimeOffset GeneratedAt);

public static class DeterminationJson
{
    /// <summary>
    /// Wire format for the draft: camelCase properties, kebab-case enum values
    /// ("pend-for-info", "not-met", "insufficient-evidence", "triage-only").
    /// </summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) },
        WriteIndented = true
    };
}
