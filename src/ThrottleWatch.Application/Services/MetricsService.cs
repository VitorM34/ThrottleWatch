using Microsoft.Extensions.Logging;
using ThrottleWatch.Application.DTOs.Metrics;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Application.Services;

public sealed class MetricsService : IMetricsService
{
    private readonly IMetricsRepository _repository;
    private readonly IMetricQueue _queue;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(
        IMetricsRepository repository,
        IMetricQueue queue,
        ILogger<MetricsService> logger)
    {
        _repository = repository;
        _queue = queue;
        _logger = logger;
    }

    public Task EnqueueBatchAsync(IEnumerable<IngestMetricDto> dtos, CancellationToken ct)
    {
        foreach (var dto in dtos)
        {
            var entry = MetricEntry.Create(
                dto.Path,
                dto.Method,
                dto.StatusCode,
                dto.DurationMs,
                dto.Timestamp,
                dto.ClientIp,
                dto.PolicyName,
                dto.ApiKey);

            if (!_queue.TryEnqueue(entry))
                _logger.LogWarning("Metric queue is full. Dropping entry for path {Path}.", dto.Path);
        }

        return Task.CompletedTask;
    }

    public async Task<MetricsSummaryDto> GetSummaryAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        var total = await _repository.GetTotalRequestsAsync(from, to, ct);
        var blocked = await _repository.GetTotalBlockedAsync(from, to, ct);

        return new MetricsSummaryDto(total, blocked, from, to);
    }

    public async Task<IReadOnlyList<TopEndpointDto>> GetTopEndpointsAsync(
        int top,
        DateTimeOffset from,
        CancellationToken ct)
    {
        var results = await _repository.GetTopEndpointsAsync(top, from, ct);

        return results
            .Select(r => new TopEndpointDto(r.Path, r.Method, r.RequestCount, r.BlockedCount))
            .ToList();
    }

    public async Task<IReadOnlyList<TopClientDto>> GetTopClientsAsync(
        int top,
        DateTimeOffset from,
        CancellationToken ct)
    {
        var results = await _repository.GetTopClientsAsync(top, from, ct);

        return results
            .Select(r => new TopClientDto(r.ClientIdentifier, r.RequestCount, r.BlockedCount))
            .ToList();
    }

    public async Task<IReadOnlyList<TimeSeriesPointDto>> GetTimeSeriesAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        var results = await _repository.GetTimeSeriesAsync(from, to, ct);

        return results
            .Select(r => new TimeSeriesPointDto(r.Timestamp, r.TotalRequests, r.BlockedRequests))
            .ToList();
    }
}
