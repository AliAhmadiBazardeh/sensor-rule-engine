using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Rules;

public interface IRuleEvaluationService
{
    IReadOnlyList<RuleResult> Evaluate(
        SensorReading reading,
        IEnumerable<Rule> rules);
}