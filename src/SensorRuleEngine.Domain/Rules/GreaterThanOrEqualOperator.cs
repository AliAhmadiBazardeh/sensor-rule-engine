using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Domain.Rules;

public sealed class GreaterThanOrEqualOperator : IRuleOperator
{
    public RuleResult Evaluate(
        SensorReading reading,
        Rule rule)
    {
        var threshold = rule.Parameters["threshold"];

        var violated = reading.Value >= threshold;

        return RuleResultFactory.Create(
            reading,
            rule,
            violated
                ? RuleResultStatus.Violated
                : RuleResultStatus.Satisfied,
            violated
                ? $"Value {reading.Value} is greater than or equal to threshold {threshold}."
                : null);
    }
}