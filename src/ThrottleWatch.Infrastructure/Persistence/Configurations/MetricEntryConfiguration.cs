using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Infrastructure.Persistence.Configurations;

public sealed class MetricEntryConfiguration : IEntityTypeConfiguration<MetricEntry>
{
    public void Configure(EntityTypeBuilder<MetricEntry> builder)
    {
        builder.ToTable("metric_entries");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Path)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.Method)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(x => x.StatusCode)
            .IsRequired();

        builder.Property(x => x.IsBlocked)
            .IsRequired();

        builder.Property(x => x.PolicyName)
            .HasMaxLength(128);

        builder.Property(x => x.ClientIp)
            .HasMaxLength(64);

        builder.Property(x => x.ApiKey)
            .HasMaxLength(256);

        builder.Property(x => x.DurationMs)
            .IsRequired();

        builder.Property(x => x.Timestamp)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Timestamp);
        builder.HasIndex(x => new { x.Path, x.Method });
        builder.HasIndex(x => x.IsBlocked);
        builder.HasIndex(x => x.ClientIp);
    }
}
