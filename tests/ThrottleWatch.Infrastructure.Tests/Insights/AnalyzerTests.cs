using FluentAssertions;
using NSubstitute;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Domain.ReadModels;
using ThrottleWatch.Infrastructure.Insights.Analyzers;

namespace ThrottleWatch.Infrastructure.Tests.Insights;

public sealed class HighBlockRateAnalyzerTests
{
    private readonly IMetricsRepository _metrics = Substitute.For<IMetricsRepository>();
    private readonly HighBlockRateAnalyzer _sut = new();

    [Fact]
    public async Task AnalyzeAsync_WhenNoRequests_ShouldReturnEmpty()
    {
        _metrics.GetTotalRequestsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(0);

        var result = await _sut.AnalyzeAsync(_metrics, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzeAsync_WhenBlockRateBelowThreshold_ShouldReturnEmpty()
    {
        _metrics.GetTotalRequestsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(100);
        _metrics.GetTotalBlockedAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(10);

        var result = await _sut.AnalyzeAsync(_metrics, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AnalyzeAsync_WhenBlockRateAboveThreshold_ShouldCreateInsight()
    {
        _metrics.GetTotalRequestsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(100);
        _metrics.GetTotalBlockedAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(30);

        var result = await _sut.AnalyzeAsync(_metrics, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Type.Should().Be(InsightType.HighBlockRate);
        result[0].Severity.Should().Be(AlertSeverity.Warning);
    }
}

public sealed class SuspiciousClientAnalyzerTests
{
    private readonly IMetricsRepository _metrics = Substitute.For<IMetricsRepository>();
    private readonly SuspiciousClientAnalyzer _sut = new();

    [Fact]
    public async Task AnalyzeAsync_WhenClientExceedsBlockedThreshold_ShouldCreateInsight()
    {
        _metrics.GetTopClientsAsync(20, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<ClientSummary>
            {
                new("10.0.0.1", 80, 55)
            });

        var result = await _sut.AnalyzeAsync(_metrics, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Type.Should().Be(InsightType.SuspiciousClient);
        result[0].AffectedResource.Should().Be("10.0.0.1");
        result[0].Severity.Should().Be(AlertSeverity.Critical);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenNoSuspiciousClients_ShouldReturnEmpty()
    {
        _metrics.GetTopClientsAsync(20, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<ClientSummary>
            {
                new("10.0.0.1", 20, 5)
            });

        var result = await _sut.AnalyzeAsync(_metrics, CancellationToken.None);

        result.Should().BeEmpty();
    }
}

public sealed class MisconfiguredPolicyAnalyzerTests
{
    private readonly IMetricsRepository _metrics = Substitute.For<IMetricsRepository>();
    private readonly MisconfiguredPolicyAnalyzer _sut = new();

    [Fact]
    public async Task AnalyzeAsync_WhenEndpointBlockRateAbove90_ShouldCreateInsight()
    {
        _metrics.GetTopEndpointsAsync(20, Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<EndpointSummary>
            {
                new("/api/login", "POST", 100, 95)
            });

        var result = await _sut.AnalyzeAsync(_metrics, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Type.Should().Be(InsightType.MisconfiguredPolicy);
        result[0].AffectedResource.Should().Be("POST /api/login");
    }
}

public sealed class PeakHoursAnalyzerTests
{
    private readonly IMetricsRepository _metrics = Substitute.For<IMetricsRepository>();
    private readonly PeakHoursAnalyzer _sut = new();

    [Fact]
    public async Task AnalyzeAsync_WhenLastHourIsSpike_ShouldCreateInsight()
    {
        _metrics.GetTotalRequestsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var from = callInfo.ArgAt<DateTimeOffset>(0);
                var to = callInfo.ArgAt<DateTimeOffset>(1);
                var hours = (to - from).TotalHours;
                return hours <= 1.1 ? 1000L : 230L;
            });

        var result = await _sut.AnalyzeAsync(_metrics, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Type.Should().Be(InsightType.PeakHours);
        result[0].Severity.Should().Be(AlertSeverity.Info);
    }

    [Fact]
    public async Task AnalyzeAsync_WhenNoSpike_ShouldReturnEmpty()
    {
        _metrics.GetTotalRequestsAsync(Arg.Any<DateTimeOffset>(), Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var from = callInfo.ArgAt<DateTimeOffset>(0);
                var to = callInfo.ArgAt<DateTimeOffset>(1);
                var hours = (to - from).TotalHours;
                return hours <= 1.1 ? 10L : 2300L;
            });

        var result = await _sut.AnalyzeAsync(_metrics, CancellationToken.None);

        result.Should().BeEmpty();
    }
}
