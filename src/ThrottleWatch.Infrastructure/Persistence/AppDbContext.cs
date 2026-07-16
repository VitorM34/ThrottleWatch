using Microsoft.EntityFrameworkCore;
using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MetricEntry> MetricEntries => Set<MetricEntry>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertEvent> AlertEvents => Set<AlertEvent>();
    public DbSet<Insight> Insights => Set<Insight>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
