using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Events;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Infrastructure.BackgroundServices;

public sealed class InsightGeneratorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InsightGeneratorService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    public InsightGeneratorService(
        IServiceScopeFactory scopeFactory,
        ILogger<InsightGeneratorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("InsightGeneratorService started.");

        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await GenerateAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating insights.");
            }
        }

        _logger.LogInformation("InsightGeneratorService stopped.");
    }

    private async Task GenerateAsync(CancellationToken ct)
    {
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        var metricsRepository = scope.ServiceProvider.GetRequiredService<IMetricsRepository>();
        var insightRepository = scope.ServiceProvider.GetRequiredService<IInsightRepository>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var topEndpoints = await metricsRepository.GetTopEndpointsAsync(5, from, ct);

        foreach (var endpoint in topEndpoints)
        {
            if (endpoint.RequestCount == 0)
                continue;

            var blockRate = (double)endpoint.BlockedCount / endpoint.RequestCount * 100;
            if (blockRate < 50)
                continue;

            var title = $"High block rate on {endpoint.Path}";
            var description =
                $"Endpoint {endpoint.Method} {endpoint.Path} blocked {blockRate:F1}% of requests in the last hour.";

            var insight = Insight.Create(
                InsightType.HighBlockRate,
                title,
                description,
                AlertSeverity.Warning,
                endpoint.Path);

            await insightRepository.AddAsync(insight, ct);

            await dispatcher.DispatchAsync(
                new InsightGeneratedEvent(insight.Id, insight.Type, insight.Title),
                ct);

            _logger.LogInformation("Insight generated: {Title}", title);
        }
    }
}
