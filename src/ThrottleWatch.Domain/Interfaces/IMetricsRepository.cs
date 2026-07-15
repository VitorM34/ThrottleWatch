using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.ReadModels;

namespace ThrottleWatch.Domain.Interfaces;

public interface IMetricsRepository
{
    Task AddRangeAsync(IEnumerable<MetricEntry> entries, CancellationToken ct);
    Task<long> GetTotalRequestsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<long> GetTotalBlockedAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<IReadOnlyList<EndpointSummary>> GetTopEndpointsAsync(int top, DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<ClientSummary>> GetTopClientsAsync(int top, DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<TimeSeriesPoint>> GetTimeSeriesAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task DeleteOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct);
}
