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
            var dto = await _httpClient.GetFromJsonAsync<DashboardMetricsDto>(
                "api/throttlewatch/metrics/summary", cancellationToken);
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
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<EndpointMetricsDto>>(
                $"api/throttlewatch/metrics/endpoints/top?count={count}", cancellationToken);
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
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<ClientMetricsDto>>(
                $"api/throttlewatch/metrics/clients/top?count={count}", cancellationToken);
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
            var minutes = (int)window.TotalMinutes;
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<TimeSeriesDataDto>>(
                $"api/throttlewatch/metrics/timeseries?windowMinutes={minutes}", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
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
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<EndpointMetricsDto>>(
                "api/throttlewatch/metrics/endpoints", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch all endpoints");
            return [];
        }
    }

    public async Task<IReadOnlyList<ClientMetrics>> GetAllClientsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<ClientMetricsDto>>(
                "api/throttlewatch/metrics/clients", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch all clients");
            return [];
        }
    }

    public async Task<IReadOnlyList<PolicyInfo>> GetPoliciesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<PolicyInfoDto>>(
                "api/throttlewatch/policies", cancellationToken);
            return dtos?.Select(d => d.ToModel()).ToList() ?? [];
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch policies");
            return [];
        }
    }

    public async Task<IReadOnlyList<AlertInfo>> GetAlertsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtos = await _httpClient.GetFromJsonAsync<IReadOnlyList<AlertInfoDto>>(
                "api/throttlewatch/alerts", cancellationToken);
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
            var insights = await _httpClient.GetFromJsonAsync<IReadOnlyList<InsightInfo>>(
                "api/throttlewatch/insights", cancellationToken);
            return insights ?? [];
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
            return await _httpClient.GetFromJsonAsync<HealthStatus>(
                "health", cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to fetch health status");
            return null;
        }
    }
}
