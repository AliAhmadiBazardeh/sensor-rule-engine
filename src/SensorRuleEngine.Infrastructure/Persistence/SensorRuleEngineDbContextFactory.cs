using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SensorRuleEngine.Infrastructure.Persistence;

public sealed class SensorRuleEngineDbContextFactory
    : IDesignTimeDbContextFactory<SensorRuleEngineDbContext>
{
    public SensorRuleEngineDbContext CreateDbContext(
        string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<SensorRuleEngineDbContext>();

        optionsBuilder.UseSqlite(
            "Data Source=sensor_rule_engine.db");

        return new SensorRuleEngineDbContext(
            optionsBuilder.Options);
    }
}