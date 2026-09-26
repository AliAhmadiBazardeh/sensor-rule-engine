using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public class EqualOperatorTests
{
    [Theory]
    [InlineData(80, RuleResultStatus.Violated)]
    [InlineData(79, RuleResultStatus.Satisfied)]
    [InlineData(81, RuleResultStatus.Satisfied)]
    public void Should_evaluate_value_correctly(
        decimal value,
        RuleResultStatus expectedStatus)
    {
        var reading = RuleTestData.CreateReading(value);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.Equal,
            new Dictionary<string, decimal>
            {
                ["value"] = 80
            });

        var result = new EqualOperator()
            .Evaluate(reading, rule);

        Assert.Equal(expectedStatus, result.Status);
    }
}