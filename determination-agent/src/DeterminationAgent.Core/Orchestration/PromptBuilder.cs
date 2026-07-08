using System.Text;
using DeterminationAgent.Core.Cases;
using DeterminationAgent.Core.Policies;

namespace DeterminationAgent.Core.Orchestration;

/// <summary>
/// Builds the assessment prompt. The section delimiters (### CLAUSE / ### EVIDENCE)
/// are a contract: the heuristic stub model parses them, and keeping the format stable
/// means prompt changes are deliberate, reviewable diffs.
/// </summary>
public static class PromptBuilder
{
    public const string SystemPrompt = """
        You are a prior-authorization determination assistant. You will be given the
        governing payer policy as a list of clauses, and the clinical evidence submitted
        with an authorization request. Assess EACH policy clause against the evidence.

        Respond with ONLY a JSON object of exactly this shape:
        {
          "criteria": [
            {
              "criterionId": "C1",
              "status": "met" | "not-met" | "insufficient-evidence",
              "citationClauseId": "C1",
              "evidenceRefs": ["ev-1"],
              "rationale": "one or two sentences explaining the finding"
            }
          ],
          "confidence": "high" | "medium" | "low"
        }

        Rules:
        - Assess every clause that appears between ### CLAUSE markers; never invent clauses.
        - A status of "met" or "not-met" REQUIRES citationClauseId referencing the clause it rests on.
        - If the evidence neither clearly confirms nor clearly refutes a clause, use
          "insufficient-evidence". Never guess.
        - evidenceRefs must only contain refs that appear between ### EVIDENCE markers.
        - Overall confidence is "high" only when every clause was decisively met or not-met.
        """;

    public static string BuildUserPrompt(PolicyDocument policy, AuthorizationCase authCase)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"### POLICY {policy.PolicyId}: {policy.Title}");
        sb.AppendLine();

        foreach (var clause in policy.Clauses)
        {
            sb.AppendLine($"### CLAUSE {clause.Id}: {clause.Title}");
            sb.AppendLine(clause.Text);
            sb.AppendLine();
        }

        sb.AppendLine("### END CLAUSES");
        sb.AppendLine();

        foreach (var item in authCase.Evidence)
        {
            sb.AppendLine($"### EVIDENCE {item.Ref} ({item.Kind}):");
            sb.AppendLine(item.Text);
            sb.AppendLine();
        }

        sb.AppendLine("### END EVIDENCE");
        return sb.ToString();
    }
}
