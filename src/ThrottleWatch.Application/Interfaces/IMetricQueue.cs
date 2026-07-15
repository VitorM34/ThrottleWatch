using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Application.Interfaces;

public interface IMetricQueue
{
    bool TryEnqueue(MetricEntry entry);
    IReadOnlyList<MetricEntry> DequeueBatch(int maxCount);
    int Count { get; }
}
