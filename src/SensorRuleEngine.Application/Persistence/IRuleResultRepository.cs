using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Persistence;

public interface IRuleResultRepository
{
    Task AddAsync(
        RuleResult result,
        CancellationToken cancellationToken = default);
}