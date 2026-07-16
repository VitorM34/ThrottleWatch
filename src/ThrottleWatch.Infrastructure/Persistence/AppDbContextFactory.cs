using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ThrottleWatch.Infrastructure.Persistence;

namespace ThrottleWatch.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=throttlewatch;Username=postgres;Password=postgres");

        return new AppDbContext(optionsBuilder.Options);
    }
}
