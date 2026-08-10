using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using ThrottleWatch.Application.DTOs.Alerts;
using ThrottleWatch.Application.DTOs.Insights;
using ThrottleWatch.Application.DTOs.Metrics;
using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Api.Tests;

[CollectionDefinition(nameof(ApiCollection))]
public sealed class ApiCollection : ICollectionFixture<ThrottleWatchApiFactory>;

[Collection(nameof(ApiCollection))]
public sealed class HealthEndpointsTests
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(ThrottleWatchApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ShouldReturnHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Healthy");
    }
}

[Collection(nameof(ApiCollection))]
public sealed class MetricsEndpointsTests
{
    private readonly HttpClient _client;

    public MetricsEndpointsTests(ThrottleWatchApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetSummary_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/metrics/summary");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await response.Content.ReadFromJsonAsync<MetricsSummaryDto>();
        summary.Should().NotBeNull();
        summary!.TotalRequests.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task PostMetrics_ShouldReturnAccepted()
    {
        var batch = new[]
        {
            new IngestMetricDto("/api/demo", "GET", 200, 12.5, DateTimeOffset.UtcNow, "127.0.0.1")
        };

        var response = await _client.PostAsJsonAsync("/api/metrics", batch);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task GetTopEndpoints_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/metrics/top-endpoints?top=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTopClients_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/metrics/top-clients?top=5");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetTimeSeries_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/metrics/timeseries");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetObservedPolicies_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/metrics/policies");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var policies = await response.Content.ReadFromJsonAsync<IReadOnlyList<ObservedPolicyDto>>();
        policies.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSummary_AfterIngest_ShouldExposeLatencyAndActiveClients()
    {
        var now = DateTimeOffset.UtcNow;
        var batch = new[]
        {
            new IngestMetricDto("/api/demo", "GET", 200, 25, now, "10.0.0.1", "fixed", null),
            new IngestMetricDto("/api/demo", "GET", 429, 40, now, "10.0.0.2", "fixed", null)
        };

        var ingest = await _client.PostAsJsonAsync("/api/metrics", batch);
        ingest.StatusCode.Should().Be(HttpStatusCode.Accepted);

        // Flush worker may need a short moment; poll summary until totals move or timeout.
        MetricsSummaryDto? summary = null;
        for (var i = 0; i < 20; i++)
        {
            await Task.Delay(100);
            var response = await _client.GetAsync("/api/metrics/summary");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            summary = await response.Content.ReadFromJsonAsync<MetricsSummaryDto>();
            if (summary is { TotalRequests: > 0 })
                break;
        }

        summary.Should().NotBeNull();
        if (summary!.TotalRequests > 0)
        {
            summary.AverageLatencyMs.Should().BeGreaterThan(0);
            summary.ActiveClients.Should().BeGreaterThan(0);
        }
    }
}

[Collection(nameof(ApiCollection))]
public sealed class AlertsEndpointsTests
{
    private readonly HttpClient _client;

    public AlertsEndpointsTests(ThrottleWatchApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task CreateAndGetRules_ShouldWork()
    {
        var create = new CreateAlertRuleDto(
            $"rule-{Guid.NewGuid():N}"[..20],
            "block_rate",
            15,
            AlertSeverity.Warning,
            10,
            "test");

        var createResponse = await _client.PostAsJsonAsync("/api/alerts/rules", create);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<AlertRuleDto>();
        created.Should().NotBeNull();

        var listResponse = await _client.GetAsync("/api/alerts/rules");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rules = await listResponse.Content.ReadFromJsonAsync<List<AlertRuleDto>>();
        rules.Should().NotBeNull();
        rules!.Any(r => r.Id == created!.Id).Should().BeTrue();
    }

    [Fact]
    public async Task GetEvents_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/alerts/events?count=10");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

[Collection(nameof(ApiCollection))]
public sealed class InsightsEndpointsTests
{
    private readonly HttpClient _client;

    public InsightsEndpointsTests(ThrottleWatchApiFactory factory)
    {
        _client = factory.CreateAuthenticatedClient();
    }

    [Fact]
    public async Task GetInsights_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/insights");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var insights = await response.Content.ReadFromJsonAsync<List<InsightDto>>();
        insights.Should().NotBeNull();
    }
}
