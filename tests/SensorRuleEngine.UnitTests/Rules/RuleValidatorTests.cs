using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public sealed class RuleValidatorTests
{
    private readonly RuleValidator _validator = new();

    [Fact]
    public void Validate_ShouldAccept_ValidGreaterThanRule()
    {
        var rule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 75
            });

        var result = _validator.Validate(rule);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldRejectRule_WhenThresholdIsMissing()
    {
        var rule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>());

        var result = _validator.Validate(rule);

        Assert.False(result.IsValid);
        Assert.Contains("threshold", result.Errors[0],
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ShouldAccept_ValidBetweenRule()
    {
        var rule = CreateRule(
            RuleOperatorType.Between,
            new Dictionary<string, decimal>
            {
                ["min"] = 10,
                ["max"] = 20
            });

        var result = _validator.Validate(rule);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldRejectBetweenRule_WhenMinIsMissing()
    {
        var rule = CreateRule(
            RuleOperatorType.Between,
            new Dictionary<string, decimal>
            {
                ["max"] = 20
            });

        var result = _validator.Validate(rule);

        Assert.False(result.IsValid);
        Assert.Contains("min", result.Errors[0],
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ShouldRejectBetweenRule_WhenMaxIsMissing()
    {
        var rule = CreateRule(
            RuleOperatorType.Between,
            new Dictionary<string, decimal>
            {
                ["min"] = 10
            });

        var result = _validator.Validate(rule);

        Assert.False(result.IsValid);
        Assert.Contains("max", result.Errors[0],
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ShouldRejectBetweenRule_WhenMinIsGreaterThanMax()
    {
        var rule = CreateRule(
            RuleOperatorType.Between,
            new Dictionary<string, decimal>
            {
                ["min"] = 20,
                ["max"] = 10
            });

        var result = _validator.Validate(rule);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ShouldAccept_ValidSustainedAboveRule()
    {
        var rule = CreateRule(
            RuleOperatorType.SustainedAbove,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 75,
                ["durationSeconds"] = 180
            });

        var result = _validator.Validate(rule);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldRejectSustainedAbove_WhenDurationIsMissing()
    {
        var rule = CreateRule(
            RuleOperatorType.SustainedAbove,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 75
            });

        var result = _validator.Validate(rule);

        Assert.False(result.IsValid);
        Assert.Contains("durationSeconds", result.Errors[0],
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_ShouldRejectSustainedAbove_WhenDurationIsZero()
    {
        var rule = CreateRule(
            RuleOperatorType.SustainedAbove,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 75,
                ["durationSeconds"] = 0
            });

        var result = _validator.Validate(rule);

        Assert.False(result.IsValid);
    }

    private static Rule CreateRule(
        RuleOperatorType operatorType,
        Dictionary<string, decimal> parameters)
    {
        return new Rule
        {
            Id = "rule-1",
            Name = "Test Rule",
            Enabled = true,
            Metric = "temperature",
            DeviceId = "device-1",
            Operator = operatorType,
            Parameters = parameters
        };
    }
}