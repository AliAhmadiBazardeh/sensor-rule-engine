using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Domain.Rules;

public sealed class EqualOperator : IRuleOperator
{
    public RuleResult Evaluate(
        SensorReading reading,
        Rule rule)
    {
        var expected = rule.Parameters["value"];

        var violated = reading.Value == expected;

        return RuleResultFactory.Create(
            reading,
            rule,
            violated
                ? RuleResultStatus.Violated
                : RuleResultStatus.Satisfied,
            violated
                ? $"Value {reading.Value} is equal to {expected}."
                : null);
    }
}