using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Insights;

namespace ThrottleWatch.Infrastructure.Insights.Analyzers;

public sealed class HighBlockRateAnalyzer : IInsightAnalyzer
{
    private const double ThresholdPercent = 20d;

    public InsightType Type => InsightType.HighBlockRate;

    public async Task<IReadOnlyList<Insight>> AnalyzeAsync(IMetricsRepository metrics, CancellationToken ct)
    {
        var to = DateTimeOffset.UtcNow;
        var from = to.AddMinutes(-15);

        var total = await metrics.GetTotalRequestsAsync(from, to, ct);
        if (total == 0)
            return [];

        var blocked = await metrics.GetTotalBlockedAsync(from, to, ct);
        var blockRate = (double)blocked / total * 100;

        if (blockRate < ThresholdPercent)
            return [];

        var insight = Insight.Create(
            InsightType.HighBlockRate,
            "High block rate detected",
            $"Block rate is {blockRate:F1}% in the last 15 minutes. Consider adjusting rate limit policies.",
            AlertSeverity.Warning);

        return [insight];
    }
}
