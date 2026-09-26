using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public class RuleApplicabilityCheckerTests
{
    private readonly RuleApplicabilityChecker _sut = new();

    [Fact]
    public void Should_not_apply_disabled_rule()
    {
        var reading = RuleTestData.CreateReading(90);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var disabledRule = new Rule
        {
            Id = rule.Id,
            Name = rule.Name,
            Enabled = false,
            Metric = rule.Metric,
            DeviceId = rule.DeviceId,
            Operator = rule.Operator,
            Parameters = rule.Parameters
        };

        var result = _sut.IsApplicable(reading, disabledRule);

        Assert.False(result);
    }

    [Fact]
    public void Should_apply_rule_when_metric_matches()
    {
        var reading = RuleTestData.CreateReading(90);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var result = _sut.IsApplicable(reading, rule);

        Assert.True(result);
    }

    [Fact]
    public void Should_not_apply_rule_when_metric_does_not_match()
    {
        var reading = RuleTestData.CreateReading(90);

        var rule = new Rule
        {
            Id = "test-rule",
            Name = "Test Rule",
            Enabled = true,
            Metric = "pressure",
            DeviceId = "PUMP-01",
            Operator = RuleOperatorType.GreaterThan,
            Parameters = new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            }
        };

        var result = _sut.IsApplicable(reading, rule);

        Assert.False(result);
    }

    [Fact]
    public void Should_apply_rule_to_any_device_when_device_id_is_null()
    {
        var reading = RuleTestData.CreateReading(90);

        var rule = new Rule
        {
            Id = "test-rule",
            Name = "Test Rule",
            Enabled = true,
            Metric = "temperature",
            DeviceId = null,
            Operator = RuleOperatorType.GreaterThan,
            Parameters = new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            }
        };

        var result = _sut.IsApplicable(reading, rule);

        Assert.True(result);
    }

    [Fact]
    public void Should_apply_rule_when_device_id_matches()
    {
        var reading = RuleTestData.CreateReading(90);

        var rule = RuleTestData.CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var result = _sut.IsApplicable(reading, rule);

        Assert.True(result);
    }

    [Fact]
    public void Should_not_apply_rule_when_device_id_does_not_match()
    {
        var reading = RuleTestData.CreateReading(90);

        var rule = new Rule
        {
            Id = "test-rule",
            Name = "Test Rule",
            Enabled = true,
            Metric = "temperature",
            DeviceId = "PUMP-02",
            Operator = RuleOperatorType.GreaterThan,
            Parameters = new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            }
        };

        var result = _sut.IsApplicable(reading, rule);

        Assert.False(result);
    }
}