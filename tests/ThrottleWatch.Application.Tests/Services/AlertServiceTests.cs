using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ThrottleWatch.Application.DTOs.Alerts;
using ThrottleWatch.Application.Services;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Application.Tests.Services;

public sealed class AlertServiceTests
{
    private readonly IAlertRepository _repository = Substitute.For<IAlertRepository>();
    private readonly ILogger<AlertService> _logger = Substitute.For<ILogger<AlertService>>();
    private readonly AlertService _sut;

    public AlertServiceTests()
    {
        _sut = new AlertService(_repository, _logger);
    }

    [Fact]
    public async Task CreateRuleAsync_WithValidDto_ShouldPersistAndReturnDto()
    {
        var dto = new CreateAlertRuleDto(
            "High block",
            "block_rate",
            20,
            AlertSeverity.Warning,
            15,
            "desc");

        var result = await _sut.CreateRuleAsync(dto, CancellationToken.None);

        result.Name.Should().Be("High block");
        result.Condition.Should().Be("block_rate");
        result.Threshold.Should().Be(20);
        result.IsEnabled.Should().BeTrue();
        await _repository.Received(1).AddAsync(Arg.Any<AlertRule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRuleByIdAsync_WhenMissing_ShouldThrow()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((AlertRule?)null);

        var act = () => _sut.GetRuleByIdAsync(id, CancellationToken.None);

        await act.Should().ThrowAsync<AlertRuleNotFoundException>();
    }

    [Fact]
    public async Task AcknowledgeEventAsync_WhenMissing_ShouldThrow()
    {
        var id = Guid.NewGuid();
        _repository.GetEventByIdAsync(id, Arg.Any<CancellationToken>()).Returns((AlertEvent?)null);

        var act = () => _sut.AcknowledgeEventAsync(id, CancellationToken.None);

        await act.Should().ThrowAsync<AlertEventNotFoundException>();
    }

    [Fact]
    public async Task AcknowledgeEventAsync_WhenFound_ShouldUpdateEvent()
    {
        var ruleId = Guid.NewGuid();
        var alertEvent = AlertEvent.Create(ruleId, "rule", "msg", AlertSeverity.Warning);
        _repository.GetEventByIdAsync(alertEvent.Id, Arg.Any<CancellationToken>()).Returns(alertEvent);

        await _sut.AcknowledgeEventAsync(alertEvent.Id, CancellationToken.None);

        alertEvent.IsAcknowledged.Should().BeTrue();
        await _repository.Received(1).UpdateEventAsync(alertEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteRuleAsync_WhenFound_ShouldDelete()
    {
        var rule = AlertRule.Create("name", "block_rate", 10, AlertSeverity.Info, 5);
        _repository.GetByIdAsync(rule.Id, Arg.Any<CancellationToken>()).Returns(rule);

        await _sut.DeleteRuleAsync(rule.Id, CancellationToken.None);

        await _repository.Received(1).DeleteAsync(rule.Id, Arg.Any<CancellationToken>());
    }
}
