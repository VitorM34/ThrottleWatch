using FluentAssertions;
using ThrottleWatch.Domain.Exceptions;
using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Domain.Tests.Tenancy;

public sealed class TenantIdsTests
{
    [Fact]
    public void Normalize_WithValidValue_ShouldTrim()
    {
        TenantIds.Normalize("  acme  ").Should().Be("acme");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Normalize_WithEmptyValue_ShouldThrow(string? tenantId)
    {
        var act = () => TenantIds.Normalize(tenantId);

        act.Should().Throw<DomainException>().WithMessage("*TenantId*");
    }

    [Fact]
    public void Normalize_WhenLongerThanMax_ShouldThrow()
    {
        var act = () => TenantIds.Normalize(new string('a', TenantIds.MaxLength + 1));

        act.Should().Throw<DomainException>().WithMessage("*64*");
    }
}
