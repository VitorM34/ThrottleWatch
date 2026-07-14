using FluentAssertions;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Domain.Tests.Entities;

public sealed class AlertEventTests
{
    private static readonly Guid _ruleId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldReturnAlertEvent()
    {
        var ev = AlertEvent.Create(_ruleId, "High block rate", "Block rate exceeded 50%", AlertSeverity.Warning);

        ev.Should().NotBeNull();
        ev.Id.Should().NotBeEmpty();
        ev.AlertRuleId.Should().Be(_ruleId);
        ev.RuleName.Should().Be("High block rate");
        ev.Message.Should().Be("Block rate exceeded 50%");
        ev.Severity.Should().Be(AlertSeverity.Warning);
        ev.IsAcknowledged.Should().BeFalse();
    }

    [Fact]
    public void Create_WithEmptyRuleId_ShouldThrowDomainException()
    {
        var act = () => AlertEvent.Create(Guid.Empty, "name", "message", AlertSeverity.Info);

        act.Should().Throw<DomainException>().WithMessage("*alertRuleId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidRuleName_ShouldThrowDomainException(string? ruleName)
    {
        var act = () => AlertEvent.Create(_ruleId, ruleName!, "message", AlertSeverity.Info);

        act.Should().Throw<DomainException>().WithMessage("*ruleName*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidMessage_ShouldThrowDomainException(string? message)
    {
        var act = () => AlertEvent.Create(_ruleId, "name", message!, AlertSeverity.Info);

        act.Should().Throw<DomainException>().WithMessage("*message*");
    }

    [Fact]
    public void Acknowledge_ShouldSetIsAcknowledgedTrue()
    {
        var ev = AlertEvent.Create(_ruleId, "name", "message", AlertSeverity.Info);
        ev.Acknowledge();

        ev.IsAcknowledged.Should().BeTrue();
    }
}
