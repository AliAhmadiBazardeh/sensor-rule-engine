using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Application.Rules;
using SensorRuleEngine.Infrastructure.Persistence;
using SensorRuleEngine.Infrastructure.Persistence.Repositories;
using SensorRuleEngine.Infrastructure.Rules;

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
        
        services.AddScoped<IReadingRepository, ReadingRepository>();
        services.AddScoped<IRuleResultRepository, RuleResultRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IProcessingPersistence, ProcessingPersistence>();
        services.AddScoped<IRuleLoader, JsonRuleLoader>();
        
        return services;
    }
}