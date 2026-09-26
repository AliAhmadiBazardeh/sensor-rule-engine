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

    public async Task<bool> AddAsync(
        Alert alert,
        CancellationToken cancellationToken = default)
    {
        var entity = AlertMapper.ToEntity(alert);

        await _dbContext.Alerts.AddAsync(
            entity,
            cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return true;
        }
        catch (DbUpdateException exception)
            when (exception.IsUniqueConstraintViolation())
        {
            _dbContext.Entry(entity).State =
                EntityState.Detached;

            return false;
        }
    }
}