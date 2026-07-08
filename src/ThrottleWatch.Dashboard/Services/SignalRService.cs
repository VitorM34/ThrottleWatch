using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.Services;

public sealed class SignalRService : ISignalRService
{
    private readonly HubConnection _connection;
    private readonly ILogger<SignalRService> _logger;

    public bool IsConnected => _connection.State == HubConnectionState.Connected;

    public ConnectionState State => _connection.State switch
    {
        HubConnectionState.Connected => ConnectionState.Connected,
        HubConnectionState.Connecting => ConnectionState.Connecting,
        HubConnectionState.Reconnecting => ConnectionState.Reconnecting,
        _ => ConnectionState.Disconnected
    };

    public event EventHandler<DashboardMetrics>? MetricsUpdated;
    public event EventHandler<AlertInfo>? AlertReceived;
    public event EventHandler<ConnectionState>? ConnectionStateChanged;

    public SignalRService(IOptions<ThrottleWatchOptions> options, ILogger<SignalRService> logger)
    {
        _logger = logger;

        _connection = new HubConnectionBuilder()
            .WithUrl(options.Value.HubUrl)
            .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10)])
            .Build();

        RegisterHandlers();
    }

    private void RegisterHandlers()
    {
        _connection.On<DashboardMetrics>("MetricsUpdated", metrics =>
        {
            MetricsUpdated?.Invoke(this, metrics);
        });

        _connection.On<AlertInfo>("AlertReceived", alert =>
        {
            AlertReceived?.Invoke(this, alert);
        });

        _connection.Reconnecting += ex =>
        {
            _logger.LogWarning(ex, "SignalR connection lost, reconnecting...");
            ConnectionStateChanged?.Invoke(this, ConnectionState.Reconnecting);
            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            _logger.LogInformation("SignalR reconnected with id {ConnectionId}", connectionId);
            ConnectionStateChanged?.Invoke(this, ConnectionState.Connected);
            return Task.CompletedTask;
        };

        _connection.Closed += ex =>
        {
            _logger.LogWarning(ex, "SignalR connection closed");
            ConnectionStateChanged?.Invoke(this, ConnectionState.Disconnected);
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State != HubConnectionState.Disconnected)
            return;

        try
        {
            ConnectionStateChanged?.Invoke(this, ConnectionState.Connecting);
            await _connection.StartAsync(cancellationToken);
            ConnectionStateChanged?.Invoke(this, ConnectionState.Connected);
            _logger.LogInformation("SignalR connection established");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to start SignalR connection");
            ConnectionStateChanged?.Invoke(this, ConnectionState.Failed);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_connection.State == HubConnectionState.Disconnected)
            return;

        try
        {
            await _connection.StopAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error stopping SignalR connection");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
