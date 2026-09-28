using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Ingestion;
using SensorRuleEngine.Application.Processing;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Readings;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Infrastructure.Persistence;
using SensorRuleEngine.Infrastructure.Persistence.Repositories;
using SensorRuleEngine.IntegrationTests.Persistence;
using Xunit;

namespace SensorRuleEngine.IntegrationTests.Processing;

public sealed class ReadingProcessingOrchestratorTests
{
    [Fact]
    public async Task ProcessAsync_ShouldProcessEntireJsonlPipeline()
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

        var batchProcessor =
            TestServiceFactory.CreateBatchProcessor(
                persistence);

        var ingestionService =
            new ReadingIngestionService(
                new JsonlReadingParser(),
                new SensorReadingValidator());

        var orchestrator =
            new ReadingProcessingOrchestrator(
                ingestionService,
                batchProcessor);

        const string jsonl =
            """
            {"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:00:00Z","value":35,"seq":1}
            {"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:00:00Z","value":35,"seq":1}
            {"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:01:00Z","value":20,"seq":2}
            {"deviceId":"device-1","metric":"temperature","ts":"invalid","value":25,"seq":3}
            """;

        await using var stream =
            new MemoryStream(Encoding.UTF8.GetBytes(jsonl));

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

        var report =
            await orchestrator.ProcessAsync(
                stream,
                new[] { rule });

        Assert.Equal(4, report.TotalLines);
        Assert.Equal(3, report.Parsed);
        Assert.Equal(1, report.Invalid);
        Assert.Equal(1, report.Duplicates);

        Assert.Equal(2, report.Stored);

        Assert.Equal(1, report.RulesLoaded);
        Assert.Equal(2, report.Evaluations);

        Assert.Equal(1, report.Acceptable);
        Assert.Equal(1, report.Unacceptable);

        Assert.Equal(1, report.Violations);
        Assert.Equal(0, report.Alerts);

        Assert.Equal(2, await dbContext.Readings.CountAsync());
        Assert.Equal(2, await dbContext.RuleResults.CountAsync());
        Assert.Equal(0, await dbContext.Alerts.CountAsync());
    }
}