using ThrottleWatch.Application.DTOs.Metrics;

namespace ThrottleWatch.Application.Services;

public interface IMetricsService
{
    Task EnqueueBatchAsync(IEnumerable<IngestMetricDto> dtos, CancellationToken ct);
    Task<MetricsSummaryDto> GetSummaryAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
    Task<IReadOnlyList<TopEndpointDto>> GetTopEndpointsAsync(int top, DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<TopClientDto>> GetTopClientsAsync(int top, DateTimeOffset from, CancellationToken ct);
    Task<IReadOnlyList<TimeSeriesPointDto>> GetTimeSeriesAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct);
}
