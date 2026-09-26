using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Rules;

public interface IRuleOperator
{
    RuleResult Evaluate(
        SensorReading reading,
        Rule rule);
}