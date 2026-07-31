using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Insights;

namespace ThrottleWatch.Infrastructure.Insights.Analyzers;

public sealed class MisconfiguredPolicyAnalyzer : IInsightAnalyzer
{
    private const double ThresholdPercent = 90d;

    public InsightType Type => InsightType.MisconfiguredPolicy;

    public async Task<IReadOnlyList<Insight>> AnalyzeAsync(IMetricsRepository metrics, CancellationToken ct)
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-30);
        var endpoints = await metrics.GetTopEndpointsAsync(20, from, ct);

        var insights = new List<Insight>();
        foreach (var endpoint in endpoints)
        {
            if (endpoint.RequestCount == 0)
                continue;

            var blockRate = (double)endpoint.BlockedCount / endpoint.RequestCount * 100;
            if (blockRate < ThresholdPercent)
                continue;

            var resource = $"{endpoint.Method} {endpoint.Path}";
            insights.Add(Insight.Create(
                InsightType.MisconfiguredPolicy,
                $"Possible misconfigured policy on {resource}",
                $"Endpoint {resource} blocked {blockRate:F1}% of requests in the last 30 minutes. Review the rate limit threshold.",
                AlertSeverity.Warning,
                resource));
        }

        return insights;
    }
}
