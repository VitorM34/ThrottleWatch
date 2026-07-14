using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Domain.Events;

public sealed record AlertTriggeredEvent(
    Guid AlertRuleId,
    string RuleName,
    AlertSeverity Severity,
    string Message) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
