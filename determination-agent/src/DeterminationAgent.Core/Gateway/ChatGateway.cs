using System.Net.Http.Json;

namespace DeterminationAgent.Core.Gateway;

/// <summary>Routing class, not a model name: the gateway maps this to a concrete deployment.</summary>
public enum TaskClass { Triage, Reasoning }

public record ChatMessage(string Role, string Content);

public record ChatRequest(TaskClass TaskClass, IReadOnlyList<ChatMessage> Messages);

public record ChatResponse(string Content, string Model, int? PromptTokens, int? CompletionTokens, long LatencyMs);

public interface IChatGateway
{
    Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken ct = default);
}

/// <summary>Calls the model gateway service over HTTP (BaseAddress must point at it).</summary>
public class HttpChatGateway(HttpClient http) : IChatGateway
{
    public async Task<ChatResponse> CompleteAsync(ChatRequest request, CancellationToken ct = default)
    {
        var response = await http.PostAsJsonAsync("v1/completions", new
        {
            taskClass = request.TaskClass.ToString().ToLowerInvariant(),
            messages = request.Messages.Select(m => new { role = m.Role, content = m.Content })
        }, ct);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CompletionBody>(cancellationToken: ct)
            ?? throw new InvalidOperationException("The gateway returned an empty response.");

        return new ChatResponse(body.Content, body.Model, body.PromptTokens, body.CompletionTokens, body.LatencyMs);
    }

    private sealed record CompletionBody(string Content, string Model, int? PromptTokens, int? CompletionTokens, long LatencyMs);
}
