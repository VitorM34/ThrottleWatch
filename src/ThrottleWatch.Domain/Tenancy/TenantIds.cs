using ThrottleWatch.Domain.Exceptions;

namespace ThrottleWatch.Domain.Tenancy;

public static class TenantIds
{
    public const string Default = "default";
    public const int MaxLength = 64;

    public static string Normalize(string? tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new DomainException("TenantId cannot be null or empty.");

        var value = tenantId.Trim();
        if (value.Length > MaxLength)
            throw new DomainException($"TenantId cannot exceed {MaxLength} characters.");

        return value;
    }
}
