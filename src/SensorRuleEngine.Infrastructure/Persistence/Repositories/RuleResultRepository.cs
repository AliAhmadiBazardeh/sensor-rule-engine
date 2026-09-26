using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Infrastructure.Persistence.Mappers;

namespace SensorRuleEngine.Infrastructure.Persistence.Repositories;

public sealed class RuleResultRepository
    : IRuleResultRepository
{
    private readonly SensorRuleEngineDbContext _dbContext;

    public RuleResultRepository(
        SensorRuleEngineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddAsync(
        RuleResult result,
        SensorReading reading,
        CancellationToken cancellationToken = default)
    {
        var entity = RuleResultMapper.ToEntity(result);

        await _dbContext.RuleResults.AddAsync(
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