using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Configuration;
using ThrottleWatch.Infrastructure.Insights;

namespace ThrottleWatch.Infrastructure.Tests.Insights;

public sealed class InsightGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_WhenAnalyzerFails_ShouldContinueOtherAnalyzers()
    {
        var failing = Substitute.For<IInsightAnalyzer>();
        failing.Type.Returns(InsightType.UnusualPattern);
        failing.AnalyzeAsync(Arg.Any<IMetricsRepository>(), Arg.Any<CancellationToken>())
            .Returns<Task<IReadOnlyList<Insight>>>(_ => throw new InvalidOperationException("boom"));

        var ok = Substitute.For<IInsightAnalyzer>();
        ok.Type.Returns(InsightType.HighBlockRate);
        var insight = Insight.Create(
            InsightType.HighBlockRate,
            "High",
            "desc",
            AlertSeverity.Warning);
        ok.AnalyzeAsync(Arg.Any<IMetricsRepository>(), Arg.Any<CancellationToken>())
            .Returns(new List<Insight> { insight });

        var metrics = Substitute.For<IMetricsRepository>();
        var insights = Substitute.For<IInsightRepository>();
        insights.ExistsRecentAsync(
                Arg.Any<InsightType>(),
                Arg.Any<string?>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(false);

        var dispatcher = Substitute.For<IDomainEventDispatcher>();
        var options = Substitute.For<IOptionsMonitor<ThrottleWatchOptions>>();
        options.CurrentValue.Returns(new ThrottleWatchOptions
        {
            Insights = new InsightsOptions { DedupWindowMinutes = 60 }
        });

        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns("default");

        var sut = new InsightGenerator(
            [failing, ok],
            metrics,
            insights,
            dispatcher,
            tenantContext,
            options,
            Substitute.For<ILogger<InsightGenerator>>());

        var generated = await sut.GenerateAsync(CancellationToken.None);

        generated.Should().ContainSingle();
        await insights.Received(1).AddAsync(insight, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateAsync_WhenRecentExists_ShouldSkipPersist()
    {
        var analyzer = Substitute.For<IInsightAnalyzer>();
        var insight = Insight.Create(
            InsightType.PeakHours,
            "Peak",
            "desc",
            AlertSeverity.Info);
        analyzer.AnalyzeAsync(Arg.Any<IMetricsRepository>(), Arg.Any<CancellationToken>())
            .Returns(new List<Insight> { insight });

        var insights = Substitute.For<IInsightRepository>();
        insights.ExistsRecentAsync(
                InsightType.PeakHours,
                null,
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(true);

        var options = Substitute.For<IOptionsMonitor<ThrottleWatchOptions>>();
        options.CurrentValue.Returns(new ThrottleWatchOptions());

        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns("default");

        var sut = new InsightGenerator(
            [analyzer],
            Substitute.For<IMetricsRepository>(),
            insights,
            Substitute.For<IDomainEventDispatcher>(),
            tenantContext,
            options,
            Substitute.For<ILogger<InsightGenerator>>());

        var generated = await sut.GenerateAsync(CancellationToken.None);

        generated.Should().BeEmpty();
        await insights.DidNotReceive().AddAsync(Arg.Any<Insight>(), Arg.Any<CancellationToken>());
    }
}
