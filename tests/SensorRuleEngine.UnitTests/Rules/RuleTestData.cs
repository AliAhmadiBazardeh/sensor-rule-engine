using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;

namespace SensorRuleEngine.UnitTests.Rules;

internal static class RuleTestData
{
    public static SensorReading CreateReading(decimal value)
    {
        return new SensorReading
        {
            DeviceId = "PUMP-01",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse("2025-06-01T08:33:00Z"),
            Value = value,
            Sequence = 1
        };
    }

    public static Rule CreateRule(
        RuleOperatorType operatorType,
        Dictionary<string, decimal> parameters)
    {
        return new Rule
        {
            Id = "test-rule",
            Name = "Test Rule",
            Enabled = true,
            Metric = "temperature",
            DeviceId = "PUMP-01",
            Operator = operatorType,
            Parameters = parameters
        };
    }
}