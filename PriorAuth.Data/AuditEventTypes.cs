namespace PriorAuth.Data;

public static class AuditEventTypes
{
    public const string RequestCreated = "RequestCreated";
    public const string MessagePublished = "MessagePublished";
    public const string FunctionReceivedMessage = "FunctionReceivedMessage";
    public const string EvaluationCompleted = "EvaluationCompleted";
    public const string StatusTransitioned = "StatusTransitioned";
    public const string ManualReviewDecision = "ManualReviewDecision";
}
