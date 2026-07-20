using FluentAssertions;
using ThrottleWatch.Client.Http;
using ThrottleWatch.Client.Queue;

namespace ThrottleWatch.Client.Tests.Queue;

public sealed class LocalMetricBufferTests
{
    private static MetricPayload CreatePayload(string path = "/api/test") =>
        new(path, "GET", 200, 12.5, DateTimeOffset.UtcNow);

    [Fact]
    public void TryEnqueue_ShouldIncreaseCount()
    {
        var buffer = new LocalMetricBuffer(capacity: 10);

        buffer.TryEnqueue(CreatePayload()).Should().BeTrue();

        buffer.Count.Should().Be(1);
    }

    [Fact]
    public void DequeueBatch_ShouldReturnEnqueuedItems_InOrder()
    {
        var buffer = new LocalMetricBuffer(capacity: 10);
        buffer.TryEnqueue(CreatePayload("/a"));
        buffer.TryEnqueue(CreatePayload("/b"));
        buffer.TryEnqueue(CreatePayload("/c"));

        var batch = buffer.DequeueBatch(2);

        batch.Should().HaveCount(2);
        batch[0].Path.Should().Be("/a");
        batch[1].Path.Should().Be("/b");
        buffer.Count.Should().Be(1);
    }

    [Fact]
    public void DequeueBatch_WhenEmpty_ShouldReturnEmptyList()
    {
        var buffer = new LocalMetricBuffer(capacity: 10);

        buffer.DequeueBatch(5).Should().BeEmpty();
    }

    [Fact]
    public void TryEnqueue_WhenFull_ShouldDropNewItem_AndKeepCapacity()
    {
        var buffer = new LocalMetricBuffer(capacity: 2);
        buffer.TryEnqueue(CreatePayload("/1")).Should().BeTrue();
        buffer.TryEnqueue(CreatePayload("/2")).Should().BeTrue();

        buffer.TryEnqueue(CreatePayload("/3"));

        buffer.Count.Should().Be(2);
        var batch = buffer.DequeueBatch(10);
        batch.Should().HaveCount(2);
        batch.Select(x => x.Path).Should().Equal("/1", "/2");
    }
}
