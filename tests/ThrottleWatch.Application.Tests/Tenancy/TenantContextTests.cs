using FluentAssertions;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Exceptions;
using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Application.Tests.Tenancy;

public sealed class TenantContextTests
{
    [Fact]
    public void NewContext_ShouldUseDefaultTenant()
    {
        var context = new TenantContext();

        context.TenantId.Should().Be(TenantIds.Default);
    }

    [Fact]
    public void Set_WithValidTenant_ShouldUpdate()
    {
        var context = new TenantContext();

        context.Set("acme");

        context.TenantId.Should().Be("acme");
    }

    [Fact]
    public void Set_WithEmptyTenant_ShouldThrow()
    {
        var context = new TenantContext();

        var act = () => context.Set("  ");

        act.Should().Throw<DomainException>();
    }
}
