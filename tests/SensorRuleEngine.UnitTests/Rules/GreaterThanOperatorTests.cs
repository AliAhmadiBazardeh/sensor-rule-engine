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
        var reading = RuleTestData.CreateReading(81);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var sut = new GreaterThanOperator();

        var result = sut.Evaluate(reading, rule);

        Assert.Equal(RuleResultStatus.Violated, result.Status);
        Assert.Equal("test-rule", result.RuleId);
        Assert.NotNull(result.Reason);
    }
    
    [Fact]
    public void Should_return_satisfied_when_value_equals_threshold()
    {
        var reading = RuleTestData.CreateReading(80);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var result = new GreaterThanOperator()
            .Evaluate(reading, rule);

        Assert.Equal(RuleResultStatus.Satisfied, result.Status);
        Assert.Null(result.Reason);
    }
    
    [Fact]
    public void Should_return_satisfied_when_value_is_less_than_threshold()
    {
        var reading = RuleTestData.CreateReading(79);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var result = new GreaterThanOperator()
            .Evaluate(reading, rule);

        Assert.Equal(RuleResultStatus.Satisfied, result.Status);
    }
}