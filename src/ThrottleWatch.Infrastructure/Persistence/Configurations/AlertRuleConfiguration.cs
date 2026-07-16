using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Infrastructure.Persistence.Configurations;

public sealed class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.ToTable("alert_rules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Condition)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Threshold)
            .IsRequired();

        builder.Property(x => x.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.CooldownMinutes)
            .IsRequired();

        builder.Property(x => x.LastTriggeredAt);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.IsEnabled);
    }
}
