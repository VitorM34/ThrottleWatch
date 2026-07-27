using System.Diagnostics.Metrics;
using ThrottleWatch.Application.Interfaces;

namespace ThrottleWatch.Infrastructure.Telemetry;

public sealed class ThrottleWatchMetrics : IOperationalMetrics
{
    public const string MeterName = "ThrottleWatch";

    private readonly Counter<long> _requestsProcessed;
    private readonly Counter<long> _batchesDropped;
    private readonly Histogram<double> _batchDurationMs;

    public ThrottleWatchMetrics(IMetricQueue queue)
    {
        var meter = new Meter(MeterName, "1.0.0");

        _ = meter.CreateObservableGauge(
            "throttlewatch.queue.depth",
            () => queue.Count,
            unit: "{items}",
            description: "Current number of metric entries in the in-memory queue");

        _requestsProcessed = meter.CreateCounter<long>(
            "throttlewatch.requests.processed",
            unit: "{requests}",
            description: "Total metric entries persisted from the queue");

        _batchesDropped = meter.CreateCounter<long>(
            "throttlewatch.batches.dropped",
            unit: "{entries}",
            description: "Metric entries dropped because the queue was full");

        _batchDurationMs = meter.CreateHistogram<double>(
            "throttlewatch.batch.duration",
            unit: "ms",
            description: "Time to persist a batch of metric entries");
    }

    public void RecordBatchProcessed(int count, double durationMs)
    {
        _requestsProcessed.Add(count);
        _batchDurationMs.Record(durationMs);
    }

    public void RecordDrop() => _batchesDropped.Add(1);
}
