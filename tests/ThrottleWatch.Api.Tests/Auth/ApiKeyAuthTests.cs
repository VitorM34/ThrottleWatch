using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ThrottleWatch.Application.DTOs.Metrics;

namespace ThrottleWatch.Api.Tests;

[Collection(nameof(ApiCollection))]
public sealed class ApiKeyAuthTests
{
    private readonly ThrottleWatchApiFactory _factory;

    public ApiKeyAuthTests(ThrottleWatchApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetHealth_WithoutApiKey_ShouldReturnOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSummary_WithoutApiKey_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/metrics/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSummary_WithWrongApiKey_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(
            ThrottleWatchApiFactory.TestApiKeyHeader,
            "wrong-key");

        var response = await client.GetAsync("/api/metrics/summary");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostMetrics_WithoutApiKey_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();
        var batch = new[]
        {
            new IngestMetricDto("/api/auth-demo", "GET", 200, 5, DateTimeOffset.UtcNow, "127.0.0.1")
        };

        var response = await client.PostAsJsonAsync("/api/metrics", batch);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostMetrics_WithValidApiKey_ShouldReturnAccepted()
    {
        var client = _factory.CreateAuthenticatedClient();
        var batch = new[]
        {
            new IngestMetricDto("/api/auth-demo", "GET", 200, 5, DateTimeOffset.UtcNow, "127.0.0.1")
        };

        var response = await client.PostAsJsonAsync("/api/metrics", batch);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }
}
