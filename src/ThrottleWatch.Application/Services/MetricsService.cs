using Microsoft.Extensions.Logging;
using ThrottleWatch.Application.DTOs.Metrics;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Application.Services;

public sealed class MetricsService : IMetricsService
{
    private readonly IMetricsRepository _repository;
    private readonly IMetricQueue _queue;
    private readonly IOperationalMetrics _operationalMetrics;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<MetricsService> _logger;

    public MetricsService(
        IMetricsRepository repository,
        IMetricQueue queue,
        IOperationalMetrics operationalMetrics,
        ITenantContext tenantContext,
        ILogger<MetricsService> logger)
    {
        _repository = repository;
        _queue = queue;
        _operationalMetrics = operationalMetrics;
        _tenantContext = tenantContext;
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
                dto.ApiKey,
                _tenantContext.TenantId);

            if (_queue.TryEnqueue(entry))
                continue;

            _operationalMetrics.RecordDrop();
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
        var averageLatencyMs = await _repository.GetAverageLatencyMsAsync(from, to, ct);
        var activeClients = await _repository.GetActiveClientsAsync(from, to, ct);

        return new MetricsSummaryDto(total, blocked, from, to, averageLatencyMs, activeClients);
    }

    public async Task<IReadOnlyList<TopEndpointDto>> GetTopEndpointsAsync(
        int top,
        DateTimeOffset from,
        CancellationToken ct)
    {
        var results = await _repository.GetTopEndpointsAsync(top, from, ct);

        return results
            .Select(r => new TopEndpointDto(
                r.Path,
                r.Method,
                r.RequestCount,
                r.BlockedCount,
                r.AverageLatencyMs,
                r.PolicyName,
                r.LastActivity))
            .ToList();
    }

    public async Task<IReadOnlyList<TopClientDto>> GetTopClientsAsync(
        int top,
        DateTimeOffset from,
        CancellationToken ct)
    {
        var results = await _repository.GetTopClientsAsync(top, from, ct);

        return results
            .Select(r => new TopClientDto(
                r.ClientIdentifier,
                r.RequestCount,
                r.BlockedCount,
                r.FirstSeen,
                r.LastSeen))
            .ToList();
    }

    public async Task<IReadOnlyList<ObservedPolicyDto>> GetObservedPoliciesAsync(
        DateTimeOffset from,
        CancellationToken ct)
    {
        var results = await _repository.GetObservedPoliciesAsync(from, ct);

        return results
            .Select(r => new ObservedPolicyDto(r.Name, r.TotalRequests, r.BlockedCount))
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
