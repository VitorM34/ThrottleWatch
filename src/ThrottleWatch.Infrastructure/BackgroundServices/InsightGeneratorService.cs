using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Infrastructure.Configuration;
using ThrottleWatch.Infrastructure.Insights;
using ThrottleWatch.Infrastructure.Tenancy;

namespace ThrottleWatch.Infrastructure.BackgroundServices;

public sealed class InsightGeneratorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IApiKeyTenantMap _tenantMap;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<InsightGeneratorService> _logger;

    public InsightGeneratorService(
        IServiceScopeFactory scopeFactory,
        IApiKeyTenantMap tenantMap,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<InsightGeneratorService> logger)
    {
        _scopeFactory = scopeFactory;
        _tenantMap = tenantMap;
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
        foreach (var tenantId in ConfiguredTenants.Ids(_tenantMap))
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(tenantId);
            var generator = scope.ServiceProvider.GetRequiredService<IInsightGenerator>();
            var insights = await generator.GenerateAsync(ct);

            if (insights.Count > 0)
            {
                _logger.LogInformation(
                    "Generated {Count} insight(s) for tenant {TenantId}.",
                    insights.Count,
                    tenantId);
            }
        }
    }
}
