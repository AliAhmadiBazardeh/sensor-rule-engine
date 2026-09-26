using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorRuleEngine.Infrastructure.Persistence.Entities;

namespace SensorRuleEngine.Infrastructure.Persistence.Configurations;

public sealed class ReadingConfiguration
    : IEntityTypeConfiguration<ReadingEntity>
{
    public void Configure(
        EntityTypeBuilder<ReadingEntity> builder)
    {
        builder.ToTable("Readings");

        builder.HasKey(reading => reading.Id);

        builder.Property(reading => reading.DeviceId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(reading => reading.Metric)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(reading => reading.Value)
            .HasPrecision(18, 6);

        builder.HasIndex(reading => new
            {
                reading.DeviceId,
                reading.Metric,
                reading.Timestamp,
                reading.Sequence
            })
            .IsUnique();
    }
}