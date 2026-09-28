using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Rules;

public sealed class InMemoryRuleProvider : IRuleProvider
{
    private readonly IReadOnlyList<Rule> _rules;

    public InMemoryRuleProvider(
        IReadOnlyList<Rule> rules)
    {
        _rules = rules;
    }

    public IReadOnlyList<Rule> GetRules()
    {
        return _rules;
    }
}