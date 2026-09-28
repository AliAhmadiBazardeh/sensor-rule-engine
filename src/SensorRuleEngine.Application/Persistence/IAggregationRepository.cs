using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Persistence;

public interface IAggregationRepository
{
    Task<IReadOnlyList<SensorReading>> GetAcceptableReadingsAsync(
        string deviceId,
        string metric,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}