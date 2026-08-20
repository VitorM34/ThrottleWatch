using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using ThrottleWatch.Infrastructure.Configuration;
using ThrottleWatch.Infrastructure.Security;

namespace ThrottleWatch.Infrastructure.Tests.Security;

public sealed class ApiKeyTenantMapTests
{
    [Fact]
    public void TryResolve_WhenKeyMatches_ShouldReturnConfiguredTenant()
    {
        var map = CreateMap(new SecurityOptions
        {
            ApiKey = "key-a",
            TenantId = "tenant-a",
            Tenants =
            {
                new TenantKeyOptions { ApiKey = "key-b", TenantId = "tenant-b" }
            }
        });

        map.TryResolve("key-b", out var tenantId).Should().BeTrue();
        tenantId.Should().Be("tenant-b");
        map.ConfiguredTenantIds.Should().BeEquivalentTo("tenant-a", "tenant-b");
    }

    [Fact]
    public void TryResolve_WhenKeyMissing_ShouldReturnFalse()
    {
        var map = CreateMap(new SecurityOptions { ApiKey = "key-a" });

        map.TryResolve("wrong", out var tenantId).Should().BeFalse();
        tenantId.Should().Be("default");
    }

    [Fact]
    public void BuildEntries_WhenTenantIdOmitted_ShouldUseDefault()
    {
        var entries = ApiKeyTenantMap.BuildEntries(new SecurityOptions { ApiKey = "only-key" });

        entries.Should().ContainSingle();
        entries[0].TenantId.Should().Be("default");
    }

    [Fact]
    public void BuildEntries_WhenSameKeyMapsToDifferentTenants_ShouldThrow()
    {
        var act = () => ApiKeyTenantMap.BuildEntries(new SecurityOptions
        {
            ApiKey = "shared",
            TenantId = "a",
            Tenants =
            {
                new TenantKeyOptions { ApiKey = "shared", TenantId = "b" }
            }
        });

        act.Should().Throw<InvalidOperationException>().WithMessage("*different tenants*");
    }

    private static ApiKeyTenantMap CreateMap(SecurityOptions security)
    {
        var options = Substitute.For<IOptionsMonitor<ThrottleWatchOptions>>();
        options.CurrentValue.Returns(new ThrottleWatchOptions { Security = security });
        return new ApiKeyTenantMap(options);
    }
}
