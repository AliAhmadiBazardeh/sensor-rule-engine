using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public class LessThanOperatorTests
{
    [Theory]
    [InlineData(79, RuleResultStatus.Violated)]
    [InlineData(80, RuleResultStatus.Satisfied)]
    [InlineData(81, RuleResultStatus.Satisfied)]
    public void Should_evaluate_value_correctly(
        decimal value,
        RuleResultStatus expectedStatus)
    {
        var reading = RuleTestData.CreateReading(value);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.LessThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var result = new LessThanOperator()
            .Evaluate(reading, rule);

        Assert.Equal(expectedStatus, result.Status);
    }
}