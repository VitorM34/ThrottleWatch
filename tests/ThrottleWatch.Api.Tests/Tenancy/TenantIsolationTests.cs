using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThrottleWatch.Application.DTOs.Alerts;
using ThrottleWatch.Application.DTOs.Metrics;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Infrastructure.Persistence;

namespace ThrottleWatch.Api.Tests;

[Collection(nameof(ApiCollection))]
public sealed class TenantIsolationTests
{
    public const string TenantBKey = "test-tenant-b-key";

    private readonly ThrottleWatchApiFactory _factory;

    public TenantIsolationTests(ThrottleWatchApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAlertRules_WithTenantAKey_ShouldNotReturnTenantBRules()
    {
        using var factory = CreateMultiTenantFactory();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var nameA = $"rule-a-{suffix}";
        var nameB = $"rule-b-{suffix}";

        var clientA = CreateClient(factory, ThrottleWatchApiFactory.TestApiKey);
        var clientB = CreateClient(factory, TenantBKey);

        (await clientA.PostAsJsonAsync("/api/alerts/rules", NewRule(nameA)))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await clientB.PostAsJsonAsync("/api/alerts/rules", NewRule(nameB)))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var rulesA = await clientA.GetFromJsonAsync<List<AlertRuleDto>>("/api/alerts/rules");
        var rulesB = await clientB.GetFromJsonAsync<List<AlertRuleDto>>("/api/alerts/rules");

        rulesA.Should().Contain(r => r.Name == nameA);
        rulesA.Should().NotContain(r => r.Name == nameB);
        rulesB.Should().Contain(r => r.Name == nameB);
        rulesB.Should().NotContain(r => r.Name == nameA);
    }

    [Fact]
    public async Task GetTopEndpoints_WithTenantAKey_ShouldNotReturnTenantBMetrics()
    {
        using var factory = CreateMultiTenantFactory();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var pathA = $"/api/tenant-a-{suffix}";
        var pathB = $"/api/tenant-b-{suffix}";

        await SeedMetricAsync(factory, "tenant-a", pathA);
        await SeedMetricAsync(factory, "tenant-b", pathB);

        var clientA = CreateClient(factory, ThrottleWatchApiFactory.TestApiKey);
        var clientB = CreateClient(factory, TenantBKey);

        var endpointsA = await clientA.GetFromJsonAsync<List<TopEndpointDto>>("/api/metrics/top-endpoints?top=50");
        var endpointsB = await clientB.GetFromJsonAsync<List<TopEndpointDto>>("/api/metrics/top-endpoints?top=50");

        endpointsA.Should().Contain(e => e.Path == pathA);
        endpointsA.Should().NotContain(e => e.Path == pathB);
        endpointsB.Should().Contain(e => e.Path == pathB);
        endpointsB.Should().NotContain(e => e.Path == pathA);
    }

    [Fact]
    public async Task PostMetrics_WithoutApiKey_ShouldReturnUnauthorized()
    {
        using var factory = CreateMultiTenantFactory();
        var client = factory.CreateClient();
        var batch = new[]
        {
            new IngestMetricDto("/api/isolated", "GET", 200, 5, DateTimeOffset.UtcNow)
        };

        var response = await client.PostAsJsonAsync("/api/metrics", batch);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private WebApplicationFactory<Program> CreateMultiTenantFactory()
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ThrottleWatch:Security:TenantId"] = "tenant-a",
                    ["ThrottleWatch:Security:Tenants:0:ApiKey"] = TenantBKey,
                    ["ThrottleWatch:Security:Tenants:0:TenantId"] = "tenant-b"
                });
            });
        });
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory, string apiKey)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(
            ThrottleWatchApiFactory.TestApiKeyHeader,
            apiKey);
        return client;
    }

    private static CreateAlertRuleDto NewRule(string name) =>
        new(name, "block_rate", 80, AlertSeverity.Warning, 15, "isolation");

    private static async Task SeedMetricAsync(
        WebApplicationFactory<Program> factory,
        string tenantId,
        string path)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<ITenantContext>().Set(tenantId);
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.MetricEntries.Add(MetricEntry.Create(
            path,
            "GET",
            200,
            10,
            DateTimeOffset.UtcNow,
            "127.0.0.1",
            "fixed",
            null,
            tenantId));
        await db.SaveChangesAsync();
    }
}
