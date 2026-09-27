using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.ValueObjects;
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

    public async Task AddAsync(
        SensorReading reading,
        CancellationToken cancellationToken = default)
    {
        var entity = ReadingMapper.ToEntity(reading);

        await _dbContext.Readings.AddAsync(
            entity,
            cancellationToken);
    }
    
    public async Task<IReadOnlySet<ReadingKey>> GetExistingKeysAsync(
        IReadOnlyCollection<ReadingKey> keys,
        CancellationToken cancellationToken = default)
    {
        if (keys.Count == 0)
        {
            return new HashSet<ReadingKey>();
        }

        var deviceIds = keys
            .Select(key => key.DeviceId)
            .Distinct()
            .ToList();

        var metrics = keys
            .Select(key => key.Metric)
            .Distinct()
            .ToList();

        var timestamps = keys
            .Select(key => key.Timestamp)
            .Distinct()
            .ToList();

        var sequences = keys
            .Select(key => key.Sequence)
            .Distinct()
            .ToList();

        var existingReadings =
            await _dbContext.Readings
                .AsNoTracking()
                .Where(reading =>
                    deviceIds.Contains(reading.DeviceId) &&
                    metrics.Contains(reading.Metric) &&
                    timestamps.Contains(reading.Timestamp) &&
                    sequences.Contains(reading.Sequence))
                .Select(reading => new ReadingKey(
                    reading.DeviceId,
                    reading.Metric,
                    reading.Timestamp,
                    reading.Sequence))
                .ToListAsync(cancellationToken);

        return existingReadings.ToHashSet();
    }
}