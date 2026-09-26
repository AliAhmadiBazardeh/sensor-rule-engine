using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Infrastructure.Persistence.Mappers;

namespace SensorRuleEngine.Infrastructure.Persistence.Repositories;

public sealed class AlertRepository : IAlertRepository
{
    private readonly SensorRuleEngineDbContext _dbContext;

    public AlertRepository(
        SensorRuleEngineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Alert alert,
        CancellationToken cancellationToken = default)
    {
        var entity = AlertMapper.ToEntity(alert);

        await _dbContext.Alerts.AddAsync(
            entity,
            cancellationToken);
        
    }
}