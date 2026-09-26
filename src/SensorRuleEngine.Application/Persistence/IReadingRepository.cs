using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Persistence;

public interface IReadingRepository
{
    Task AddAsync(
        SensorReading reading,
        CancellationToken cancellationToken = default);
}