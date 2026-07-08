using System.Text;
using DeterminationAgent.Contracts;
using DeterminationAgent.Core.Gateway;
using DeterminationAgent.Core.Cases;
using DeterminationAgent.Core.Orchestration;
using DeterminationAgent.Core.Policies;
using DeterminationAgent.Eval;

string? policiesDir = null, casesDir = null, gatewayUrl = null;
var reportPath = "eval-report.md";
var escalation = true;
double? minCriterionAccuracy = null;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--policies": policiesDir = args[++i]; break;
        case "--cases": casesDir = args[++i]; break;
        case "--report": reportPath = args[++i]; break;
        case "--gateway": gatewayUrl = args[++i]; break;
        case "--no-escalation": escalation = false; break;
        case "--min-criterion-accuracy": minCriterionAccuracy = double.Parse(args[++i]); break;
        default:
            Console.Error.WriteLine($"Unknown argument '{args[i]}'.");
            return 2;
    }
}

policiesDir ??= FindDirectory(Path.Combine("eval", "data", "policies"));
casesDir ??= FindDirectory(Path.Combine("eval", "data", "cases"));

if (policiesDir is null || casesDir is null)
{
    Console.Error.WriteLine("Could not locate eval/data/policies and eval/data/cases; pass --policies and --cases explicitly.");
    return 2;
}

var policyStore = MarkdownPolicyStore.LoadFromDirectory(policiesDir);
var cases = EvalCaseLoader.LoadFromDirectory(casesDir);

IChatGateway gateway = gatewayUrl is null
    ? new HeuristicStubGateway()
    : new HttpChatGateway(new HttpClient { BaseAddress = new Uri(gatewayUrl.TrimEnd('/') + "/") });

var caseClient = new InMemoryCaseClient(cases.Select(c => c.ToAuthorizationCase()));
var orchestrator = new DeterminationOrchestrator(
    policyStore, caseClient, gateway, new DeterminationOptions { EscalationEnabled = escalation });

var matrix = new ConfusionMatrix();
var results = new List<CaseResult>();

foreach (var evalCase in cases)
{
    var outcome = await orchestrator.EvaluateAsync(evalCase.Request.RequestId);
    results.Add(Scorer.Score(evalCase, outcome, matrix));
}

var report = BuildReport(results, matrix, gatewayUrl ?? "in-process stub (stub-heuristic-v1)");
await File.WriteAllTextAsync(reportPath, report);
Console.WriteLine(report);
Console.WriteLine($"Report written to {Path.GetFullPath(reportPath)}");

var criterionAccuracy = matrix.Total == 0 ? 0 : (double)matrix.Correct / matrix.Total;
if (minCriterionAccuracy is not null && criterionAccuracy < minCriterionAccuracy)
{
    Console.Error.WriteLine(
        $"FAIL: criterion accuracy {criterionAccuracy:P1} is below the required {minCriterionAccuracy:P1}.");
    return 1;
}

return 0;

static string? FindDirectory(string relative)
{
    var dir = Directory.GetCurrentDirectory();
    while (dir is not null)
    {
        var candidate = Path.Combine(dir, relative);
        if (Directory.Exists(candidate)) return candidate;
        dir = Path.GetDirectoryName(dir);
    }
    return null;
}

static string BuildReport(List<CaseResult> results, ConfusionMatrix matrix, string modelSource)
{
    static string Pct(double value) => double.IsNaN(value) ? "n/a" : value.ToString("P1");

    var sb = new StringBuilder();
    sb.AppendLine("# Determination Agent — Eval Report");
    sb.AppendLine();
    sb.AppendLine($"- Generated: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm} UTC");
    sb.AppendLine($"- Model source: {modelSource}");
    sb.AppendLine($"- Cases: {results.Count}");
    sb.AppendLine();

    var recommendationCorrect = results.Count(r => r.RecommendationCorrect);
    var criterionAccuracy = matrix.Total == 0 ? double.NaN : (double)matrix.Correct / matrix.Total;

    sb.AppendLine("## Summary");
    sb.AppendLine();
    sb.AppendLine("| Metric | Value |");
    sb.AppendLine("|---|---|");
    sb.AppendLine($"| Recommendation accuracy | {recommendationCorrect}/{results.Count} ({Pct((double)recommendationCorrect / results.Count)}) |");
    sb.AppendLine($"| Per-criterion accuracy | {matrix.Correct}/{matrix.Total} ({Pct(criterionAccuracy)}) |");
    sb.AppendLine($"| Precision on not-met (unjustified denials) | {Pct(matrix.Precision(CriterionStatus.NotMet))} |");
    sb.AppendLine($"| Recall on not-met | {Pct(matrix.Recall(CriterionStatus.NotMet))} |");
    sb.AppendLine($"| Precision on insufficient-evidence | {Pct(matrix.Precision(CriterionStatus.InsufficientEvidence))} |");
    sb.AppendLine($"| Gap-detection recall (insufficient-evidence) | {Pct(matrix.Recall(CriterionStatus.InsufficientEvidence))} |");
    sb.AppendLine($"| Hallucinated criteria (dropped by guard) | {results.Sum(r => r.HallucinatedCriteria)} |");
    sb.AppendLine($"| Citation downgrades (guard-enforced) | {results.Sum(r => r.CitationDowngrades)} |");
    sb.AppendLine($"| Escalation rate | {results.Count(r => r.Escalated)}/{results.Count} |");
    sb.AppendLine();

    sb.AppendLine("## Per-case results");
    sb.AppendLine();
    sb.AppendLine("| Case | Tags | Recommendation | Criteria | Notes |");
    sb.AppendLine("|---|---|---|---|---|");

    foreach (var r in results)
    {
        var rec = r.RecommendationCorrect
            ? $"OK ({r.ActualRecommendation})"
            : $"WRONG (expected {r.ExpectedRecommendation}, got {r.ActualRecommendation})";
        var notes = r.Mismatches.Count == 0 ? "" : string.Join("; ", r.Mismatches);
        sb.AppendLine($"| {r.CaseId} | {string.Join(", ", r.Tags)} | {rec} | {r.CriteriaCorrect}/{r.CriteriaTotal} | {notes} |");
    }

    return sb.ToString();
}
