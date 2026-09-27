using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Persistence;

public interface IRuleResultRepository
{
    Task AddAsync(
        RuleResult result,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlySet<string>> GetExistingKeysAsync(
        IReadOnlyCollection<RuleResult> results,
        CancellationToken cancellationToken = default);
}