using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Insights;

namespace ThrottleWatch.Infrastructure.Insights.Analyzers;

public sealed class PeakHoursAnalyzer : IInsightAnalyzer
{
    private const double SpikeMultiplier = 2.0;

    public InsightType Type => InsightType.PeakHours;

    public async Task<IReadOnlyList<Insight>> AnalyzeAsync(IMetricsRepository metrics, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var lastHourFrom = now.AddHours(-1);
        var historyFrom = now.AddHours(-24);

        var lastHourTotal = await metrics.GetTotalRequestsAsync(lastHourFrom, now, ct);
        if (lastHourTotal == 0)
            return [];

        var historyTotal = await metrics.GetTotalRequestsAsync(historyFrom, lastHourFrom, ct);
        if (historyTotal == 0)
            return [];

        var averagePerHour = historyTotal / 23d;
        if (averagePerHour <= 0)
            return [];

        var ratio = lastHourTotal / averagePerHour;
        if (ratio < SpikeMultiplier)
            return [];

        var insight = Insight.Create(
            InsightType.PeakHours,
            "Traffic peak detected",
            $"Last hour traffic ({lastHourTotal:N0} requests) is {ratio:F1}x the previous 23h hourly average. Expect higher throttle pressure.",
            AlertSeverity.Info);

        return [insight];
    }
}
