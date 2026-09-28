using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Infrastructure.Persistence.Repositories;

public sealed class AggregationRepository : IAggregationRepository
{
    private readonly SensorRuleEngineDbContext _dbContext;

    public AggregationRepository(
        SensorRuleEngineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<SensorReading>>
        GetAcceptableReadingsAsync(
            string deviceId,
            string metric,
            DateTimeOffset from,
            DateTimeOffset to,
            CancellationToken cancellationToken = default)
    {
        var readings =
            await _dbContext.Readings
                .AsNoTracking()
                .Where(reading =>
                    reading.DeviceId == deviceId &&
                    reading.Metric == metric &&
                    reading.IsAcceptable)
                .Select(reading => new SensorReading
                {
                    DeviceId = reading.DeviceId,
                    Metric = reading.Metric,
                    Timestamp = reading.Timestamp,
                    Value = reading.Value,
                    Sequence = reading.Sequence
                })
                .ToListAsync(cancellationToken);

        return readings
            .Where(reading =>
                reading.Timestamp >= from &&
                reading.Timestamp < to)
            .OrderBy(reading => reading.Timestamp)
            .ThenBy(reading => reading.Sequence)
            .ToList();
    }
}