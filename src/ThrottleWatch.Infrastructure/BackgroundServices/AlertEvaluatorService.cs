using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Events;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Alerting;
using ThrottleWatch.Infrastructure.Tenancy;

namespace ThrottleWatch.Infrastructure.BackgroundServices;

public sealed class AlertEvaluatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IApiKeyTenantMap _tenantMap;
    private readonly ILogger<AlertEvaluatorService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public AlertEvaluatorService(
        IServiceScopeFactory scopeFactory,
        IApiKeyTenantMap tenantMap,
        ILogger<AlertEvaluatorService> logger)
    {
        _scopeFactory = scopeFactory;
        _tenantMap = tenantMap;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AlertEvaluatorService started.");

        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await EvaluateAllTenantsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while evaluating alert rules.");
            }
        }

        _logger.LogInformation("AlertEvaluatorService stopped.");
    }

    private async Task EvaluateAllTenantsAsync(CancellationToken ct)
    {
        foreach (var tenantId in ConfiguredTenants.Ids(_tenantMap))
        {
            await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(tenantId);
            await EvaluateAsync(scope.ServiceProvider, ct);
        }
    }

    private async Task EvaluateAsync(IServiceProvider services, CancellationToken ct)
    {
        var alertRepository = services.GetRequiredService<IAlertRepository>();
        var metricsRepository = services.GetRequiredService<IMetricsRepository>();
        var dispatcher = services.GetRequiredService<IDomainEventDispatcher>();
        var notificationService = services.GetRequiredService<AlertNotificationService>();

        var rules = await alertRepository.GetActiveRulesAsync(ct);
        var now = DateTimeOffset.UtcNow;
        var from = now.AddMinutes(-5);

        var total = await metricsRepository.GetTotalRequestsAsync(from, now, ct);
        var blocked = await metricsRepository.GetTotalBlockedAsync(from, now, ct);
        var blockRate = total == 0 ? 0d : (double)blocked / total * 100;

        foreach (var rule in rules)
        {
            if (!rule.CanTrigger(now))
                continue;

            if (!IsConditionMet(rule.Condition, blockRate, rule.Threshold))
                continue;

            var message = $"Alert '{rule.Name}' triggered. Block rate {blockRate:F2}% exceeded threshold {rule.Threshold}.";
            var alertEvent = AlertEvent.Create(rule.Id, rule.Name, message, rule.Severity, rule.TenantId);

            rule.RecordTrigger(now);
            await alertRepository.UpdateAsync(rule, ct);
            await alertRepository.AddEventAsync(alertEvent, ct);

            await dispatcher.DispatchAsync(
                new AlertTriggeredEvent(rule.Id, rule.Name, rule.Severity, message),
                ct);

            await notificationService.NotifyAllAsync(
                new AlertNotification(
                    rule.Name,
                    message,
                    rule.Severity,
                    alertEvent.TriggeredAt),
                ct);

            _logger.LogWarning("Alert triggered: {RuleName} (tenant {TenantId})", rule.Name, rule.TenantId);
        }
    }

    private static bool IsConditionMet(string condition, double blockRate, double threshold)
    {
        if (condition.Contains("block_rate", StringComparison.OrdinalIgnoreCase))
            return blockRate >= threshold;

        return false;
    }
}
