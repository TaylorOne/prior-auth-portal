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
        try
        {
            _db.AuditEvents.Add(new AuditEvent
            {
                PriorAuthRequestId = requestId,
                EventType = eventType,
                Actor = actor,
                Timestamp = DateTime.UtcNow,
                Details = details is not null ? JsonSerializer.Serialize(details) : null
            });
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log {EventType} audit event for request {Id}", eventType, requestId);
        }
    }
}
