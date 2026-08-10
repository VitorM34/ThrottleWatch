using ThrottleWatch.Dashboard.DTOs;
using ThrottleWatch.Dashboard.Models;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.DTOs;

public sealed class DtoMappingTests
{
    [Fact]
    public void DashboardMetricsDto_MapsTo_Model_Correctly()
    {
        var dto = new DashboardMetricsDto(
            TotalRequests: 1000,
            BlockedRequests: 100,
            AllowedRequests: 900,
            RequestsPerSecond: 5.5,
            AverageLatencyMs: 42.3,
            ActiveClients: 15,
            UptimeSeconds: 3600
        );

        var model = dto.ToModel();

        Assert.Equal(1000, model.TotalRequests);
        Assert.Equal(100, model.BlockedRequests);
        Assert.Equal(900, model.AllowedRequests);
        Assert.Equal(5.5, model.RequestsPerSecond);
        Assert.Equal(42.3, model.AverageLatencyMs);
        Assert.Equal(15, model.ActiveClients);
        Assert.Equal(TimeSpan.FromSeconds(3600), model.Uptime);
    }

    [Fact]
    public void EndpointMetricsDto_MapsTo_Model_WithCorrectStatus()
    {
        var dto = new EndpointMetricsDto(
            Path: "/api/test",
            Method: "GET",
            TotalRequests: 500,
            BlockedRequests: 25,
            AllowedRequests: 475,
            AverageLatencyMs: 20.0,
            PolicyName: "fixed-window",
            Status: "Warning",
            LastActivity: DateTimeOffset.UtcNow
        );

        var model = dto.ToModel();

        Assert.Equal("/api/test", model.Path);
        Assert.Equal("GET", model.Method);
        Assert.Equal(EndpointStatus.Warning, model.Status);
        Assert.Equal("fixed-window", model.PolicyName);
    }

    [Fact]
    public void EndpointMetricsDto_MapsTo_Healthy_WhenUnknownStatus()
    {
        var dto = new EndpointMetricsDto(
            Path: "/api/unknown",
            Method: "POST",
            TotalRequests: 10,
            BlockedRequests: 0,
            AllowedRequests: 10,
            AverageLatencyMs: 5.0,
            PolicyName: "default",
            Status: "Unknown",
            LastActivity: DateTimeOffset.UtcNow
        );

        var model = dto.ToModel();

        Assert.Equal(EndpointStatus.Healthy, model.Status);
    }

    [Fact]
    public void ClientMetricsDto_MapsTo_Model_Correctly()
    {
        var firstSeen = DateTimeOffset.UtcNow.AddHours(-5);
        var lastSeen = DateTimeOffset.UtcNow;
        var dto = new ClientMetricsDto(
            IpAddress: "192.168.1.1",
            UserAgent: "TestAgent/1.0",
            TotalRequests: 200,
            BlockedRequests: 40,
            FirstSeen: firstSeen,
            LastSeen: lastSeen,
            RiskLevel: "High",
            IsBlocked: false
        );

        var model = dto.ToModel();

        Assert.Equal("192.168.1.1", model.IpAddress);
        Assert.Equal(ClientRisk.High, model.RiskLevel);
        Assert.Equal(firstSeen, model.FirstSeen);
        Assert.Equal(lastSeen, model.LastSeen);
    }

    [Fact]
    public void PolicyInfoDto_MapsTo_Model_WithWindowTimeSpan()
    {
        var dto = new PolicyInfoDto(
            Name: "api-limit",
            PermitLimit: 100,
            WindowSeconds: 60,
            Algorithm: "FixedWindow",
            TotalRequests: 1000,
            RejectedRequests: 50,
            IsActive: true,
            ActiveConnections: 8
        );

        var model = dto.ToModel();

        Assert.Equal("api-limit", model.Name);
        Assert.Equal(TimeSpan.FromSeconds(60), model.Window);
        Assert.Equal(100, model.PermitLimit);
        Assert.True(model.IsActive);
    }

    [Fact]
    public void TimeSeriesPointDto_MapsTo_Model()
    {
        var now = DateTimeOffset.UtcNow;
        var dto = new TimeSeriesPointDto(now, 42.5);

        var model = dto.ToModel();

        Assert.Equal(now, model.Timestamp);
        Assert.Equal(42.5, model.Value);
    }

    [Fact]
    public void TimeSeriesDataDto_MapsTo_Model_WithAllPoints()
    {
        var now = DateTimeOffset.UtcNow;
        var dto = new TimeSeriesDataDto("Requests",
        [
            new(now, 10),
            new(now.AddMinutes(1), 20),
            new(now.AddMinutes(2), 30)
        ]);

        var model = dto.ToModel();

        Assert.Equal("Requests", model.Name);
        Assert.Equal(3, model.Points.Count);
        Assert.Equal(30, model.Points[2].Value);
    }

    [Fact]
    public void MetricsSummaryApiDto_MapsHonestyFields()
    {
        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow;
        var dto = new MetricsSummaryApiDto(100, 10, from, to, 10, 33.5, 4);

        var model = dto.ToModel();

        Assert.Equal(33.5, model.AverageLatencyMs);
        Assert.Equal(4, model.ActiveClients);
    }

    [Fact]
    public void TopEndpointApiDto_MapsLatencyAndPolicy()
    {
        var last = DateTimeOffset.UtcNow.AddMinutes(-2);
        var dto = new TopEndpointApiDto("/api/x", "GET", 50, 5, 12.0, "fixed", last);

        var model = dto.ToModel();

        Assert.Equal(12.0, model.AverageLatencyMs);
        Assert.Equal("fixed", model.PolicyName);
        Assert.Equal(last, model.LastActivity);
    }

    [Fact]
    public void TopClientApiDto_MapsSeenTimestamps()
    {
        var first = DateTimeOffset.UtcNow.AddHours(-5);
        var last = DateTimeOffset.UtcNow.AddMinutes(-1);
        var dto = new TopClientApiDto("10.0.0.1", 20, 2, first, last);

        var model = dto.ToModel();

        Assert.Equal(first, model.FirstSeen);
        Assert.Equal(last, model.LastSeen);
    }

    [Fact]
    public void ObservedPolicyApiDto_MapsUsageOnly()
    {
        var dto = new ObservedPolicyApiDto("fixed-window", 200, 40);

        var model = dto.ToModel();

        Assert.Equal("fixed-window", model.Name);
        Assert.Equal(200, model.TotalRequests);
        Assert.Equal(40, model.RejectedRequests);
        Assert.Equal(0, model.PermitLimit);
        Assert.Equal(TimeSpan.Zero, model.Window);
    }
}
