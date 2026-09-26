using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Persistence;

public interface IReadingRepository
{
    Task<bool> AddAsync(
        SensorReading reading,
        CancellationToken cancellationToken = default);
}