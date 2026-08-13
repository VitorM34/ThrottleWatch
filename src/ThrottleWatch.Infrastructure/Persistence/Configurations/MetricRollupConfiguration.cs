using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThrottleWatch.Infrastructure.Persistence.Models;

namespace ThrottleWatch.Infrastructure.Persistence.Configurations;

public sealed class MetricRollupConfiguration : IEntityTypeConfiguration<MetricRollup>
{
    public void Configure(EntityTypeBuilder<MetricRollup> builder)
    {
        builder.ToTable("metric_rollups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BucketStart)
            .IsRequired();

        builder.Property(x => x.Granularity)
            .IsRequired()
            .HasConversion<byte>();

        builder.Property(x => x.TotalRequests)
            .IsRequired();

        builder.Property(x => x.BlockedRequests)
            .IsRequired();

        builder.HasIndex(x => new { x.Granularity, x.BucketStart })
            .IsUnique();

        builder.HasIndex(x => x.BucketStart);
    }
}
