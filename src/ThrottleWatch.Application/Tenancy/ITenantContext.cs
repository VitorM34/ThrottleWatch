namespace ThrottleWatch.Application.Tenancy;

public interface ITenantContext
{
    string TenantId { get; }

    void Set(string tenantId);
}
