using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.BackgroundServices;

public sealed class DataRetentionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<DataRetentionService> _logger;

    public DataRetentionService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<DataRetentionService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var storage = CurrentStorage();
        _logger.LogInformation(
            "DataRetentionService started. RetentionDays={RetentionDays}, IntervalHours={IntervalHours}.",
            storage.RetentionDays,
            storage.RetentionIntervalHours);

        // Same loop shape as InsightGeneratorService: work → delay → repeat.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunRetentionAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running data retention.");
            }

            storage = CurrentStorage();
            try
            {
                await Task.Delay(TimeSpan.FromHours(storage.RetentionIntervalHours), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("DataRetentionService stopped.");
    }

    private async Task RunRetentionAsync(CancellationToken stoppingToken)
    {
        var storage = CurrentStorage();
        var cutoff = DateTimeOffset.UtcNow.AddDays(-storage.RetentionDays);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMetricsRepository>();

        await repository.DeleteOlderThanAsync(cutoff, stoppingToken);

        _logger.LogInformation(
            "Retention complete. Deleted raw metrics and rollups older than {Cutoff:o} (RetentionDays={RetentionDays}).",
            cutoff,
            storage.RetentionDays);
    }

    private StorageOptions CurrentStorage() =>
        StorageOptions.Normalize(_options.CurrentValue.Storage);
}
