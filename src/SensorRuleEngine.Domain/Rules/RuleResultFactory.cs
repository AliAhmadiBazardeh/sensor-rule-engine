using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Domain.Rules;

internal static class RuleResultFactory
{
    public static RuleResult Create(
        SensorReading reading,
        Rule rule,
        RuleResultStatus status,
        string? reason = null)
    {
        return new RuleResult
        {
            RuleId = rule.Id,
            ReadingKey = new ReadingKey(
                reading.DeviceId,
                reading.Metric,
                reading.Timestamp,
                reading.Sequence),
            Status = status,
            Reason = reason
        };
    }
}