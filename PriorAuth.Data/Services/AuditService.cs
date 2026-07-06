using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PriorAuth.Data.Entities;
using System.Text.Json;

namespace PriorAuth.Data.Services;

public class AuditService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext db, ILogger<AuditService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task LogAsync(int requestId, string eventType, string actor, object? details = null, CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            PriorAuthRequestId = requestId,
            EventType = eventType,
            Actor = actor,
            Timestamp = DateTime.UtcNow,
            Details = details is not null ? JsonSerializer.Serialize(details) : null
        };

        try
        {
            _db.AuditEvents.Add(auditEvent);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Detach the failed row so it doesn't poison later SaveChanges calls
            // made by other services sharing this scoped context.
            _db.Entry(auditEvent).State = EntityState.Detached;
            _logger.LogError(ex, "Failed to log {EventType} audit event for request {Id}", eventType, requestId);
        }
    }
}
