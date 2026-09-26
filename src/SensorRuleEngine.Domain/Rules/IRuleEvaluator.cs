using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Rules;

public interface IRuleEvaluator
{
    RuleResult Evaluate(
        SensorReading reading,
        Rule rule);
}