namespace ThrottleWatch.Infrastructure.Alerting;

public interface IAlertNotifier
{
    string ChannelName { get; }

    bool IsEnabled { get; }

    Task NotifyAsync(AlertNotification notification, CancellationToken ct);
}
