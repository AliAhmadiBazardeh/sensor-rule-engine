using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Rules;

public interface IRuleProvider
{
    IReadOnlyList<Rule> GetRules();
}