using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Infrastructure.Configuration;
using ThrottleWatch.Infrastructure.Insights;

namespace ThrottleWatch.Infrastructure.BackgroundServices;

public sealed class InsightGeneratorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<InsightGeneratorService> _logger;

    public InsightGeneratorService(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<InsightGeneratorService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InsightGeneratorService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunInsightsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating insights.");
            }

            var intervalMinutes = Math.Clamp(_options.CurrentValue.Insights.IntervalMinutes, 1, 60);
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("InsightGeneratorService stopped.");
    }

    private async Task RunInsightsAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        var generator = scope.ServiceProvider.GetRequiredService<IInsightGenerator>();
        var insights = await generator.GenerateAsync(ct);

        if (insights.Count > 0)
            _logger.LogInformation("Generated {Count} insight(s).", insights.Count);
    }
}
