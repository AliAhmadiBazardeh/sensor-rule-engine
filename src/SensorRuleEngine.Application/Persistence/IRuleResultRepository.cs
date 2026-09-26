using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Persistence;

public interface IRuleResultRepository
{
    Task<bool> AddAsync(
        RuleResult result,
        SensorReading reading,
        CancellationToken cancellationToken = default);
}