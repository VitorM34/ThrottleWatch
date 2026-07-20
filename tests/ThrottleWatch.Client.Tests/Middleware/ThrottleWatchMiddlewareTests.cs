using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ThrottleWatch.Client.Configuration;
using ThrottleWatch.Client.Middleware;
using ThrottleWatch.Client.Queue;

namespace ThrottleWatch.Client.Tests.Middleware;

public sealed class ThrottleWatchMiddlewareTests
{
    [Fact]
    public void Constructor_WithNullNext_ShouldThrow()
    {
        var act = () => new ThrottleWatchMiddleware(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextDelegate()
    {
        var called = false;
        var middleware = new ThrottleWatchMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            new DefaultHttpContext(),
            new LocalMetricBuffer(),
            CreateOptionsMonitor(new ThrottleWatchOptions()),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldEnqueueMetric_WithRequestMetadata()
    {
        var buffer = new LocalMetricBuffer();
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/orders";
        context.Request.Headers["X-Api-Key"] = "key-123";
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.10");

        var middleware = new ThrottleWatchMiddleware(async ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            ctx.Response.Headers["X-RateLimit-Policy"] = "fixed-window";
            await Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            context,
            buffer,
            CreateOptionsMonitor(new ThrottleWatchOptions()),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        var batch = buffer.DequeueBatch(1);
        batch.Should().HaveCount(1);
        batch[0].Path.Should().Be("/api/orders");
        batch[0].Method.Should().Be("POST");
        batch[0].StatusCode.Should().Be(200);
        batch[0].ApiKey.Should().Be("key-123");
        batch[0].PolicyName.Should().Be("fixed-window");
        batch[0].ClientIp.Should().Be("203.0.113.10");
        batch[0].DurationMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task InvokeAsync_WhenCaptureOnlyBlocked_AndStatusIsNot429_ShouldNotEnqueue()
    {
        var buffer = new LocalMetricBuffer();
        var options = new ThrottleWatchOptions { CaptureOnlyBlocked = true };
        var middleware = new ThrottleWatchMiddleware(ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            new DefaultHttpContext(),
            buffer,
            CreateOptionsMonitor(options),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        buffer.Count.Should().Be(0);
    }

    [Fact]
    public async Task InvokeAsync_WhenCaptureOnlyBlocked_AndStatusIs429_ShouldEnqueue()
    {
        var buffer = new LocalMetricBuffer();
        var options = new ThrottleWatchOptions { CaptureOnlyBlocked = true };
        var middleware = new ThrottleWatchMiddleware(ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(
            new DefaultHttpContext(),
            buffer,
            CreateOptionsMonitor(options),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        buffer.DequeueBatch(1).Should().ContainSingle(m => m.StatusCode == 429);
    }

    [Fact]
    public async Task InvokeAsync_WhenCaptureClientIpDisabled_ShouldNotSetClientIp()
    {
        var buffer = new LocalMetricBuffer();
        var options = new ThrottleWatchOptions { CaptureClientIp = false };
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Loopback;

        var middleware = new ThrottleWatchMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(
            context,
            buffer,
            CreateOptionsMonitor(options),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        buffer.DequeueBatch(1)[0].ClientIp.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrows_ShouldPropagate_AndStillCapture()
    {
        var buffer = new LocalMetricBuffer();
        var middleware = new ThrottleWatchMiddleware(_ =>
            throw new InvalidOperationException("boom"));

        var context = new DefaultHttpContext();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var act = () => middleware.InvokeAsync(
            context,
            buffer,
            CreateOptionsMonitor(new ThrottleWatchOptions()),
            NullLogger<ThrottleWatchMiddleware>.Instance);

        await act.Should().ThrowAsync<InvalidOperationException>();
        buffer.Count.Should().Be(1);
    }

    private static IOptionsMonitor<ThrottleWatchOptions> CreateOptionsMonitor(ThrottleWatchOptions options) =>
        new TestOptionsMonitor<ThrottleWatchOptions>(options);
}
