using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
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

        var added = await repository.AddAsync(reading);

        Assert.True(added);

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
    public async Task ReadingRepository_ShouldReturnFalseForDuplicate()
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

        var firstResult =
            await repository.AddAsync(reading);

        var secondResult =
            await repository.AddAsync(reading);

        Assert.True(firstResult);
        Assert.False(secondResult);

        Assert.Equal(
            1,
            await dbContext.Readings.CountAsync());
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

        var added =
            await repository.AddAsync(
                result,
                reading);

        Assert.True(added);

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
    public async Task RuleResultRepository_ShouldReturnFalseForDuplicate()
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

        var firstResult =
            await repository.AddAsync(
                result,
                reading);

        var secondResult =
            await repository.AddAsync(
                result,
                reading);

        Assert.True(firstResult);
        Assert.False(secondResult);

        Assert.Equal(
            1,
            await dbContext.RuleResults.CountAsync());
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

        var added =
            await repository.AddAsync(alert);

        Assert.True(added);

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
    public async Task AlertRepository_ShouldReturnFalseForDuplicate()
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

        var firstResult =
            await repository.AddAsync(alert);

        var secondResult =
            await repository.AddAsync(alert);

        Assert.True(firstResult);
        Assert.False(secondResult);

        Assert.Equal(
            1,
            await dbContext.Alerts.CountAsync());
    }
}