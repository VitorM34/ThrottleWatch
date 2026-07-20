using System.Threading.Channels;
using ThrottleWatch.Client.Http;

namespace ThrottleWatch.Client.Queue;

public sealed class LocalMetricBuffer
{
    private readonly Channel<MetricPayload> _channel;

    public LocalMetricBuffer(int capacity = 10_000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = false,
            SingleWriter = false
        };

        _channel = Channel.CreateBounded<MetricPayload>(options);
    }

    public int Count => _channel.Reader.Count;

    public bool TryEnqueue(MetricPayload payload) => _channel.Writer.TryWrite(payload);

    public IReadOnlyList<MetricPayload> DequeueBatch(int maxCount)
    {
        var batch = new List<MetricPayload>(Math.Min(maxCount, 256));

        while (batch.Count < maxCount && _channel.Reader.TryRead(out var payload))
            batch.Add(payload);

        return batch;
    }
}
