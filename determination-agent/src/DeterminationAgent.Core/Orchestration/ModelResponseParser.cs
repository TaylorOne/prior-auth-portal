using System.Text.Json;
using DeterminationAgent.Contracts;

namespace DeterminationAgent.Core.Orchestration;

public record AssessedCriterion(
    string? CriterionId,
    string? Status,
    string? CitationClauseId,
    List<string>? EvidenceRefs,
    string? Rationale);

public record AssessmentPayload(List<AssessedCriterion>? Criteria, string? Confidence);

public static class ModelResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Extracts the first JSON object from the model output (models sometimes wrap
    /// JSON in prose or code fences despite instructions). Null when unparseable —
    /// the orchestrator treats that as "no findings", which the citation guard then
    /// converts to insufficient-evidence across the board. Fail closed, never crash.
    /// </summary>
    public static AssessmentPayload? TryParse(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start < 0 || end <= start) return null;

        try
        {
            return JsonSerializer.Deserialize<AssessmentPayload>(content[start..(end + 1)], JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static CriterionStatus? MapStatus(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        "met" => CriterionStatus.Met,
        "not-met" => CriterionStatus.NotMet,
        "insufficient-evidence" => CriterionStatus.InsufficientEvidence,
        _ => null
    };

    public static ConfidenceLevel MapConfidence(string? confidence) => confidence?.Trim().ToLowerInvariant() switch
    {
        "high" => ConfidenceLevel.High,
        "low" => ConfidenceLevel.Low,
        _ => ConfidenceLevel.Medium
    };
}
