using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using PriorAuth.Contracts;
using PriorAuth.Data;
using PriorAuth.Data.Services;
using System.Text.Json;

namespace PriorAuthApi.Services;

/// <summary>
/// Delivers pending outbox rows to Service Bus (ADR-008). Delivery is at-least-once:
/// a crash between SendMessageAsync and SaveChangesAsync re-sends the message on the
/// next pass, and the evaluation function's status guard makes the duplicate a no-op.
/// </summary>
public class OutboxDispatcher(
    AppDbContext db,
    ServiceBusSender sender,
    AuditService audit,
    ILogger<OutboxDispatcher> logger)
{
    private const int BatchSize = 20;

    /// <summary>Failed rows wait this long before the dispatcher retries them.</summary>
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMinutes(1);

    public async Task<int> ProcessPendingAsync(CancellationToken ct = default)
    {
        var retryCutoff = DateTime.UtcNow - RetryDelay;

        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null &&
                        (m.AttemptCount == 0 || m.LastAttemptAt < retryCutoff))
            .OrderBy(m => m.Id)
            .Take(BatchSize)
            .ToListAsync(ct);

        var delivered = 0;

        foreach (var message in pending)
        {
            message.AttemptCount++;
            message.LastAttemptAt = DateTime.UtcNow;

            try
            {
                await sender.SendMessageAsync(new ServiceBusMessage(message.Payload)
                {
                    CorrelationId = message.CorrelationId,
                    // Outbox id as MessageId enables broker-side duplicate detection
                    MessageId = message.Id.ToString()
                }, ct);

                message.ProcessedAt = DateTime.UtcNow;
                message.LastError = null;
                delivered++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                message.LastError = ex.Message;
                logger.LogError(ex,
                    "Outbox message {Id} ({Type}) failed to send on attempt {Attempt}.",
                    message.Id, message.MessageType, message.AttemptCount);
            }

            await db.SaveChangesAsync(ct);

            if (message.ProcessedAt is not null &&
                message.MessageType == nameof(PriorAuthSubmittedMessage))
            {
                var payload = JsonSerializer.Deserialize<PriorAuthSubmittedMessage>(message.Payload);
                if (payload is not null)
                {
                    await audit.LogAsync(payload.PriorAuthRequestId, AuditEventTypes.MessagePublished,
                        AuditActors.System, new
                        {
                            correlationId = message.CorrelationId,
                            outboxMessageId = message.Id,
                            queue = "auth-evaluation"
                        }, ct);
                }
            }
        }

        return delivered;
    }
}

/// <summary>
/// Polls the outbox on a fixed interval. Registered only when Service Bus is configured.
/// </summary>
public class OutboxDispatcherService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<OutboxDispatcherService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(
            configuration.GetValue("Outbox:PollIntervalSeconds", 5));

        logger.LogInformation("Outbox dispatcher started (interval {Interval}).", interval);

        using var timer = new PeriodicTimer(interval);

        while (await WaitForNextTickSafeAsync(timer, stoppingToken))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();
                await dispatcher.ProcessPendingAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Failures are recorded per-row by the dispatcher; this guards the loop
                // against anything outside it (e.g. the database being unreachable).
                logger.LogError(ex, "Outbox dispatch pass failed.");
            }
        }
    }

    private static async Task<bool> WaitForNextTickSafeAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
