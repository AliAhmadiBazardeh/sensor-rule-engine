using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorRuleEngine.Infrastructure.Persistence;

namespace SensorRuleEngine.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection connection string is not configured.");

        services.AddDbContext<SensorRuleEngineDbContext>(options =>
            options.UseSqlite(connectionString));

        return services;
    }
}