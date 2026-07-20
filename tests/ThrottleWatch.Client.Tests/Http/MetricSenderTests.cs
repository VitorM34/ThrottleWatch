using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using ThrottleWatch.Client.Configuration;
using ThrottleWatch.Client.Http;
using ThrottleWatch.Client.Queue;

namespace ThrottleWatch.Client.Tests.Http;

public sealed class MetricSenderTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldPostBufferedMetrics_AsJsonBatch()
    {
        var buffer = new LocalMetricBuffer();
        buffer.TryEnqueue(new MetricPayload("/api/a", "GET", 200, 10, DateTimeOffset.UtcNow));
        buffer.TryEnqueue(new MetricPayload("/api/b", "POST", 429, 5, DateTimeOffset.UtcNow));

        var handler = new RecordingHandler(HttpStatusCode.Accepted);
        var factory = new FakeHttpClientFactory(handler, new Uri("http://throttlewatch.test/"));
        var options = new ThrottleWatchOptions
        {
            ApiBaseUrl = "http://throttlewatch.test",
            BatchSize = 50,
            FlushIntervalMilliseconds = 100
        };

        var sender = new MetricSender(
            buffer,
            factory,
            new TestOptionsMonitor<ThrottleWatchOptions>(options),
            NullLogger<MetricSender>.Instance);

        await sender.StartAsync(CancellationToken.None);

        try
        {
            await WaitForAsync(() => handler.RequestCount > 0, TimeSpan.FromSeconds(2));
        }
        finally
        {
            await sender.StopAsync(CancellationToken.None);
        }

        handler.RequestCount.Should().BeGreaterThanOrEqualTo(1);
        handler.LastRequestUri!.AbsoluteUri.Should().Contain("api/metrics");
        handler.LastRequestMethod.Should().Be(HttpMethod.Post);

        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().Should().Be(2);
        doc.RootElement[0].GetProperty("path").GetString().Should().Be("/api/a");
        doc.RootElement[1].GetProperty("statusCode").GetInt32().Should().Be(429);
        buffer.Count.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WhenBufferEmpty_ShouldNotPost()
    {
        var handler = new RecordingHandler(HttpStatusCode.Accepted);
        var factory = new FakeHttpClientFactory(handler, new Uri("http://throttlewatch.test/"));
        var options = new ThrottleWatchOptions
        {
            ApiBaseUrl = "http://throttlewatch.test",
            FlushIntervalMilliseconds = 100
        };

        var sender = new MetricSender(
            new LocalMetricBuffer(),
            factory,
            new TestOptionsMonitor<ThrottleWatchOptions>(options),
            NullLogger<MetricSender>.Instance);

        await sender.StartAsync(CancellationToken.None);
        await Task.Delay(150);
        await sender.StopAsync(CancellationToken.None);

        handler.RequestCount.Should().Be(0);
    }

    private static async Task WaitForAsync(Func<bool> condition, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            if (condition())
                return;
            await Task.Delay(20);
        }

        throw new TimeoutException("Condition was not met within the timeout.");
    }

    private sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public FakeHttpClientFactory(HttpMessageHandler handler, Uri baseAddress)
        {
            _client = new HttpClient(handler) { BaseAddress = baseAddress };
        }

        public HttpClient CreateClient(string name) => _client;
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private int _requestCount;

        public RecordingHandler(HttpStatusCode statusCode) => _statusCode = statusCode;

        public int RequestCount => Volatile.Read(ref _requestCount);
        public Uri? LastRequestUri { get; private set; }
        public HttpMethod? LastRequestMethod { get; private set; }
        public string? LastRequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            LastRequestMethod = request.Method;
            LastRequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            Interlocked.Increment(ref _requestCount);
            return new HttpResponseMessage(_statusCode);
        }
    }
}
