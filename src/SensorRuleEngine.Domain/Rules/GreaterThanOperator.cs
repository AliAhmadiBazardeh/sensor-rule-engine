using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Domain.Rules;

public sealed class GreaterThanOperator : IRuleOperator
{
    public RuleResult Evaluate(
        SensorReading reading,
        Rule rule)
    {
        var threshold = rule.Parameters["threshold"];

        var violated = reading.Value > threshold;

        return new RuleResult
        {
            RuleId = rule.Id,
            ReadingKey = new ReadingKey(
                reading.DeviceId,
                reading.Metric,
                reading.Timestamp,
                reading.Sequence),
            Status = violated
                ? RuleResultStatus.Violated
                : RuleResultStatus.Satisfied,
            Reason = violated
                ? $"Value {reading.Value} is greater than threshold {threshold}."
                : null
        };
    }
}