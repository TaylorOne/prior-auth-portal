using System.Text.Json;
using DeterminationAgent.Core.Cases;

namespace DeterminationAgent.Eval;

public record EvalEvidence(string Ref, string Kind, string Text);

public record EvalRequest(
    string RequestId,
    string Payer,
    string ServiceCode,
    string IndicationCode,
    List<EvalEvidence> Evidence);

public record ExpectedCriterion(string CriterionId, string Status);

public record EvalExpected(string? PolicyId, string Recommendation, List<ExpectedCriterion> Criteria);

public record EvalCase(string CaseId, List<string>? Tags, EvalRequest Request, EvalExpected Expected)
{
    public AuthorizationCase ToAuthorizationCase() => new(
        Request.RequestId,
        Request.Payer,
        Request.ServiceCode,
        Request.IndicationCode,
        Request.Evidence.Select(e => new EvidenceItem(e.Ref, e.Kind, e.Text)).ToList());
}

public static class EvalCaseLoader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static List<EvalCase> LoadFromDirectory(string directory) =>
        Directory.EnumerateFiles(directory, "*.json")
            .OrderBy(f => f, StringComparer.Ordinal)
            .SelectMany(f => JsonSerializer.Deserialize<List<EvalCase>>(File.ReadAllText(f), Options)
                ?? throw new InvalidDataException($"Could not parse eval cases from '{f}'."))
            .ToList();
}
