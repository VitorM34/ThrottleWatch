using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Domain.Events;

namespace ThrottleWatch.Infrastructure.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(
        IServiceProvider serviceProvider,
        ILogger<DomainEventDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct)
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
        var handlers = _serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            if (handler is null)
                continue;

            var method = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync));
            if (method is null)
                continue;

            var task = (Task?)method.Invoke(handler, [domainEvent, ct]);
            if (task is not null)
                await task;
        }

        _logger.LogDebug(
            "Dispatched domain event {EventType} at {OccurredAt}",
            domainEvent.GetType().Name,
            domainEvent.OccurredAt);
    }
}
