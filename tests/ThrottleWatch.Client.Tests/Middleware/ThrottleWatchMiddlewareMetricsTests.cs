using System.Diagnostics.Metrics;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ThrottleWatch.Client.Configuration;
using ThrottleWatch.Client.Http;
using ThrottleWatch.Client.Metrics;
using ThrottleWatch.Client.Middleware;
using ThrottleWatch.Client.Queue;

namespace ThrottleWatch.Client.Tests.Middleware;

public sealed class ThrottleWatchMiddlewareMetricsTests : IDisposable
{
    private readonly MeterListener _listener = new();
    private readonly ClientMetrics _metrics;
    private readonly List<string> _instruments = [];

    public ThrottleWatchMiddlewareMetricsTests()
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == ClientMetrics.MeterName)
                listener.EnableMeasurementEvents(instrument);
        };

        _listener.SetMeasurementEventCallback<long>((instrument, _, tags, _) =>
        {
            lock (_instruments)
                _instruments.Add($"{instrument.Name}:{GetTag(tags, ClientMetrics.BlockedTag)}");
        });

        _listener.Start();
        _metrics = new ClientMetrics(new TestMeterFactory());
    }

    [Fact]
    public async Task InvokeAsync_WhenCaptureOnlyBlocked_ShouldStillRecordAllowedRequest()
    {
        var options = new ThrottleWatchOptions { CaptureOnlyBlocked = true };
        var buffer = new LocalMetricBuffer();
        var middleware = new ThrottleWatchMiddleware(ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;

        await middleware.InvokeAsync(
            context,
            buffer,
            _metrics,
            new TestOptionsMonitor<ThrottleWatchOptions>(options),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        buffer.Count.Should().Be(0);
        lock (_instruments)
            _instruments.Should().Contain($"{ClientMetrics.RequestsInstrument}:false");
    }

    [Fact]
    public async Task InvokeAsync_WhenStatusIs429_ShouldRecordBlocked()
    {
        var middleware = new ThrottleWatchMiddleware(ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;

        await middleware.InvokeAsync(
            context,
            new LocalMetricBuffer(),
            _metrics,
            new TestOptionsMonitor<ThrottleWatchOptions>(new ThrottleWatchOptions()),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        lock (_instruments)
            _instruments.Should().Contain($"{ClientMetrics.RequestsInstrument}:true");
    }

    [Fact]
    public async Task InvokeAsync_WhenBufferFull_ShouldRecordDrop()
    {
        var buffer = new LocalMetricBuffer(capacity: 2);
        var payload = new MetricPayload("/full", "GET", 200, 1, DateTimeOffset.UtcNow);
        buffer.TryEnqueue(payload).Should().BeTrue();
        buffer.TryEnqueue(payload).Should().BeTrue();
        buffer.TryEnqueue(payload).Should().BeFalse();

        var middleware = new ThrottleWatchMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            new DefaultHttpContext(),
            buffer,
            _metrics,
            new TestOptionsMonitor<ThrottleWatchOptions>(new ThrottleWatchOptions()),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        buffer.Count.Should().Be(2);
        lock (_instruments)
            _instruments.Should().Contain($"{ClientMetrics.DroppedInstrument}:");
    }

    public void Dispose() => _listener.Dispose();

    private static string? GetTag(ReadOnlySpan<KeyValuePair<string, object?>> tags, string name)
    {
        foreach (var tag in tags)
        {
            if (tag.Key == name)
                return tag.Value?.ToString();
        }

        return null;
    }
}
