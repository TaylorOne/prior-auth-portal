namespace PriorAuth.Data.Entities
{
    /// <summary>
    /// Transactional outbox row (ADR-008). Persisted in the same database transaction
    /// as the domain change it announces, then delivered to Service Bus by a background
    /// dispatcher. Delivery is at-least-once; consumers must be idempotent.
    /// </summary>
    public class OutboxMessage
    {
        public long Id { get; set; }

        /// <summary>Message type discriminator, e.g. nameof(PriorAuthSubmittedMessage).</summary>
        public string MessageType { get; set; } = string.Empty;

        /// <summary>JSON-serialized message body.</summary>
        public string Payload { get; set; } = string.Empty;

        /// <summary>Correlation id stamped on the Service Bus message for tracing.</summary>
        public string CorrelationId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        /// <summary>Null until the dispatcher has successfully delivered the message.</summary>
        public DateTime? ProcessedAt { get; set; }

        public int AttemptCount { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public string? LastError { get; set; }
    }
}
