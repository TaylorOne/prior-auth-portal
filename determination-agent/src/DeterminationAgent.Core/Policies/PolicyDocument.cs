namespace DeterminationAgent.Core.Policies;

public record PolicyClause(string Id, string Title, string Text);

public record PolicyDocument(
    string PolicyId,
    string Payer,
    string ServiceCode,
    string IndicationCode,
    string Title,
    IReadOnlyList<PolicyClause> Clauses)
{
    public PolicyClause? FindClause(string id) =>
        Clauses.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));

    public int ClauseIndex(string id)
    {
        for (var i = 0; i < Clauses.Count; i++)
        {
            if (string.Equals(Clauses[i].Id, id, StringComparison.OrdinalIgnoreCase)) return i;
        }
        return int.MaxValue;
    }
}

/// <summary>
/// Policy resolution + retrieval. The local implementation is an exact-match lookup;
/// the Azure AI Search implementation (embeddings over chunked policy text) is a
/// build-plan milestone and slots in behind this same interface.
/// </summary>
public interface IPolicyStore
{
    Task<PolicyDocument?> ResolveAsync(string serviceCode, string indicationCode, CancellationToken ct = default);
    IReadOnlyList<PolicyDocument> All { get; }
}
