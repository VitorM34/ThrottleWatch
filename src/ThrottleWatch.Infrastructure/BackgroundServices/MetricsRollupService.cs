using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.BackgroundServices;

/// <summary>
/// Builds minute/hour rollups from completed raw buckets so History/timeseries
/// can scale without loading every MetricEntry into memory.
/// </summary>
public sealed class MetricsRollupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<MetricsRollupService> _logger;

    public MetricsRollupService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<MetricsRollupService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var storage = CurrentStorage();
        _logger.LogInformation(
            "MetricsRollupService started. IntervalMinutes={IntervalMinutes}, LookbackHours={LookbackHours}.",
            storage.RollupIntervalMinutes,
            storage.RollupLookbackHours);

        // Brief settle so MetricProcessor can flush before the first rebuild.
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunRollupAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while running metrics rollup.");
            }

            storage = CurrentStorage();
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(storage.RollupIntervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("MetricsRollupService stopped.");
    }

    private async Task RunRollupAsync(CancellationToken stoppingToken)
    {
        var storage = CurrentStorage();
        var now = DateTimeOffset.UtcNow;

        // Only completed UTC minutes (exclude the in-flight minute).
        var toExclusive = TruncateToMinute(now);
        var fromInclusive = toExclusive.AddHours(-storage.RollupLookbackHours);

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMetricsRepository>();

        await repository.RebuildRollupsAsync(fromInclusive, toExclusive, stoppingToken);

        _logger.LogInformation(
            "Rollup rebuild complete for [{From:o}, {To:o}) (lookback {LookbackHours}h).",
            fromInclusive,
            toExclusive,
            storage.RollupLookbackHours);
    }

    private StorageOptions CurrentStorage() =>
        StorageOptions.Normalize(_options.CurrentValue.Storage);

    private static DateTimeOffset TruncateToMinute(DateTimeOffset value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, TimeSpan.Zero);
}
