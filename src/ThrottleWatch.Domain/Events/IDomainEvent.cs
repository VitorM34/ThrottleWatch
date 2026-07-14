namespace ThrottleWatch.Domain.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
