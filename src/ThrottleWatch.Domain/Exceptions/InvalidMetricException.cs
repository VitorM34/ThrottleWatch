namespace ThrottleWatch.Domain.Exceptions;

public sealed class InvalidMetricException : DomainException
{
    public InvalidMetricException(string field)
        : base($"Invalid metric data: {field} cannot be null or empty.") { }
}
