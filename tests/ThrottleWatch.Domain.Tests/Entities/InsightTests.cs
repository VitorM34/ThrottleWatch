using FluentAssertions;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Domain.Tests.Entities;

public sealed class InsightTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnInsight()
    {
        var insight = Insight.Create(
            InsightType.HighBlockRate,
            "High block rate on /api/orders",
            "The /api/orders endpoint is blocking over 60% of requests.",
            AlertSeverity.Warning,
            "/api/orders");

        insight.Should().NotBeNull();
        insight.Id.Should().NotBeEmpty();
        insight.Type.Should().Be(InsightType.HighBlockRate);
        insight.Title.Should().Be("High block rate on /api/orders");
        insight.Severity.Should().Be(AlertSeverity.Warning);
        insight.AffectedResource.Should().Be("/api/orders");
        insight.IsDismissed.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ShouldThrowDomainException(string? title)
    {
        var act = () => Insight.Create(InsightType.HighBlockRate, title!, "description", AlertSeverity.Info);

        act.Should().Throw<DomainException>().WithMessage("*title*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ShouldThrowDomainException(string? description)
    {
        var act = () => Insight.Create(InsightType.HighBlockRate, "title", description!, AlertSeverity.Info);

        act.Should().Throw<DomainException>().WithMessage("*description*");
    }

    [Fact]
    public void Create_WithoutAffectedResource_ShouldSetItNull()
    {
        var insight = Insight.Create(InsightType.PeakHours, "title", "description", AlertSeverity.Info);

        insight.AffectedResource.Should().BeNull();
    }

    [Fact]
    public void Dismiss_ShouldSetIsDismissedTrue()
    {
        var insight = Insight.Create(InsightType.HighBlockRate, "title", "description", AlertSeverity.Warning);
        insight.Dismiss();

        insight.IsDismissed.Should().BeTrue();
    }
}
