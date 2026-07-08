namespace DeterminationAgent.Core.Policies;

/// <summary>
/// Loads clause-structured policy documents from markdown files. Format:
///
///   # POLICY: DH-RAD-73721-KNEE
///   payer: DemoHealth
///   service: 73721
///   indication: M25.561
///   title: MRI Knee Without Contrast — Medical Necessity
///
///   ## C1: Conservative therapy completed
///   clause body...
///
/// Clause ids double as criterion ids, which is what makes citations checkable.
/// </summary>
public class MarkdownPolicyStore : IPolicyStore
{
    private readonly List<PolicyDocument> _policies;

    public MarkdownPolicyStore(IEnumerable<PolicyDocument> policies) => _policies = policies.ToList();

    public IReadOnlyList<PolicyDocument> All => _policies;

    public Task<PolicyDocument?> ResolveAsync(string serviceCode, string indicationCode, CancellationToken ct = default) =>
        Task.FromResult(_policies.FirstOrDefault(p =>
            string.Equals(p.ServiceCode, serviceCode, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(p.IndicationCode, indicationCode, StringComparison.OrdinalIgnoreCase)));

    public static MarkdownPolicyStore LoadFromDirectory(string directory)
    {
        var docs = Directory.EnumerateFiles(directory, "*.md")
            .OrderBy(f => f, StringComparer.Ordinal)
            .Select(f => Parse(File.ReadAllText(f), Path.GetFileName(f)))
            .ToList();
        return new MarkdownPolicyStore(docs);
    }

    public static PolicyDocument Parse(string markdown, string sourceName = "<memory>")
    {
        string? policyId = null;
        string payer = "", service = "", indication = "", title = "";
        var clauses = new List<PolicyClause>();
        string? clauseId = null, clauseTitle = null;
        var body = new List<string>();

        void FlushClause()
        {
            if (clauseId is not null)
            {
                clauses.Add(new PolicyClause(clauseId, clauseTitle ?? "", string.Join('\n', body).Trim()));
            }
            body.Clear();
        }

        foreach (var raw in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();

            if (line.StartsWith("# POLICY:", StringComparison.OrdinalIgnoreCase))
            {
                policyId = line["# POLICY:".Length..].Trim();
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                FlushClause();
                var heading = line[3..];
                var idx = heading.IndexOf(':');
                clauseId = idx > 0 ? heading[..idx].Trim() : heading.Trim();
                clauseTitle = idx > 0 ? heading[(idx + 1)..].Trim() : "";
            }
            else if (clauseId is null && line.Contains(':'))
            {
                var idx = line.IndexOf(':');
                var key = line[..idx].Trim().ToLowerInvariant();
                var value = line[(idx + 1)..].Trim();
                switch (key)
                {
                    case "payer": payer = value; break;
                    case "service": service = value; break;
                    case "indication": indication = value; break;
                    case "title": title = value; break;
                }
            }
            else if (clauseId is not null)
            {
                body.Add(raw);
            }
        }

        FlushClause();

        if (policyId is null)
        {
            throw new InvalidDataException($"'{sourceName}' is missing a '# POLICY: <id>' header.");
        }

        return new PolicyDocument(policyId, payer, service, indication, title, clauses);
    }
}
