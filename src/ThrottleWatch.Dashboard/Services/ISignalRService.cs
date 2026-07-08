using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.Services;

public interface ISignalRService : IAsyncDisposable
{
    bool IsConnected { get; }
    ConnectionState State { get; }

    event EventHandler<DashboardMetrics>? MetricsUpdated;
    event EventHandler<AlertInfo>? AlertReceived;
    event EventHandler<ConnectionState>? ConnectionStateChanged;

    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Failed
}
