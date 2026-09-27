using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Infrastructure.Persistence;
using SensorRuleEngine.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SensorRuleEngine.IntegrationTests.Persistence;

public sealed class ReadingBatchProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldReportZeroStoredRecords_OnIdempotentRerun()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<SensorRuleEngineDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var dbContext =
            new SensorRuleEngineDbContext(options);

        await dbContext.Database.MigrateAsync();

        var readingRepository =
            new ReadingRepository(dbContext);

        var ruleResultRepository =
            new RuleResultRepository(dbContext);

        var alertRepository =
            new AlertRepository(dbContext);

        var persistence =
            new ProcessingPersistence(
                readingRepository,
                ruleResultRepository,
                alertRepository,
                dbContext);

        var processor =
            TestServiceFactory.CreateBatchProcessor(
                persistence);

        var reading = new SensorReading
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = new DateTimeOffset(
                2026, 1, 1, 10, 0, 0, TimeSpan.Zero),
            Value = 25m,
            Sequence = 1
        };

        var rule = new Rule
        {
            Id = "rule-1",
            Name = "Temperature threshold",
            Enabled = true,
            Metric = "temperature",
            Operator = RuleOperatorType.GreaterThan,
            Parameters = new Dictionary<string, decimal>
            {
                ["threshold"] = 30m
            }
        };

        var firstReport =
            await processor.ProcessAsync(
                new[] { reading },
                new[] { rule },
                totalLines: 1,
                parsed: 1,
                invalid: 0,
                duplicates: 0);

        var secondReport =
            await processor.ProcessAsync(
                new[] { reading },
                new[] { rule },
                totalLines: 1,
                parsed: 1,
                invalid: 0,
                duplicates: 0);

        Assert.Equal(1, firstReport.Stored);
        Assert.Equal(0, secondReport.Stored);
    }
}