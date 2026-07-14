namespace ThrottleWatch.Domain.Events;

public sealed record MetricRecordedEvent(
    Guid MetricEntryId,
    bool IsBlocked,
    string Path) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
