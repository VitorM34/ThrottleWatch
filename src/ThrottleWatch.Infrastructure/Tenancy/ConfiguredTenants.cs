using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Infrastructure.Tenancy;

internal static class ConfiguredTenants
{
    public static IReadOnlyList<string> Ids(IApiKeyTenantMap map)
    {
        var ids = map.ConfiguredTenantIds;
        return ids.Count > 0 ? ids : [TenantIds.Default];
    }
}
