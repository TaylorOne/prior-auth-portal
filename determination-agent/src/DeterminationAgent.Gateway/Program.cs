using System.Diagnostics;
using DeterminationAgent.Core.Gateway;
using DeterminationAgent.Gateway;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<UsageLog>();
builder.Services.AddSingleton<StubModelProvider>();
builder.Services.AddHttpClient<AzureOpenAIProvider>();
builder.Services.AddSingleton<ProviderRouter>(sp => new ProviderRouter(
    sp.GetRequiredService<IConfiguration>(),
    sp.GetRequiredService<StubModelProvider>(),
    sp.GetRequiredService<AzureOpenAIProvider>()));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "alive" }));

app.MapPost("/v1/completions", async (
    ProviderRouter router,
    UsageLog usage,
    ILogger<Program> logger,
    GatewayCompletionRequest request,
    CancellationToken ct) =>
{
    var resolved = router.Resolve(request.TaskClass);
    if (resolved is null)
    {
        return Results.BadRequest(new { error = "taskClass must be 'triage' or 'reasoning'." });
    }

    var (provider, target) = resolved.Value;
    var messages = request.Messages.Select(m => new ChatMessage(m.Role, m.Content)).ToList();
    var promptChars = messages.Sum(m => m.Content.Length);
    var stopwatch = Stopwatch.StartNew();

    try
    {
        var result = await provider.CompleteAsync(target.Model, messages, ct);
        stopwatch.Stop();

        await usage.RecordAsync(new UsageRecord(
            DateTimeOffset.UtcNow, request.TaskClass, target.Provider, result.Model,
            promptChars, result.PromptTokens, result.CompletionTokens,
            stopwatch.ElapsedMilliseconds, Success: true), ct);

        return Results.Ok(new
        {
            content = result.Content,
            model = result.Model,
            promptTokens = result.PromptTokens,
            completionTokens = result.CompletionTokens,
            latencyMs = stopwatch.ElapsedMilliseconds
        });
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
        stopwatch.Stop();
        logger.LogError(ex, "Completion failed for task class {TaskClass} via {Provider}.",
            request.TaskClass, target.Provider);

        await usage.RecordAsync(new UsageRecord(
            DateTimeOffset.UtcNow, request.TaskClass, target.Provider, target.Model,
            promptChars, null, null, stopwatch.ElapsedMilliseconds, Success: false), ct);

        return Results.Problem("The upstream model call failed.", statusCode: 502);
    }
});

app.Run();

public record GatewayMessage(string Role, string Content);
public record GatewayCompletionRequest(string TaskClass, List<GatewayMessage> Messages);
