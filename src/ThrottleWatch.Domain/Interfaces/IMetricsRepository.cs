using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Domain.Interfaces;

public interface IMetricsRepository
{
    Task AddRangeAsync(IEnumerable<MetricEntry> entries, CancellationToken ct);
    Task<long> GetTotalRequestsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<long> GetTotalBlockedAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<IReadOnlyList<MetricEntry>> GetTopEndpointsAsync(int top, DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<MetricEntry>> GetTopClientsAsync(int top, DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<MetricEntry>> GetTimeSeriesAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task DeleteOlderThanAsync(DateTimeOffset cutoff, CancellationToken ct);
}
