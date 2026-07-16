using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Domain.Events;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Infrastructure.BackgroundServices;

public sealed class MetricProcessorService : BackgroundService
{
    private readonly IMetricQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MetricProcessorService> _logger;

    public MetricProcessorService(
        IMetricQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<MetricProcessorService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MetricProcessorService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var batch = _queue.DequeueBatch(100);

                if (batch.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);
                    continue;
                }

                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IMetricsRepository>();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

                await repository.AddRangeAsync(batch, stoppingToken);

                foreach (var entry in batch)
                {
                    await dispatcher.DispatchAsync(
                        new MetricRecordedEvent(entry.Id, entry.IsBlocked, entry.Path),
                        stoppingToken);
                }

                _logger.LogDebug("Persisted {Count} metric entries.", batch.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing metric batch.");
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        _logger.LogInformation("MetricProcessorService stopped.");
    }
}
