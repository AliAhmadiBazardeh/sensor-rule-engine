using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorRuleEngine.Infrastructure.Persistence.Entities;

namespace SensorRuleEngine.Infrastructure.Persistence.Configurations;

public sealed class AlertConfiguration
    : IEntityTypeConfiguration<AlertEntity>
{
    public void Configure(
        EntityTypeBuilder<AlertEntity> builder)
    {
        builder.ToTable("Alerts");

        builder.HasKey(alert => alert.Id);

        builder.Property(alert => alert.RuleId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(alert => alert.DeviceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(alert => alert.Metric)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(alert => alert.PeakValue)
            .HasPrecision(18, 6);

        builder.HasIndex(alert => new
            {
                alert.RuleId,
                alert.DeviceId,
                alert.Metric,
                alert.StartTimestamp,
                alert.EndTimestamp
            })
            .IsUnique();
    }
}