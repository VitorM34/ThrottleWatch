using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Tenancy;
using ThrottleWatch.Infrastructure.Configuration;

namespace ThrottleWatch.Infrastructure.Security;

public sealed class ApiKeyTenantMap : IApiKeyTenantMap
{
    private readonly IOptionsMonitor<ThrottleWatchOptions> _options;

    public ApiKeyTenantMap(IOptionsMonitor<ThrottleWatchOptions> options)
    {
        _options = options;
    }

    public bool AuthEnabled => Snapshot().Count > 0;

    public IReadOnlyList<string> ConfiguredTenantIds =>
        Snapshot().Select(x => x.TenantId).Distinct(StringComparer.Ordinal).ToArray();

    public bool TryResolve(string? providedKey, out string tenantId)
    {
        tenantId = TenantIds.Default;
        if (string.IsNullOrEmpty(providedKey))
            return false;

        foreach (var entry in Snapshot())
        {
            if (!FixedTimeEquals(providedKey, entry.ApiKey))
                continue;

            tenantId = entry.TenantId;
            return true;
        }

        return false;
    }

    public static IReadOnlyList<(string ApiKey, string TenantId)> BuildEntries(SecurityOptions security)
    {
        var entries = new List<(string ApiKey, string TenantId)>();
        var seen = new Dictionary<string, string>(StringComparer.Ordinal);

        void Add(string? key, string? tenantId)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            var normalizedKey = key.Trim();
            var normalizedTenant = string.IsNullOrWhiteSpace(tenantId)
                ? TenantIds.Default
                : TenantIds.Normalize(tenantId);

            if (seen.TryGetValue(normalizedKey, out var existing)
                && !string.Equals(existing, normalizedTenant, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    "Duplicate ThrottleWatch API keys cannot map to different tenants.");
            }

            if (seen.ContainsKey(normalizedKey))
                return;

            seen[normalizedKey] = normalizedTenant;
            entries.Add((normalizedKey, normalizedTenant));
        }

        Add(security.ApiKey, security.TenantId);
        foreach (var tenant in security.Tenants)
            Add(tenant.ApiKey, tenant.TenantId);

        return entries;
    }

    private IReadOnlyList<(string ApiKey, string TenantId)> Snapshot() =>
        BuildEntries(_options.CurrentValue.Security);

    private static bool FixedTimeEquals(string provided, string expected)
    {
        var a = Encoding.UTF8.GetBytes(provided);
        var b = Encoding.UTF8.GetBytes(expected);
        if (a.Length != b.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(a, b);
    }
}
