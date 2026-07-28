using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.Alerting;

public sealed class AlertNotificationService
{
    private readonly IEnumerable<IAlertNotifier> _notifiers;
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;
    private readonly ILogger<AlertNotificationService> _logger;

    public AlertNotificationService(
        IEnumerable<IAlertNotifier> notifiers,
        IOptionsMonitor<ThrottleWatchOptions> options,
        ILogger<AlertNotificationService> logger)
    {
        _notifiers = notifiers;
        _options = options;
        _logger = logger;
    }

    public async Task NotifyAllAsync(AlertNotification notification, CancellationToken ct)
    {
        if (!_options.CurrentValue.Alerts.Enabled)
        {
            _logger.LogDebug("Alert notifications are disabled globally.");
            return;
        }

        var enabledNotifiers = _notifiers.Where(n => n.IsEnabled).ToArray();
        if (enabledNotifiers.Length == 0)
        {
            _logger.LogDebug("No alert notification channels are enabled.");
            return;
        }

        var tasks = enabledNotifiers.Select(notifier => NotifyChannelAsync(notifier, notification, ct));
        await Task.WhenAll(tasks);
    }

    private async Task NotifyChannelAsync(
        IAlertNotifier notifier,
        AlertNotification notification,
        CancellationToken ct)
    {
        try
        {
            await notifier.NotifyAsync(notification, ct);
            _logger.LogInformation(
                "Alert notification sent via {Channel} for rule {RuleName}.",
                notifier.ChannelName,
                notification.RuleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Alert notification failed via {Channel} for rule {RuleName}.",
                notifier.ChannelName,
                notification.RuleName);
        }
    }
}
