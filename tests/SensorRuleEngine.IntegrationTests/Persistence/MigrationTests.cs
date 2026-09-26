using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Infrastructure.Persistence;
using Xunit;

namespace SensorRuleEngine.IntegrationTests.Persistence;

public sealed class MigrationTests
{
    [Fact]
    public async Task Database_ShouldContainExpectedSchema_AfterMigration()
    {
        await using var connection = new SqliteConnection(
            "Data Source=:memory:");

        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<SensorRuleEngineDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext =
            new SensorRuleEngineDbContext(options);

        await dbContext.Database.MigrateAsync();

        var tables = await GetTableNamesAsync(dbContext);

        Assert.Contains("Readings", tables);
        Assert.Contains("RuleResults", tables);
        Assert.Contains("Alerts", tables);
        Assert.Contains("__EFMigrationsHistory", tables);
    }

    private static async Task<List<string>> GetTableNamesAsync(
        SensorRuleEngineDbContext dbContext)
    {
        await using var command =
            dbContext.Database.GetDbConnection().CreateCommand();

        command.CommandText =
            """
            SELECT name
            FROM sqlite_master
            WHERE type = 'table';
            """;

        await using var reader = await command.ExecuteReaderAsync();

        var tables = new List<string>();

        while (await reader.ReadAsync())
        {
            tables.Add(reader.GetString(0));
        }

        return tables;
    }
}