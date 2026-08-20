using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Application.Tenancy;

public sealed class TenantContext : ITenantContext
{
    public string TenantId { get; private set; } = TenantIds.Default;

    public void Set(string tenantId) => TenantId = TenantIds.Normalize(tenantId);
}
