using FluentValidation;
using ThrottleWatch.Application.DTOs.Alerts;

namespace ThrottleWatch.Application.Validators;

public sealed class UpdateAlertRuleValidator : AbstractValidator<UpdateAlertRuleDto>
{
    public UpdateAlertRuleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Condition)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Threshold)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CooldownMinutes)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Severity)
            .IsInEnum();

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description is not null);
    }
}
