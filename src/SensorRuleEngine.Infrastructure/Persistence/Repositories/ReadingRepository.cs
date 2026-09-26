using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Infrastructure.Persistence.Mappers;

namespace SensorRuleEngine.Infrastructure.Persistence.Repositories;

public sealed class ReadingRepository : IReadingRepository
{
    private readonly SensorRuleEngineDbContext _dbContext;

    public ReadingRepository(
        SensorRuleEngineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddAsync(
        SensorReading reading,
        CancellationToken cancellationToken = default)
    {
        var entity = ReadingMapper.ToEntity(reading);

        await _dbContext.Readings.AddAsync(
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