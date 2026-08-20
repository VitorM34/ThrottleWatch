using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Tenancy;

namespace ThrottleWatch.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=throttlewatch;Username=postgres;Password=postgres");

        return new AppDbContext(optionsBuilder.Options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public string TenantId => TenantIds.Default;

        public void Set(string tenantId)
        {
        }
    }
}
