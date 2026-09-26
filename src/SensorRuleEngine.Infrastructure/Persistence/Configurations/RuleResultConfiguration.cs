using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorRuleEngine.Infrastructure.Persistence.Entities;

namespace SensorRuleEngine.Infrastructure.Persistence.Configurations;

public sealed class RuleResultConfiguration
    : IEntityTypeConfiguration<RuleResultEntity>
{
    public void Configure(
        EntityTypeBuilder<RuleResultEntity> builder)
    {
        builder.ToTable("RuleResults");

        builder.HasKey(result => result.Id);

        builder.Property(result => result.RuleId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(result => result.DeviceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(result => result.Metric)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(result => result.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(result => result.Reason)
            .HasMaxLength(500);

        builder.HasIndex(result => new
            {
                result.RuleId,
                result.DeviceId,
                result.Metric,
                result.Timestamp,
                result.Sequence
            })
            .IsUnique();
    }
}