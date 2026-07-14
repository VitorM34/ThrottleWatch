using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Domain.Events;

public sealed record InsightGeneratedEvent(
    Guid InsightId,
    InsightType Type,
    string Title) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
