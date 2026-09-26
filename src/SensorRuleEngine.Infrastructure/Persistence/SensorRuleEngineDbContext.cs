using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Infrastructure.Persistence.Entities;

namespace SensorRuleEngine.Infrastructure.Persistence;

public sealed class SensorRuleEngineDbContext
    : DbContext
{
    public SensorRuleEngineDbContext(
        DbContextOptions<SensorRuleEngineDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReadingEntity> Readings => Set<ReadingEntity>();

    public DbSet<RuleResultEntity> RuleResults => Set<RuleResultEntity>();

    public DbSet<AlertEntity> Alerts => Set<AlertEntity>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SensorRuleEngineDbContext).Assembly);
    }
}