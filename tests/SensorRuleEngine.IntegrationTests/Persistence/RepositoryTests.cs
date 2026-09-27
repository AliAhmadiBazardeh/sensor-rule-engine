using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Processing;
using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.ValueObjects;
using SensorRuleEngine.Infrastructure.Persistence;
using SensorRuleEngine.Infrastructure.Persistence.Repositories;
using Xunit;

namespace SensorRuleEngine.IntegrationTests.Persistence;

public sealed class RepositoryTests
{
    [Fact]
    public async Task ReadingRepository_ShouldPersistReading()
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

        var repository =
            new ReadingRepository(dbContext);

        var reading = new SensorReading
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(
                "2026-01-01T10:00:00Z"),
            Value = 75,
            Sequence = 1
        };

        await repository.AddAsync(reading);

        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.Readings.SingleAsync();

        Assert.Equal(reading.DeviceId, persisted.DeviceId);

        var storedReading =
            await dbContext.Readings.SingleAsync();

        Assert.Equal(
            reading.DeviceId,
            storedReading.DeviceId);

        Assert.Equal(
            reading.Metric,
            storedReading.Metric);

        Assert.Equal(
            reading.Timestamp,
            storedReading.Timestamp);

        Assert.Equal(
            reading.Value,
            storedReading.Value);

        Assert.Equal(
            reading.Sequence,
            storedReading.Sequence);
    }
    
    [Fact]
    public async Task RuleResultRepository_ShouldPersistRuleResult()
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

        var repository =
            new RuleResultRepository(dbContext);

        var reading = new SensorReading
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(
                "2026-01-01T10:00:00Z"),
            Value = 85,
            Sequence = 1
        };

        var result = new RuleResult
        {
            RuleId = "rule-1",
            ReadingKey = new ReadingKey(
                reading.DeviceId,
                reading.Metric,
                reading.Timestamp,
                reading.Sequence),
            Status = RuleResultStatus.Violated,
            Reason = "Temperature exceeded threshold."
        };
        
        await repository.AddAsync(
            result);

        await dbContext.SaveChangesAsync();
        
        var storedResult =
            await dbContext.RuleResults.SingleAsync();

        Assert.Equal(
            result.RuleId,
            storedResult.RuleId);

        Assert.Equal(
            result.ReadingKey.DeviceId,
            storedResult.DeviceId);

        Assert.Equal(
            result.ReadingKey.Metric,
            storedResult.Metric);

        Assert.Equal(
            result.ReadingKey.Timestamp,
            storedResult.Timestamp);

        Assert.Equal(
            result.ReadingKey.Sequence,
            storedResult.Sequence);

        Assert.Equal(
            result.Status.ToString(),
            storedResult.Status);

        Assert.Equal(
            result.Reason,
            storedResult.Reason);
    }
    
    [Fact]
    public async Task AlertRepository_ShouldPersistAlert()
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

        var repository =
            new AlertRepository(dbContext);

        var alert = new Alert
        {
            RuleId = "rule-1",
            DeviceId = "device-1",
            Metric = "temperature",
            StartTimestamp = DateTimeOffset.Parse(
                "2026-01-01T10:00:00Z"),
            EndTimestamp = DateTimeOffset.Parse(
                "2026-01-01T10:05:00Z"),
            PeakValue = 95
        };

        await repository.AddAsync(alert);

        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.Alerts.SingleAsync();

        Assert.Equal(alert.DeviceId, persisted.DeviceId);

        var storedAlert =
            await dbContext.Alerts.SingleAsync();

        Assert.Equal(
            alert.RuleId,
            storedAlert.RuleId);

        Assert.Equal(
            alert.DeviceId,
            storedAlert.DeviceId);

        Assert.Equal(
            alert.Metric,
            storedAlert.Metric);

        Assert.Equal(
            alert.StartTimestamp,
            storedAlert.StartTimestamp);

        Assert.Equal(
            alert.EndTimestamp,
            storedAlert.EndTimestamp);

        Assert.Equal(
            alert.PeakValue,
            storedAlert.PeakValue);
    }
    
    [Fact]
    public async Task ProcessingPersistence_ShouldBeIdempotent_WhenSameResultIsPersistedTwice()
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

        var persistence =
            new ProcessingPersistence(
                new ReadingRepository(dbContext),
                new RuleResultRepository(dbContext),
                new AlertRepository(dbContext),
                dbContext);

        var reading = new SensorReading
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(
                "2026-01-01T00:00:00Z"),
            Value = 105,
            Sequence = 1
        };

        var readingKey = new ReadingKey(
            reading.DeviceId,
            reading.Metric,
            reading.Timestamp,
            reading.Sequence);

        var ruleResult = new RuleResult
        {
            RuleId = "rule-1",
            ReadingKey = readingKey,
            Status = RuleResultStatus.Violated,
            Reason = "Value exceeded threshold."
        };

        var alert = new Alert
        {
            RuleId = "sustained-1",
            DeviceId = reading.DeviceId,
            Metric = reading.Metric,
            StartTimestamp = reading.Timestamp,
            EndTimestamp = reading.Timestamp.AddMinutes(2),
            PeakValue = 110
        };

        var result = new ReadingProcessingResult
        {
            ProcessedReadings = new[] { reading },
            RuleResults = new[] { ruleResult },
            Classifications = Array.Empty<ReadingClassification>(),
            Alerts = new[] { alert }
        };

        await persistence.PersistAsync(result);
        await persistence.PersistAsync(result);

        Assert.Equal(
            1,
            await dbContext.Readings.CountAsync());

        Assert.Equal(
            1,
            await dbContext.RuleResults.CountAsync());

        Assert.Equal(
            1,
            await dbContext.Alerts.CountAsync());
    }
}