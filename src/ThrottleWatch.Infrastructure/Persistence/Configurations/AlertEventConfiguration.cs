using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Infrastructure.Persistence.Configurations;

public sealed class AlertEventConfiguration : IEntityTypeConfiguration<AlertEvent>
{
    public void Configure(EntityTypeBuilder<AlertEvent> builder)
    {
        builder.ToTable("alert_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AlertRuleId)
            .IsRequired();

        builder.Property(x => x.RuleName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.TriggeredAt)
            .IsRequired();

        builder.Property(x => x.IsAcknowledged)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.TriggeredAt);
        builder.HasIndex(x => x.AlertRuleId);
    }
}
