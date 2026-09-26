using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public class BetweenOperatorTests
{
    [Theory]
    [InlineData(79, RuleResultStatus.Satisfied)]
    [InlineData(80, RuleResultStatus.Violated)]
    [InlineData(90, RuleResultStatus.Violated)]
    [InlineData(100, RuleResultStatus.Violated)]
    [InlineData(101, RuleResultStatus.Satisfied)]
    public void Should_evaluate_value_correctly(
        decimal value,
        RuleResultStatus expectedStatus)
    {
        var reading = RuleTestData.CreateReading(value);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.Between,
            new Dictionary<string, decimal>
            {
                ["min"] = 80,
                ["max"] = 100
            });

        var result = new BetweenOperator()
            .Evaluate(reading, rule);

        Assert.Equal(expectedStatus, result.Status);
    }
    
    [Fact]
    public void Should_preserve_rule_and_reading_identity()
    {
        var reading = RuleTestData.CreateReading(90);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.Between,
            new Dictionary<string, decimal>
            {
                ["min"] = 80,
                ["max"] = 100
            });

        var result = new BetweenOperator()
            .Evaluate(reading, rule);

        Assert.Equal("test-rule", result.RuleId);

        Assert.Equal(
            reading.DeviceId,
            result.ReadingKey.DeviceId);

        Assert.Equal(
            reading.Metric,
            result.ReadingKey.Metric);

        Assert.Equal(
            reading.Timestamp,
            result.ReadingKey.Timestamp);

        Assert.Equal(
            reading.Sequence,
            result.ReadingKey.Sequence);
    }
}