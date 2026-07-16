using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Infrastructure.BackgroundServices;

public sealed class DataRetentionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DataRetentionService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6);
    private static readonly TimeSpan Retention = TimeSpan.FromDays(30);

    public DataRetentionService(
        IServiceScopeFactory scopeFactory,
        ILogger<DataRetentionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DataRetentionService started.");

        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
                var repository = scope.ServiceProvider.GetRequiredService<IMetricsRepository>();
                var cutoff = DateTimeOffset.UtcNow.Subtract(Retention);

                await repository.DeleteOlderThanAsync(cutoff, stoppingToken);

                _logger.LogInformation("Deleted metrics older than {Cutoff}.", cutoff);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running data retention.");
            }
        }

        _logger.LogInformation("DataRetentionService stopped.");
    }
}
