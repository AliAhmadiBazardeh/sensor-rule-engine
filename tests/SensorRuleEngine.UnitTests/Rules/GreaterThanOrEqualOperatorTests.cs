using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public class GreaterThanOrEqualOperatorTests
{
    [Theory]
    [InlineData(81, RuleResultStatus.Violated)]
    [InlineData(80, RuleResultStatus.Violated)]
    [InlineData(79, RuleResultStatus.Satisfied)]
    public void Should_evaluate_value_correctly(
        decimal value,
        RuleResultStatus expectedStatus)
    {
        var reading = RuleTestData.CreateReading(value);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.GreaterThanOrEqual,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var result = new GreaterThanOrEqualOperator()
            .Evaluate(reading, rule);

        Assert.Equal(expectedStatus, result.Status);
    }
}