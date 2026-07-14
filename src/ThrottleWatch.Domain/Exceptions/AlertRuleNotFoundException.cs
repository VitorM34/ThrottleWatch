namespace ThrottleWatch.Domain.Exceptions;

public sealed class AlertRuleNotFoundException : DomainException
{
    public AlertRuleNotFoundException(Guid id)
        : base($"AlertRule with id '{id}' was not found.") { }
}
