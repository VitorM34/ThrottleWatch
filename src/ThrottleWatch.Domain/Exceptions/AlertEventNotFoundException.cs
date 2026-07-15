namespace ThrottleWatch.Domain.Exceptions;

public sealed class AlertEventNotFoundException : DomainException
{
    public AlertEventNotFoundException(Guid id)
        : base($"AlertEvent with id '{id}' was not found.") { }
}
