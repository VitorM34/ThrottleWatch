using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ThrottleWatch.Application.DTOs.Metrics;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Application.Services;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Domain.ReadModels;

namespace ThrottleWatch.Application.Tests.Services;

public sealed class MetricsServiceTests
{
    private readonly IMetricsRepository _repository = Substitute.For<IMetricsRepository>();
    private readonly IMetricQueue _queue = Substitute.For<IMetricQueue>();
    private readonly IOperationalMetrics _operationalMetrics = Substitute.For<IOperationalMetrics>();
    private readonly ILogger<MetricsService> _logger = Substitute.For<ILogger<MetricsService>>();
    private readonly MetricsService _sut;

    public MetricsServiceTests()
    {
        _sut = new MetricsService(_repository, _queue, _operationalMetrics, _logger);
    }

    [Fact]
    public async Task EnqueueBatchAsync_WhenQueueAccepts_ShouldEnqueueEntries()
    {
        _queue.TryEnqueue(Arg.Any<MetricEntry>()).Returns(true);
        var batch = new[]
        {
            new IngestMetricDto("/api/test", "GET", 200, 12, DateTimeOffset.UtcNow)
        };

        await _sut.EnqueueBatchAsync(batch, CancellationToken.None);

        _queue.Received(1).TryEnqueue(Arg.Any<MetricEntry>());
        _operationalMetrics.DidNotReceive().RecordDrop();
    }

    [Fact]
    public async Task EnqueueBatchAsync_WhenQueueFull_ShouldRecordDrop()
    {
        _queue.TryEnqueue(Arg.Any<MetricEntry>()).Returns(false);
        var batch = new[]
        {
            new IngestMetricDto("/api/test", "GET", 429, 5, DateTimeOffset.UtcNow)
        };

        await _sut.EnqueueBatchAsync(batch, CancellationToken.None);

        _operationalMetrics.Received(1).RecordDrop();
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldMapRepositoryTotals()
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow;
        _repository.GetTotalRequestsAsync(from, to, Arg.Any<CancellationToken>()).Returns(100);
        _repository.GetTotalBlockedAsync(from, to, Arg.Any<CancellationToken>()).Returns(25);

        var summary = await _sut.GetSummaryAsync(from, to, CancellationToken.None);

        summary.TotalRequests.Should().Be(100);
        summary.TotalBlocked.Should().Be(25);
        summary.BlockRatePercent.Should().Be(25);
        summary.From.Should().Be(from);
        summary.To.Should().Be(to);
    }

    [Fact]
    public async Task GetTopEndpointsAsync_ShouldMapReadModels()
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        _repository.GetTopEndpointsAsync(5, from, Arg.Any<CancellationToken>())
            .Returns(new List<EndpointSummary>
            {
                new("/api/users", "GET", 50, 10)
            });

        var result = await _sut.GetTopEndpointsAsync(5, from, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Path.Should().Be("/api/users");
        result[0].Method.Should().Be("GET");
        result[0].RequestCount.Should().Be(50);
        result[0].BlockedCount.Should().Be(10);
    }
}
