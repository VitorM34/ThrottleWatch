using ThrottleWatch.Dashboard.Models;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Models;

public sealed class DashboardMetricsTests
{
    [Fact]
    public void BlockRatePercent_ReturnsZero_WhenNoRequests()
    {
        var metrics = new DashboardMetrics
        {
            TotalRequests = 0,
            BlockedRequests = 0
        };

        Assert.Equal(0, metrics.BlockRatePercent);
    }

    [Fact]
    public void BlockRatePercent_CalculatesCorrectly()
    {
        var metrics = new DashboardMetrics
        {
            TotalRequests = 1000,
            BlockedRequests = 100
        };

        Assert.Equal(10.0, metrics.BlockRatePercent);
    }

    [Fact]
    public void BlockRatePercent_IsRoundedToTwoDecimals()
    {
        var metrics = new DashboardMetrics
        {
            TotalRequests = 3,
            BlockedRequests = 1
        };

        Assert.Equal(33.33, metrics.BlockRatePercent);
    }
}

public sealed class EndpointMetricsTests
{
    [Fact]
    public void BlockRatePercent_ReturnsZero_WhenNoRequests()
    {
        var endpoint = new EndpointMetrics
        {
            TotalRequests = 0,
            BlockedRequests = 0
        };

        Assert.Equal(0, endpoint.BlockRatePercent);
    }

    [Fact]
    public void BlockRatePercent_CalculatesCorrectly()
    {
        var endpoint = new EndpointMetrics
        {
            TotalRequests = 200,
            BlockedRequests = 50
        };

        Assert.Equal(25.0, endpoint.BlockRatePercent);
    }
}

public sealed class ClientMetricsTests
{
    [Fact]
    public void BlockRatePercent_ReturnsZero_WhenNoRequests()
    {
        var client = new ClientMetrics
        {
            TotalRequests = 0,
            BlockedRequests = 0
        };

        Assert.Equal(0, client.BlockRatePercent);
    }

    [Fact]
    public void BlockRatePercent_Is100_WhenAllBlocked()
    {
        var client = new ClientMetrics
        {
            TotalRequests = 10,
            BlockedRequests = 10
        };

        Assert.Equal(100.0, client.BlockRatePercent);
    }
}
