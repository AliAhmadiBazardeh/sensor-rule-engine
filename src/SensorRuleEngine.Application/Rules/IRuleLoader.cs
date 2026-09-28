using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Rules;

public interface IRuleLoader
{
    Task<RuleLoaderResult> LoadAsync(
        Stream input,
        CancellationToken cancellationToken = default);
}