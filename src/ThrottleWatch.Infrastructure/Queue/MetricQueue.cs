using System.Threading.Channels;
using ThrottleWatch.Application.Interfaces;
using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Infrastructure.Queue;

public sealed class MetricQueue : IMetricQueue
{
    private readonly Channel<MetricEntry> _channel;

    public MetricQueue(int capacity = 10_000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = false,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<MetricEntry>(options);
    }

    public int Count => _channel.Reader.Count;

    public bool TryEnqueue(MetricEntry entry) => _channel.Writer.TryWrite(entry);

    public IReadOnlyList<MetricEntry> DequeueBatch(int maxCount)
    {
        var batch = new List<MetricEntry>(Math.Min(maxCount, 256));

        while (batch.Count < maxCount && _channel.Reader.TryRead(out var entry))
            batch.Add(entry);

        return batch;
    }
}
