using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PriorAuth.AuthEngine.Services;
using PriorAuth.AuthEngine.Models;
using PriorAuth.Data;
using PriorAuth.Data.Entities;
using PriorAuth.Contracts;
using System.Text.Json;

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

        // Idempotency guard
        if (request.Status != Status.Submitted)
        {
            _logger.LogWarning("Request {Id} is not in Submitted state ({Status}). Skipping.", 
                request.Id, request.Status);
            return;
        }

        if (request.AuthRule.RequiresManualReview)
        {
            request.Status = Status.UnderReview;
            await _db.SaveChangesAsync(cancellationToken);
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