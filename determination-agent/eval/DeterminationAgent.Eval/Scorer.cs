using System.Text.Json;
using DeterminationAgent.Contracts;
using DeterminationAgent.Core.Orchestration;

namespace DeterminationAgent.Eval;

public record CaseResult(
    string CaseId,
    IReadOnlyList<string> Tags,
    bool RecommendationCorrect,
    string ExpectedRecommendation,
    string ActualRecommendation,
    int CriteriaTotal,
    int CriteriaCorrect,
    IReadOnlyList<string> Mismatches,
    bool Escalated,
    int HallucinatedCriteria,
    int CitationDowngrades);

/// <summary>
/// Expected-vs-predicted counts per criterion status. Precision and recall on
/// NotMet and InsufficientEvidence are the headline metrics: wrongly asserting
/// "not met" is an unjustified denial, and missing an evidence gap is exactly
/// the mistake a reviewer relies on the agent to catch.
/// </summary>
public class ConfusionMatrix
{
    private readonly Dictionary<(CriterionStatus Expected, CriterionStatus Predicted), int> _counts = [];

    public void Add(CriterionStatus expected, CriterionStatus predicted)
    {
        _counts[(expected, predicted)] = _counts.GetValueOrDefault((expected, predicted)) + 1;
    }

    public int Total => _counts.Values.Sum();

    public int Correct => _counts.Where(kv => kv.Key.Expected == kv.Key.Predicted).Sum(kv => kv.Value);

    public double Precision(CriterionStatus status)
    {
        var predicted = _counts.Where(kv => kv.Key.Predicted == status).Sum(kv => kv.Value);
        return predicted == 0 ? double.NaN : (double)_counts.GetValueOrDefault((status, status)) / predicted;
    }

    public double Recall(CriterionStatus status)
    {
        var expected = _counts.Where(kv => kv.Key.Expected == status).Sum(kv => kv.Value);
        return expected == 0 ? double.NaN : (double)_counts.GetValueOrDefault((status, status)) / expected;
    }
}

public static class Scorer
{
    public static string ToKebab<T>(T value) where T : struct, Enum =>
        JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());

    public static CaseResult Score(EvalCase evalCase, DeterminationOutcome outcome, ConfusionMatrix matrix)
    {
        var draft = outcome.Draft;
        var mismatches = new List<string>();

        var predicted = draft.Criteria.ToDictionary(c => c.CriterionId, StringComparer.OrdinalIgnoreCase);
        var correct = 0;

        foreach (var expected in evalCase.Expected.Criteria)
        {
            var expectedStatus = ModelResponseParser.MapStatus(expected.Status)
                ?? throw new InvalidDataException(
                    $"Case '{evalCase.CaseId}' has an invalid expected status '{expected.Status}' for {expected.CriterionId}.");

            var actualStatus = predicted.TryGetValue(expected.CriterionId, out var finding)
                ? finding.Status
                : CriterionStatus.InsufficientEvidence;

            matrix.Add(expectedStatus, actualStatus);

            if (actualStatus == expectedStatus)
            {
                correct++;
            }
            else
            {
                mismatches.Add($"{expected.CriterionId}: expected {ToKebab(expectedStatus)}, got {ToKebab(actualStatus)}");
            }
        }

        if (!string.Equals(evalCase.Expected.PolicyId ?? "", draft.PolicyId ?? "", StringComparison.OrdinalIgnoreCase))
        {
            mismatches.Add($"policy: expected {evalCase.Expected.PolicyId ?? "none"}, got {draft.PolicyId ?? "none"}");
        }

        var actualRecommendation = ToKebab(draft.Recommendation);

        return new CaseResult(
            evalCase.CaseId,
            evalCase.Tags ?? [],
            string.Equals(evalCase.Expected.Recommendation, actualRecommendation, StringComparison.OrdinalIgnoreCase),
            evalCase.Expected.Recommendation,
            actualRecommendation,
            evalCase.Expected.Criteria.Count,
            correct,
            mismatches,
            draft.ModelPath == ModelPath.Escalated,
            outcome.HallucinatedCriterionIds.Count,
            outcome.CitationDowngrades);
    }
}
