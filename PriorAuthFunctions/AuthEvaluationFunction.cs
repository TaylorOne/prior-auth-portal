Azure.Messaging.ServiceBus;
Microsoft.Azure.Functions.Worker;
Microsoft.EntityFrameworkCore;
Microsoft.Extensions.Logging;
PriorAuth.AuthEngine.Services;
PriorAuth.AuthEngine.Models;
PriorAuth.Data.Entities;
PriorAuth.Contracts;
System.Text.Json;

namespace PriorAuthFunctions.Functions;

public class AuthEvaluationFunction
{
    private readonly AppDbContext _db;
    private readonly AuthEvaluationEngine _engine;
    private readonly ILogger<AuthEvaluationFunction> _logger;

    public AuthEvaluationFunction(
        AppDbContext db,
        AuthEvaluationEngine engine,
        ILogger<AuthEvaluationFunction> logger)
    {
        _db = db;
        _engine = engine;
        _logger = logger;
    }

    [Function(nameof(AuthEvaluationFunction))]
    public async Task Run(
        [ServiceBusTrigger("auth-evaluation", Connection = "ServiceBusConnection")] 
        ServiceBusReceivedMessage message,
        CancellationToken cancellationToken)
    {
        var correlationId = message.CorrelationId ?? message.MessageId;
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        });

        _logger.LogInformation("AuthEvaluationFunction triggered. MessageId: {MessageId}", message.MessageId);

        PriorAuthSubmittedMessage? payload;

        try
        {
            payload = message.Body.ToObjectFromJson<PriorAuthSubmittedMessage>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize message body. Sending to dead letter.");
            throw; // SDK dead-letters on unhandled exception
        }

        if (payload is null)
        {
            _logger.LogError("Deserialized payload was null. MessageId: {MessageId}", message.MessageId);
            throw new InvalidOperationException("Null payload.");
        }

        var request = await _db.PriorAuthRequests
            .Include(r => r.AuthRule)
            .FirstOrDefaultAsync(r => r.Id == payload.PriorAuthRequestId, cancellationToken);

        if (request is null)
        {
            _logger.LogError("PriorAuthRequest {Id} not found.", payload.PriorAuthRequestId);
            throw new InvalidOperationException($"Request {payload.PriorAuthRequestId} not found.");
        }

        // Idempotency guard — don't re-evaluate a decided request
        if (request.Status is Status.Approved or Status.Denied)
        {
            _logger.LogWarning("Request {Id} already decided ({Status}). Skipping.", 
                request.Id, request.Status);
            return;
        }

        var decision = _engine.Evaluate(request.ClinicalData, request.AuthRule.RuleDefinition);

        request.Status = decision.Outcome == AuthOutcome.Approved
            ? Status.Approved
            : Status.Denied;

        request.DeterminationDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Request {Id} decided: {Outcome}", request.Id, decision.Outcome);
    }
}