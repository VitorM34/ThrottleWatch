using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Interfaces;
using ThrottleWatch.Infrastructure.Insights;

namespace ThrottleWatch.Infrastructure.Insights.Analyzers;

public sealed class SuspiciousClientAnalyzer : IInsightAnalyzer
{
    private const long BlockedThreshold = 50;

    public InsightType Type => InsightType.SuspiciousClient;

    public async Task<IReadOnlyList<Insight>> AnalyzeAsync(IMetricsRepository metrics, CancellationToken ct)
    {
        var from = DateTimeOffset.UtcNow.AddMinutes(-5);
        var clients = await metrics.GetTopClientsAsync(20, from, ct);

        var insights = new List<Insight>();
        foreach (var client in clients.Where(c => c.BlockedCount > BlockedThreshold))
        {
            insights.Add(Insight.Create(
                InsightType.SuspiciousClient,
                $"Suspicious client activity: {client.ClientIdentifier}",
                $"Client '{client.ClientIdentifier}' accumulated {client.BlockedCount} blocked requests in the last 5 minutes.",
                AlertSeverity.Critical,
                client.ClientIdentifier));
        }

        return insights;
    }
}
