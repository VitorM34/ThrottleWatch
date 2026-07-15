using ThrottleWatch.Domain.Events;

namespace ThrottleWatch.Application.Interfaces;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct);
}
