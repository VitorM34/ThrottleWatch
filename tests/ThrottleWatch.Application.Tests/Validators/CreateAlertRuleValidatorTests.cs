using FluentAssertions;
using FluentValidation.TestHelper;
using ThrottleWatch.Application.DTOs.Alerts;
using ThrottleWatch.Application.Validators;
using ThrottleWatch.Domain.Enums;

namespace ThrottleWatch.Application.Tests.Validators;

public sealed class CreateAlertRuleValidatorTests
{
    private readonly CreateAlertRuleValidator _sut = new();

    [Fact]
    public void Validate_WithValidDto_ShouldNotHaveErrors()
    {
        var dto = new CreateAlertRuleDto(
            "Rule",
            "block_rate",
            10,
            AlertSeverity.Warning,
            15);

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var dto = new CreateAlertRuleDto(
            "",
            "block_rate",
            10,
            AlertSeverity.Warning,
            15);

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNegativeThreshold_ShouldHaveError()
    {
        var dto = new CreateAlertRuleDto(
            "Rule",
            "block_rate",
            -1,
            AlertSeverity.Warning,
            15);

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Threshold);
    }
}

public sealed class UpdateAlertRuleValidatorTests
{
    private readonly UpdateAlertRuleValidator _sut = new();

    [Fact]
    public void Validate_WithValidDto_ShouldNotHaveErrors()
    {
        var dto = new UpdateAlertRuleDto(
            "Rule",
            "block_rate",
            10,
            AlertSeverity.Info,
            5,
            true);

        var result = _sut.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyCondition_ShouldHaveError()
    {
        var dto = new UpdateAlertRuleDto(
            "Rule",
            "",
            10,
            AlertSeverity.Info,
            5,
            true);

        var result = _sut.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Condition);
    }
}
