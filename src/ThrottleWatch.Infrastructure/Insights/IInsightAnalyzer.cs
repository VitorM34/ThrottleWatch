using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Infrastructure.Insights;

public interface IInsightAnalyzer
{
    InsightType Type { get; }

    Task<IReadOnlyList<Insight>> AnalyzeAsync(IMetricsRepository metrics, CancellationToken ct);
}
