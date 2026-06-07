namespace PriorAuth.Data.Entities;

public class AuditEvent
{
    public int Id { get; set; }
    public int PriorAuthRequestId { get; set; }
    public PriorAuthRequest Request { get; set; } = null!;
    public string EventType { get; set; } = string.Empty;
    public string? Actor { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }
}
