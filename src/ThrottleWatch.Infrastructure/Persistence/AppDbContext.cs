using Microsoft.EntityFrameworkCore;
using ThrottleWatch.Application.Tenancy;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Infrastructure.Persistence.Models;

namespace ThrottleWatch.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public string CurrentTenantId => _tenantContext.TenantId;

    public DbSet<MetricEntry> MetricEntries => Set<MetricEntry>();
    public DbSet<MetricRollup> MetricRollups => Set<MetricRollup>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertEvent> AlertEvents => Set<AlertEvent>();
    public DbSet<Insight> Insights => Set<Insight>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<MetricEntry>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<MetricRollup>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<AlertRule>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<AlertEvent>().HasQueryFilter(e => e.TenantId == CurrentTenantId);
        modelBuilder.Entity<Insight>().HasQueryFilter(e => e.TenantId == CurrentTenantId);

        base.OnModelCreating(modelBuilder);
    }
}
