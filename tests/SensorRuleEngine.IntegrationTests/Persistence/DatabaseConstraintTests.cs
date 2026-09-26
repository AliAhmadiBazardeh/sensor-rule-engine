using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Infrastructure.Persistence;
using SensorRuleEngine.Infrastructure.Persistence.Entities;
using Xunit;

namespace SensorRuleEngine.IntegrationTests.Persistence;

public sealed class DatabaseConstraintTests
{
    [Fact]
    public async Task Readings_ShouldRejectDuplicateNaturalKey()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SensorRuleEngineDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new SensorRuleEngineDbContext(options);

        await db.Database.EnsureCreatedAsync();

        var reading = new ReadingEntity
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(
                "2026-01-01T00:00:00Z"),
            Value = 100,
            Sequence = 1
        };

        db.Readings.Add(reading);
        await db.SaveChangesAsync();

        db.Readings.Add(new ReadingEntity
        {
            DeviceId = reading.DeviceId,
            Metric = reading.Metric,
            Timestamp = reading.Timestamp,
            Value = 200,
            Sequence = reading.Sequence
        });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => db.SaveChangesAsync());
    }

    [Fact]
    public async Task RuleResults_ShouldRejectDuplicateNaturalKey()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SensorRuleEngineDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new SensorRuleEngineDbContext(options);

        await db.Database.EnsureCreatedAsync();

        var result = new RuleResultEntity
        {
            RuleId = "rule-1",
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(
                "2026-01-01T00:00:00Z"),
            Sequence = 1,
            Status = "Violated",
            Reason = "Test"
        };

        db.RuleResults.Add(result);
        await db.SaveChangesAsync();

        db.RuleResults.Add(new RuleResultEntity
        {
            RuleId = result.RuleId,
            DeviceId = result.DeviceId,
            Metric = result.Metric,
            Timestamp = result.Timestamp,
            Sequence = result.Sequence,
            Status = "Violated",
            Reason = "Duplicate"
        });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Alerts_ShouldRejectDuplicateNaturalKey()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SensorRuleEngineDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var db = new SensorRuleEngineDbContext(options);

        await db.Database.EnsureCreatedAsync();

        var start =
            DateTimeOffset.Parse("2026-01-01T00:00:00Z");

        var end =
            DateTimeOffset.Parse("2026-01-01T00:05:00Z");

        db.Alerts.Add(new AlertEntity
        {
            RuleId = "rule-1",
            DeviceId = "device-1",
            Metric = "temperature",
            StartTimestamp = start,
            EndTimestamp = end,
            PeakValue = 110
        });

        await db.SaveChangesAsync();

        db.Alerts.Add(new AlertEntity
        {
            RuleId = "rule-1",
            DeviceId = "device-1",
            Metric = "temperature",
            StartTimestamp = start,
            EndTimestamp = end,
            PeakValue = 115
        });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => db.SaveChangesAsync());
    }
}