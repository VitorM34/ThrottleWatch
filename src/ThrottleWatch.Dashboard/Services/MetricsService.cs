using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ThrottleWatch.Dashboard.DTOs;
using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.Services;

public sealed class MetricsService : IMetricsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MetricsService> _logger;
    private readonly ThrottleWatchOptions _options;

    public MetricsService(
        HttpClient httpClient,
        IOptions<ThrottleWatchOptions> options,
        ILogger<MetricsService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DashboardMetrics?> GetDashboardMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var dto = await _httpClient.GetFromJsonAsync<MetricsSummaryApiDto>(
                "api/metrics/summary", cancellationToken);
            return dto?.ToModel();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch dashboard metrics");
            return null;
        }
    }

    public async Task<IReadOnlyList<EndpointMetrics>> GetTopEndpointsAsync(
        int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<TopEndpointApiDto>>(
                $"api/metrics/top-endpoints?top={count}", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch top endpoints");
            return [];
        }
    }

    public async Task<IReadOnlyList<ClientMetrics>> GetTopClientsAsync(
        int count = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<TopClientApiDto>>(
                $"api/metrics/top-clients?top={count}", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch top clients");
            return [];
        }
    }

    public async Task<IReadOnlyList<TimeSeriesData>> GetRequestTimeSeriesAsync(
        TimeSpan window, CancellationToken cancellationToken = default)
    {
        try
        {
            var to = DateTimeOffset.UtcNow;
            var from = to - window;
            var path =
                $"api/metrics/timeseries?from={Uri.EscapeDataString(from.ToString("o"))}&to={Uri.EscapeDataString(to.ToString("o"))}";

            var points = await _httpClient.GetFromJsonAsync<IReadOnlyList<TimeSeriesPointApiDto>>(
                path, cancellationToken);

            if (points is null || points.Count == 0)
                return [];

            return
            [
                new TimeSeriesData
                {
                    Name = "Total",
                    Points = points
                        .Select(p => new TimeSeriesPoint { Timestamp = p.Timestamp, Value = p.TotalRequests })
                        .ToList()
                },
                new TimeSeriesData
                {
                    Name = "Bloqueadas",
                    Points = points
                        .Select(p => new TimeSeriesPoint { Timestamp = p.Timestamp, Value = p.BlockedRequests })
                        .ToList()
                }
            ];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch time series data");
            return [];
        }
    }

    public async Task<IReadOnlyList<EndpointMetrics>> GetAllEndpointsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetTopEndpointsAsync(50, cancellationToken);
    }

    public async Task<IReadOnlyList<ClientMetrics>> GetAllClientsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetTopClientsAsync(50, cancellationToken);
    }

    public Task<IReadOnlyList<PolicyInfo>> GetPoliciesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<PolicyInfo>>([]);
    }

    public async Task<IReadOnlyList<AlertInfo>> GetAlertsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<AlertEventApiDto>>(
                "api/alerts/events?count=50", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch alerts");
            return [];
        }
    }

    public async Task<IReadOnlyList<InsightInfo>> GetInsightsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<InsightApiDto>>(
                "api/insights", cancellationToken);
            return dtos?.Where(d => !d.IsDismissed).Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch insights");
            return [];
        }
    }

    public async Task<HealthStatus?> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.GetAsync("health", cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var body = (await response.Content.ReadAsStringAsync(cancellationToken)).Trim();
            return new HealthStatus
            {
                Status = string.IsNullOrWhiteSpace(body) ? "Healthy" : body
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch health status");
            return null;
        }
    }
}
