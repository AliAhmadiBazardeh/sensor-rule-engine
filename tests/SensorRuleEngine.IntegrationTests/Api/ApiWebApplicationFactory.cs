using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SensorRuleEngine.Infrastructure.Persistence;

namespace SensorRuleEngine.IntegrationTests.Api;

public sealed class ApiWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();

            services.RemoveAll<
                DbContextOptions<SensorRuleEngineDbContext>>();

            services.RemoveAll<
                SensorRuleEngineDbContext>();

            _connection = new SqliteConnection(
                "Data Source=:memory:");

            _connection.Open();

            services.AddDbContext<SensorRuleEngineDbContext>(
                options =>
                    options.UseSqlite(_connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}