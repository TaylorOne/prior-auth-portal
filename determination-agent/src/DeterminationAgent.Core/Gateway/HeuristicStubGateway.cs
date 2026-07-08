using System.Text.Json;

namespace DeterminationAgent.Core.Gateway;

/// <summary>
/// A deterministic, offline "model": keyword overlap + negation + numeric-threshold
/// heuristics over the prompt's CLAUSE/EVIDENCE sections, answering in exactly the
/// JSON shape the real models are asked for.
///
/// Purpose: (1) the whole pipeline — prompt → parse → citation guard → aggregation —
/// runs and is testable with zero cloud dependencies; (2) the eval harness gets an
/// honest, imperfect baseline that real models must beat. It is intentionally not
/// clever; do not "fix" its mistakes — beat them with a better model.
/// </summary>
public class HeuristicStubGateway : IChatGateway
{
    private const string ModelName = "stub-heuristic-v1";

    private static readonly string[] NegationMarkers =
        ["no ", "not ", "denies", "without", "never", "declined", "none "];

    private static readonly string[] ThresholdMarkers =
        ["at least", "minimum", "or more", "no less than", "or greater"];

    private static readonly HashSet<string> Stopwords =
    [
        "least", "minimum", "within", "should", "would", "there", "their",
        "these", "those", "member", "patient", "documented", "documentation"
    ];

    public Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken ct = default)
    {
        var prompt = request.Messages.LastOrDefault(m => m.Role == "user")?.Content ?? string.Empty;
        var (clauses, evidence) = ParsePrompt(prompt);

        var criteria = new List<object>();
        var decisiveCount = 0;
        var zeroOverlapCount = 0;

        foreach (var clause in clauses)
        {
            var clauseTokens = Tokenize(clause.Text + " " + clause.Title);
            var best = evidence
                .Select(e => (Item: e, Overlap: Tokenize(e.Text).Intersect(clauseTokens).Count()))
                .OrderByDescending(x => x.Overlap)
                .FirstOrDefault();

            string status;
            string? citation = null;
            List<string> refs = [];
            string rationale;

            if (best.Item is null || best.Overlap == 0)
            {
                status = "insufficient-evidence";
                zeroOverlapCount++;
                rationale = "No submitted evidence addresses this clause.";
            }
            else if (best.Overlap >= 2 && IsNegated(best.Item.Text))
            {
                status = "not-met";
                citation = clause.Id;
                refs = [best.Item.Ref];
                decisiveCount++;
                rationale = $"Evidence {best.Item.Ref} indicates the requirement is not satisfied.";
            }
            else if (best.Overlap >= 3)
            {
                var threshold = EvaluateThreshold(clause.Text, best.Item.Text);
                if (threshold == false)
                {
                    status = "not-met";
                    rationale = $"Evidence {best.Item.Ref} reports a value below the clause threshold.";
                }
                else
                {
                    status = "met";
                    rationale = $"Evidence {best.Item.Ref} substantiates this clause.";
                }
                citation = clause.Id;
                refs = [best.Item.Ref];
                decisiveCount++;
            }
            else
            {
                status = "insufficient-evidence";
                refs = [best.Item.Ref];
                rationale = $"Evidence {best.Item.Ref} touches on this clause but is not conclusive.";
            }

            criteria.Add(new
            {
                criterionId = clause.Id,
                status,
                citationClauseId = citation,
                evidenceRefs = refs,
                rationale
            });
        }

        var confidence = decisiveCount == clauses.Count && clauses.Count > 0 ? "high"
            : zeroOverlapCount > 0 ? "low"
            : "medium";

        var content = JsonSerializer.Serialize(new { criteria, confidence });
        return Task.FromResult(new ChatResponse(content, ModelName, prompt.Length / 4, content.Length / 4, 0));
    }

    private static bool IsNegated(string text)
    {
        var lower = " " + text.ToLowerInvariant();
        return NegationMarkers.Any(lower.Contains);
    }

    /// <summary>True/false when a numeric threshold comparison applies; null when it doesn't.</summary>
    private static bool? EvaluateThreshold(string clauseText, string evidenceText)
    {
        var lower = clauseText.ToLowerInvariant();
        if (!ThresholdMarkers.Any(lower.Contains)) return null;

        var clauseNumbers = ExtractNumbers(clauseText);
        var evidenceNumbers = ExtractNumbers(evidenceText);
        if (clauseNumbers.Count == 0 || evidenceNumbers.Count == 0) return null;

        return evidenceNumbers.Max() >= clauseNumbers.Min();
    }

    private static List<int> ExtractNumbers(string text)
    {
        var numbers = new List<int>();
        var current = 0;
        var inNumber = false;

        foreach (var c in text)
        {
            if (char.IsAsciiDigit(c))
            {
                current = current * 10 + (c - '0');
                inNumber = true;
            }
            else if (inNumber)
            {
                numbers.Add(current);
                current = 0;
                inNumber = false;
            }
        }

        if (inNumber) numbers.Add(current);
        return numbers;
    }

    private static HashSet<string> Tokenize(string text) =>
        text.ToLowerInvariant()
            .Split(' ', '\n', '\t', '.', ',', ';', ':', '(', ')', '/', '-', '"', '\'')
            .Where(t => t.Length >= 5 && !Stopwords.Contains(t))
            .ToHashSet();

    private static (List<StubClause> Clauses, List<StubEvidence> Evidence) ParsePrompt(string prompt)
    {
        var clauses = new List<StubClause>();
        var evidence = new List<StubEvidence>();

        string? sectionKind = null;
        string? id = null, title = null, kind = null;
        var body = new List<string>();

        void Flush()
        {
            var text = string.Join('\n', body).Trim();
            if (sectionKind == "clause" && id is not null) clauses.Add(new StubClause(id, title ?? "", text));
            if (sectionKind == "evidence" && id is not null) evidence.Add(new StubEvidence(id, kind ?? "", text));
            body.Clear();
            sectionKind = null;
            id = null;
            title = null;
            kind = null;
        }

        foreach (var line in prompt.Replace("\r\n", "\n").Split('\n'))
        {
            if (line.StartsWith("### CLAUSE ", StringComparison.Ordinal))
            {
                Flush();
                var heading = line["### CLAUSE ".Length..];
                var idx = heading.IndexOf(':');
                sectionKind = "clause";
                id = idx > 0 ? heading[..idx].Trim() : heading.Trim();
                title = idx > 0 ? heading[(idx + 1)..].Trim() : "";
            }
            else if (line.StartsWith("### EVIDENCE ", StringComparison.Ordinal))
            {
                Flush();
                var heading = line["### EVIDENCE ".Length..].TrimEnd(':');
                var parenStart = heading.IndexOf('(');
                sectionKind = "evidence";
                id = parenStart > 0 ? heading[..parenStart].Trim() : heading.Trim();
                kind = parenStart > 0 ? heading[(parenStart + 1)..].TrimEnd(')').Trim() : "";
            }
            else if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                Flush(); // END CLAUSES / END EVIDENCE / POLICY header
            }
            else if (sectionKind is not null)
            {
                body.Add(line);
            }
        }

        Flush();
        return (clauses, evidence);
    }

    private sealed record StubClause(string Id, string Title, string Text);
    private sealed record StubEvidence(string Ref, string Kind, string Text);
}
