using System.Text;
using System.Text.Json;
using DeterminationAgent.Core.Gateway;

namespace DeterminationAgent.Gateway;

public record ProviderResult(string Content, string Model, int? PromptTokens, int? CompletionTokens);

public interface IModelProvider
{
    Task<ProviderResult> CompleteAsync(string model, IReadOnlyList<ChatMessage> messages, CancellationToken ct);
}

/// <summary>Offline provider for local development and CI — wraps the Core heuristic stub.</summary>
public class StubModelProvider : IModelProvider
{
    private readonly HeuristicStubGateway _stub = new();

    public async Task<ProviderResult> CompleteAsync(string model, IReadOnlyList<ChatMessage> messages, CancellationToken ct)
    {
        var response = await _stub.CompleteAsync(new ChatRequest(TaskClass.Triage, messages), ct);
        return new ProviderResult(response.Content, response.Model, response.PromptTokens, response.CompletionTokens);
    }
}

/// <summary>
/// Azure OpenAI chat completions over raw REST. Requires Gateway:AzureOpenAI:Endpoint
/// and an api key (Gateway:AzureOpenAI:ApiKey or AZURE_OPENAI_API_KEY). Moving this to
/// managed identity (DefaultAzureCredential bearer token) is a build-plan milestone.
/// </summary>
public class AzureOpenAIProvider(HttpClient http, IConfiguration configuration) : IModelProvider
{
    public async Task<ProviderResult> CompleteAsync(string model, IReadOnlyList<ChatMessage> messages, CancellationToken ct)
    {
        var endpoint = configuration["Gateway:AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException("Gateway:AzureOpenAI:Endpoint is not configured.");
        var apiKey = configuration["Gateway:AzureOpenAI:ApiKey"]
            ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY")
            ?? throw new InvalidOperationException("Set Gateway:AzureOpenAI:ApiKey or AZURE_OPENAI_API_KEY.");

        var url = $"{endpoint.TrimEnd('/')}/openai/deployments/{model}/chat/completions?api-version=2024-10-21";
        var body = JsonSerializer.Serialize(new
        {
            messages = messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = 0
        });

        using var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("api-key", apiKey);

        using var response = await http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        var root = json.RootElement;
        var content = root.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

        int? promptTokens = null, completionTokens = null;
        if (root.TryGetProperty("usage", out var usage))
        {
            promptTokens = usage.GetProperty("prompt_tokens").GetInt32();
            completionTokens = usage.GetProperty("completion_tokens").GetInt32();
        }

        return new ProviderResult(content, model, promptTokens, completionTokens);
    }
}

public record RouteTarget(string Provider, string Model);

/// <summary>Maps a task class to a provider + concrete model per configuration.</summary>
public class ProviderRouter(IConfiguration configuration, StubModelProvider stub, AzureOpenAIProvider azureOpenAI)
{
    public (IModelProvider Provider, RouteTarget Target)? Resolve(string taskClass)
    {
        var key = taskClass.Trim().ToLowerInvariant();
        if (key is not ("triage" or "reasoning")) return null;

        var target = new RouteTarget(
            configuration[$"Gateway:Routes:{key}:Provider"] ?? "stub",
            configuration[$"Gateway:Routes:{key}:Model"] ?? "stub-heuristic-v1");

        IModelProvider provider = target.Provider.ToLowerInvariant() switch
        {
            "stub" => stub,
            "azure-openai" => azureOpenAI,
            _ => throw new InvalidOperationException($"Unknown provider '{target.Provider}' for task class '{key}'.")
        };

        return (provider, target);
    }
}

public record UsageRecord(
    DateTimeOffset Timestamp,
    string TaskClass,
    string Provider,
    string Model,
    int PromptChars,
    int? PromptTokens,
    int? CompletionTokens,
    long LatencyMs,
    bool Success);

/// <summary>
/// Central usage log — every request through the gateway lands here as one JSON line,
/// which is what makes cost tracking, audit, and offline eval of production traffic possible.
/// </summary>
public class UsageLog(IConfiguration configuration)
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly string _path = configuration["Gateway:UsageLogPath"] ?? "gateway-usage.jsonl";

    public async Task RecordAsync(UsageRecord record, CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await File.AppendAllTextAsync(_path, JsonSerializer.Serialize(record) + Environment.NewLine, ct);
        }
        finally
        {
            _gate.Release();
        }
    }
}
