using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Application.Persistence;

public interface IReadingRepository
{
    Task AddAsync(
        SensorReading reading,
        bool isAcceptable,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlySet<ReadingKey>> GetExistingKeysAsync(
        IReadOnlyCollection<ReadingKey> keys,
        CancellationToken cancellationToken = default);
}