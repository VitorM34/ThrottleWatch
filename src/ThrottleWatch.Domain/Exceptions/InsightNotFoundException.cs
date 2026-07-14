namespace ThrottleWatch.Domain.Exceptions;

public sealed class InsightNotFoundException : DomainException
{
    public InsightNotFoundException(Guid id)
        : base($"Insight with id '{id}' was not found.") { }
}
