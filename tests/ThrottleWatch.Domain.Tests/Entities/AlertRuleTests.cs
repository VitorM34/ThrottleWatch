using FluentAssertions;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Domain.Tests.Entities;

public sealed class AlertRuleTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnAlertRule()
    {
        var rule = AlertRule.Create("High block rate", "block_rate > 50", 50, AlertSeverity.Warning, 15);

        rule.Should().NotBeNull();
        rule.Id.Should().NotBeEmpty();
        rule.Name.Should().Be("High block rate");
        rule.Condition.Should().Be("block_rate > 50");
        rule.Threshold.Should().Be(50);
        rule.Severity.Should().Be(AlertSeverity.Warning);
        rule.CooldownMinutes.Should().Be(15);
        rule.IsEnabled.Should().BeTrue();
        rule.LastTriggeredAt.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ShouldThrowDomainException(string? name)
    {
        var act = () => AlertRule.Create(name!, "condition", 50, AlertSeverity.Info, 5);

        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCondition_ShouldThrowDomainException(string? condition)
    {
        var act = () => AlertRule.Create("name", condition!, 50, AlertSeverity.Info, 5);

        act.Should().Throw<DomainException>().WithMessage("*condition*");
    }

    [Fact]
    public void Create_WithNegativeThreshold_ShouldThrowDomainException()
    {
        var act = () => AlertRule.Create("name", "condition", -1, AlertSeverity.Info, 5);

        act.Should().Throw<DomainException>().WithMessage("*threshold*");
    }

    [Fact]
    public void Create_WithNegativeCooldown_ShouldThrowDomainException()
    {
        var act = () => AlertRule.Create("name", "condition", 50, AlertSeverity.Info, -1);

        act.Should().Throw<DomainException>().WithMessage("*cooldownMinutes*");
    }

    [Fact]
    public void CanTrigger_WhenNeverTriggered_ShouldReturnTrue()
    {
        var rule = AlertRule.Create("name", "condition", 50, AlertSeverity.Info, 15);

        rule.CanTrigger(DateTimeOffset.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void CanTrigger_WhenCooldownNotElapsed_ShouldReturnFalse()
    {
        var rule = AlertRule.Create("name", "condition", 50, AlertSeverity.Info, 15);
        var triggeredAt = DateTimeOffset.UtcNow;
        rule.RecordTrigger(triggeredAt);

        rule.CanTrigger(triggeredAt.AddMinutes(10)).Should().BeFalse();
    }

    [Fact]
    public void CanTrigger_WhenCooldownElapsed_ShouldReturnTrue()
    {
        var rule = AlertRule.Create("name", "condition", 50, AlertSeverity.Info, 15);
        var triggeredAt = DateTimeOffset.UtcNow;
        rule.RecordTrigger(triggeredAt);

        rule.CanTrigger(triggeredAt.AddMinutes(15)).Should().BeTrue();
    }

    [Fact]
    public void CanTrigger_WhenDisabled_ShouldReturnFalse()
    {
        var rule = AlertRule.Create("name", "condition", 50, AlertSeverity.Info, 0);
        rule.Disable();

        rule.CanTrigger(DateTimeOffset.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void Enable_ShouldSetIsEnabledTrue()
    {
        var rule = AlertRule.Create("name", "condition", 50, AlertSeverity.Info, 0);
        rule.Disable();
        rule.Enable();

        rule.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void RecordTrigger_ShouldUpdateLastTriggeredAt()
    {
        var rule = AlertRule.Create("name", "condition", 50, AlertSeverity.Info, 15);
        var now = DateTimeOffset.UtcNow;
        rule.RecordTrigger(now);

        rule.LastTriggeredAt.Should().Be(now);
    }
}
