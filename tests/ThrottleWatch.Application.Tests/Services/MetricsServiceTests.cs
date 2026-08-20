using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ThrottleWatch.Application.DTOs.Metrics;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Application.Services;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Domain.ReadModels;

namespace ThrottleWatch.Application.Tests.Services;

public sealed class MetricsServiceTests
{
    private readonly IMetricsRepository _repository = Substitute.For<IMetricsRepository>();
    private readonly IMetricQueue _queue = Substitute.For<IMetricQueue>();
    private readonly IOperationalMetrics _operationalMetrics = Substitute.For<IOperationalMetrics>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly ILogger<MetricsService> _logger = Substitute.For<ILogger<MetricsService>>();
    private readonly MetricsService _sut;

    public MetricsServiceTests()
    {
        _tenantContext.TenantId.Returns("default");
        _sut = new MetricsService(_repository, _queue, _operationalMetrics, _tenantContext, _logger);
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
    public async Task EnqueueBatchAsync_ShouldStampTenantFromContext()
    {
        _tenantContext.TenantId.Returns("acme");
        MetricEntry? captured = null;
        _queue.TryEnqueue(Arg.Do<MetricEntry>(e => captured = e)).Returns(true);
        var batch = new[]
        {
            new IngestMetricDto("/api/test", "GET", 200, 12, DateTimeOffset.UtcNow)
        };

        await _sut.EnqueueBatchAsync(batch, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.TenantId.Should().Be("acme");
    }

    [Fact]
    public async Task GetSummaryAsync_ShouldMapRepositoryTotalsAndHonestyFields()
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow;
        _repository.GetTotalRequestsAsync(from, to, Arg.Any<CancellationToken>()).Returns(100);
        _repository.GetTotalBlockedAsync(from, to, Arg.Any<CancellationToken>()).Returns(25);
        _repository.GetAverageLatencyMsAsync(from, to, Arg.Any<CancellationToken>()).Returns(42.5);
        _repository.GetActiveClientsAsync(from, to, Arg.Any<CancellationToken>()).Returns(7);

        var summary = await _sut.GetSummaryAsync(from, to, CancellationToken.None);

        summary.TotalRequests.Should().Be(100);
        summary.TotalBlocked.Should().Be(25);
        summary.BlockRatePercent.Should().Be(25);
        summary.AverageLatencyMs.Should().Be(42.5);
        summary.ActiveClients.Should().Be(7);
        summary.From.Should().Be(from);
        summary.To.Should().Be(to);
    }

    [Fact]
    public async Task GetTopEndpointsAsync_ShouldMapReadModels()
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var lastActivity = DateTimeOffset.UtcNow.AddMinutes(-5);
        _repository.GetTopEndpointsAsync(5, from, Arg.Any<CancellationToken>())
            .Returns(new List<EndpointSummary>
            {
                new("/api/users", "GET", 50, 10, 18.2, "fixed-window", lastActivity)
            });

        var result = await _sut.GetTopEndpointsAsync(5, from, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Path.Should().Be("/api/users");
        result[0].Method.Should().Be("GET");
        result[0].RequestCount.Should().Be(50);
        result[0].BlockedCount.Should().Be(10);
        result[0].AverageLatencyMs.Should().Be(18.2);
        result[0].PolicyName.Should().Be("fixed-window");
        result[0].LastActivity.Should().Be(lastActivity);
    }

    [Fact]
    public async Task GetTopClientsAsync_ShouldMapFirstAndLastSeen()
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var firstSeen = DateTimeOffset.UtcNow.AddHours(-3);
        var lastSeen = DateTimeOffset.UtcNow.AddMinutes(-1);
        _repository.GetTopClientsAsync(5, from, Arg.Any<CancellationToken>())
            .Returns(new List<ClientSummary>
            {
                new("10.0.0.1", 40, 8, firstSeen, lastSeen)
            });

        var result = await _sut.GetTopClientsAsync(5, from, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].ClientIdentifier.Should().Be("10.0.0.1");
        result[0].FirstSeen.Should().Be(firstSeen);
        result[0].LastSeen.Should().Be(lastSeen);
    }

    [Fact]
    public async Task GetObservedPoliciesAsync_ShouldMapPolicySummaries()
    {
        var from = DateTimeOffset.UtcNow.AddHours(-24);
        _repository.GetObservedPoliciesAsync(from, Arg.Any<CancellationToken>())
            .Returns(new List<PolicySummary>
            {
                new("fixed-window", 120, 30)
            });

        var result = await _sut.GetObservedPoliciesAsync(from, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Name.Should().Be("fixed-window");
        result[0].TotalRequests.Should().Be(120);
        result[0].BlockedCount.Should().Be(30);
        result[0].BlockRatePercent.Should().Be(25);
    }
}
