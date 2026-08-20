namespace ThrottleWatch.Application.Tenancy;

public interface IApiKeyTenantMap
{
    bool AuthEnabled { get; }

    IReadOnlyList<string> ConfiguredTenantIds { get; }

    bool TryResolve(string? providedKey, out string tenantId);
}
