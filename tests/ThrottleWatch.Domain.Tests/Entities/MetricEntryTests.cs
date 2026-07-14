using FluentAssertions;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Domain.Tests.Entities;

public sealed class MetricEntryTests
{
    private static readonly DateTimeOffset _timestamp = DateTimeOffset.UtcNow;

    [Fact]
    public void Create_WithValidData_ShouldReturnMetricEntry()
    {
        var entry = MetricEntry.Create(
            path: "/api/health",
            method: "GET",
            statusCode: 200,
            durationMs: 15.5,
            timestamp: _timestamp);

        entry.Should().NotBeNull();
        entry.Id.Should().NotBeEmpty();
        entry.Path.Should().Be("/api/health");
        entry.Method.Should().Be("GET");
        entry.StatusCode.Should().Be(200);
        entry.IsBlocked.Should().BeFalse();
        entry.DurationMs.Should().Be(15.5);
        entry.Timestamp.Should().Be(_timestamp);
    }

    [Fact]
    public void Create_ShouldNormalizePath_ToLowercase()
    {
        var entry = MetricEntry.Create("/API/Users", "GET", 200, 10, _timestamp);

        entry.Path.Should().Be("/api/users");
    }

    [Fact]
    public void Create_ShouldNormalizeMethod_ToUppercase()
    {
        var entry = MetricEntry.Create("/api/data", "post", 201, 20, _timestamp);

        entry.Method.Should().Be("POST");
    }

    [Fact]
    public void Create_WithStatusCode429_ShouldMarkAsBlocked()
    {
        var entry = MetricEntry.Create("/api/heavy", "GET", 429, 0, _timestamp);

        entry.IsBlocked.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNonBlockedStatusCode_ShouldNotMarkAsBlocked()
    {
        var entry = MetricEntry.Create("/api/endpoint", "GET", 200, 10, _timestamp);

        entry.IsBlocked.Should().BeFalse();
    }

    [Fact]
    public void Create_WithOptionalFields_ShouldMapCorrectly()
    {
        var entry = MetricEntry.Create(
            path: "/api/secure",
            method: "DELETE",
            statusCode: 200,
            durationMs: 30,
            timestamp: _timestamp,
            clientIp: "192.168.1.1",
            policyName: "fixed-window",
            apiKey: "abc123");

        entry.ClientIp.Should().Be("192.168.1.1");
        entry.PolicyName.Should().Be("fixed-window");
        entry.ApiKey.Should().Be("abc123");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyPath_ShouldThrowDomainException(string? path)
    {
        var act = () => MetricEntry.Create(path!, "GET", 200, 10, _timestamp);

        act.Should().Throw<DomainException>()
            .WithMessage("*path*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyMethod_ShouldThrowDomainException(string? method)
    {
        var act = () => MetricEntry.Create("/api/test", method!, 200, 10, _timestamp);

        act.Should().Throw<DomainException>()
            .WithMessage("*method*");
    }

    [Fact]
    public void Create_WithNegativeDuration_ShouldThrowDomainException()
    {
        var act = () => MetricEntry.Create("/api/test", "GET", 200, -1, _timestamp);

        act.Should().Throw<DomainException>()
            .WithMessage("*durationMs*");
    }

    [Fact]
    public void Create_ShouldAssignNewId_PerInstance()
    {
        var a = MetricEntry.Create("/api/a", "GET", 200, 10, _timestamp);
        var b = MetricEntry.Create("/api/b", "GET", 200, 10, _timestamp);

        a.Id.Should().NotBe(b.Id);
    }
}
