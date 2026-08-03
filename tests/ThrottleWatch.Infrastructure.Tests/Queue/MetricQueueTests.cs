using FluentAssertions;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Infrastructure.Queue;

namespace ThrottleWatch.Infrastructure.Tests.Queue;

public sealed class MetricQueueTests
{
    [Fact]
    public void TryEnqueue_WhenCapacityAvailable_ShouldReturnTrueAndIncreaseCount()
    {
        var queue = new MetricQueue(capacity: 10);
        var entry = MetricEntry.Create("/api/test", "GET", 200, 10, DateTimeOffset.UtcNow);

        var accepted = queue.TryEnqueue(entry);

        accepted.Should().BeTrue();
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void TryEnqueue_WhenFull_ShouldDropWriteAndReturnFalse()
    {
        var queue = new MetricQueue(capacity: 1);
        queue.TryEnqueue(MetricEntry.Create("/a", "GET", 200, 1, DateTimeOffset.UtcNow)).Should().BeTrue();

        var accepted = queue.TryEnqueue(MetricEntry.Create("/b", "GET", 200, 1, DateTimeOffset.UtcNow));

        accepted.Should().BeFalse();
        queue.Count.Should().Be(1);
    }

    [Fact]
    public void DequeueBatch_ShouldReturnQueuedEntriesUpToMax()
    {
        var queue = new MetricQueue(capacity: 10);
        queue.TryEnqueue(MetricEntry.Create("/a", "GET", 200, 1, DateTimeOffset.UtcNow));
        queue.TryEnqueue(MetricEntry.Create("/b", "GET", 429, 2, DateTimeOffset.UtcNow));
        queue.TryEnqueue(MetricEntry.Create("/c", "POST", 200, 3, DateTimeOffset.UtcNow));

        var batch = queue.DequeueBatch(2);

        batch.Should().HaveCount(2);
        queue.Count.Should().Be(1);
    }
}
