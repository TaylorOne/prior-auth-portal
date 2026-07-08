namespace DeterminationAgent.Core.Cases;

/// <summary>A piece of submitted evidence: a clinical-note excerpt, form field, lab value, or report.</summary>
public record EvidenceItem(string Ref, string Kind, string Text);

public record AuthorizationCase(
    string RequestId,
    string Payer,
    string ServiceCode,
    string IndicationCode,
    IReadOnlyList<EvidenceItem> Evidence);

/// <summary>
/// The agent's tool boundary into the PriorAuth portal. The production implementation
/// is an HTTP client authenticating with client credentials against the portal API
/// (build-plan milestone); eval and tests use the in-memory implementation.
/// </summary>
public interface IPriorAuthCaseClient
{
    Task<AuthorizationCase?> GetCaseAsync(string requestId, CancellationToken ct = default);
}

public class InMemoryCaseClient(IEnumerable<AuthorizationCase> cases) : IPriorAuthCaseClient
{
    private readonly Dictionary<string, AuthorizationCase> _cases =
        cases.ToDictionary(c => c.RequestId, StringComparer.OrdinalIgnoreCase);

    public Task<AuthorizationCase?> GetCaseAsync(string requestId, CancellationToken ct = default) =>
        Task.FromResult(_cases.GetValueOrDefault(requestId));
}
