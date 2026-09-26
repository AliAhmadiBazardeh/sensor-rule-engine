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

    public async Task AddAsync(
        RuleResult result,
        CancellationToken cancellationToken = default)
    {
        var entity = RuleResultMapper.ToEntity(result);

        await _dbContext.RuleResults.AddAsync(
            entity,
            cancellationToken);
    }
}