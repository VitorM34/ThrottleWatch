using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Infrastructure.Persistence.Configurations;

public sealed class InsightConfiguration : IEntityTypeConfiguration<Insight>
{
    public void Configure(EntityTypeBuilder<Insight> builder)
    {
        builder.ToTable("insights");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.AffectedResource)
            .HasMaxLength(512);

        builder.Property(x => x.GeneratedAt)
            .IsRequired();

        builder.Property(x => x.IsDismissed)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.IsDismissed);
        builder.HasIndex(x => x.GeneratedAt);
    }
}
