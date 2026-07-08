using ThrottleWatch.Dashboard.Models;

namespace ThrottleWatch.Dashboard.Services;

public interface IMetricsService
{
    Task<DashboardMetrics?> GetDashboardMetricsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EndpointMetrics>> GetTopEndpointsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientMetrics>> GetTopClientsAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TimeSeriesData>> GetRequestTimeSeriesAsync(TimeSpan window, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EndpointMetrics>> GetAllEndpointsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientMetrics>> GetAllClientsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PolicyInfo>> GetPoliciesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AlertInfo>> GetAlertsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InsightInfo>> GetInsightsAsync(CancellationToken cancellationToken = default);
    Task<HealthStatus?> GetHealthAsync(CancellationToken cancellationToken = default);
}
