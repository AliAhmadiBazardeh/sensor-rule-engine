using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public class GreaterThanOperatorTests
{
    [Fact]
    public void Should_return_violated_when_value_is_greater_than_threshold()
    {
        var reading = new SensorReading
        {
            DeviceId = "PUMP-01",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse("2025-06-01T08:33:00Z"),
            Value = 90,
            Sequence = 1
        };

        var rule = new Rule
        {
            Id = "high-temperature",
            Name = "High Temperature",
            Enabled = true,
            Metric = "temperature",
            DeviceId = "PUMP-01",
            Operator = RuleOperatorType.GreaterThan,
            Parameters = new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            }
        };

        var sut = new GreaterThanOperator();

        var result = sut.Evaluate(reading, rule);

        Assert.Equal(
            RuleResultStatus.Violated,
            result.Status);

        Assert.Equal(
            "high-temperature",
            result.RuleId);

        Assert.NotNull(result.Reason);
    }

    [Fact]
    public void Should_return_satisfied_when_value_is_not_greater_than_threshold()
    {
        var reading = new SensorReading
        {
            DeviceId = "PUMP-01",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse("2025-06-01T08:33:00Z"),
            Value = 70,
            Sequence = 1
        };

        var rule = new Rule
        {
            Id = "high-temperature",
            Name = "High Temperature",
            Enabled = true,
            Metric = "temperature",
            DeviceId = "PUMP-01",
            Operator = RuleOperatorType.GreaterThan,
            Parameters = new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            }
        };

        var sut = new GreaterThanOperator();

        var result = sut.Evaluate(reading, rule);

        Assert.Equal(
            RuleResultStatus.Satisfied,
            result.Status);

        Assert.Null(result.Reason);
    }
}