using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;

namespace SensorRuleEngine.Domain.Rules;

public sealed class BetweenOperator : IRuleOperator
{
    public RuleResult Evaluate(
        SensorReading reading,
        Rule rule)
    {
        var min = rule.Parameters["min"];
        var max = rule.Parameters["max"];

        var violated =
            reading.Value >= min &&
            reading.Value <= max;

        return RuleResultFactory.Create(
            reading,
            rule,
            violated
                ? RuleResultStatus.Violated
                : RuleResultStatus.Satisfied,
            violated
                ? $"Value {reading.Value} is between {min} and {max}."
                : null);
    }
}