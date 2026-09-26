using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Domain.ValueObjects;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public sealed class RuleEvaluationServiceTests
{
    [Fact]
    public void Evaluate_ShouldReturnSatisfiedResult_WhenRuleIsSatisfied()
    {
        var reading = CreateReading(75);
        var rule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var service = CreateService();

        var results = service.Evaluate(
            reading,
            new[] { rule });

        var result = Assert.Single(results);

        Assert.Equal(
            RuleResultStatus.Satisfied,
            result.Status);

        Assert.Equal(
            rule.Id,
            result.RuleId);
    }

    [Fact]
    public void Evaluate_ShouldReturnViolatedResult_WhenRuleIsViolated()
    {
        var reading = CreateReading(82);
        var rule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            });

        var service = CreateService();

        var results = service.Evaluate(
            reading,
            new[] { rule });

        var result = Assert.Single(results);

        Assert.Equal(
            RuleResultStatus.Violated,
            result.Status);

        Assert.Equal(
            rule.Id,
            result.RuleId);

        Assert.False(
            string.IsNullOrWhiteSpace(result.Reason));
    }

    [Fact]
    public void Evaluate_ShouldIgnoreRule_WhenRuleIsDisabled()
    {
        var reading = CreateReading(75);

        var rule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            },
            enabled: false);

        var service = CreateService();

        var results = service.Evaluate(
            reading,
            new[] { rule });

        Assert.Empty(results);
    }

    [Fact]
    public void Evaluate_ShouldIgnoreRule_WhenMetricDoesNotMatch()
    {
        var reading = CreateReading(
            value: 82,
            metric: "temperature");

        var rule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            },
            metric: "pressure");

        var service = CreateService();

        var results = service.Evaluate(
            reading,
            new[] { rule });

        Assert.Empty(results);
    }

    [Fact]
    public void Evaluate_ShouldIgnoreRule_WhenDeviceDoesNotMatch()
    {
        var reading = CreateReading(
            value: 82,
            deviceId: "PUMP-01");

        var rule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            },
            deviceId: "PUMP-02");

        var service = CreateService();

        var results = service.Evaluate(
            reading,
            new[] { rule });

        Assert.Empty(results);
    }

    [Fact]
    public void Evaluate_ShouldApplyRuleToAllDevices_WhenRuleHasNoDeviceId()
    {
        var reading = CreateReading(
            value: 82,
            deviceId: "PUMP-02");

        var rule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            },
            deviceId: null);

        var service = CreateService();

        var results = service.Evaluate(
            reading,
            new[] { rule });

        Assert.Single(results);
    }

    [Fact]
    public void Evaluate_ShouldEvaluateMultipleApplicableRules()
    {
        var reading = CreateReading(82);

        var firstRule = CreateRule(
            RuleOperatorType.GreaterThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80
            },
            id: "rule-1");

        var secondRule = CreateRule(
            RuleOperatorType.LessThan,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 100
            },
            id: "rule-2");

        var service = CreateService();

        var results = service.Evaluate(
            reading,
            new[] { firstRule, secondRule });

        Assert.Equal(2, results.Count);

        Assert.Contains(
            results,
            result => result.RuleId == "rule-1");

        Assert.Contains(
            results,
            result => result.RuleId == "rule-2");
    }

    [Fact]
    public void Evaluate_ShouldNotEvaluateSustainedAbove()
    {
        var reading = CreateReading(90);

        var rule = CreateRule(
            RuleOperatorType.SustainedAbove,
            new Dictionary<string, decimal>
            {
                ["threshold"] = 80,
                ["durationSeconds"] = 60
            });

        var service = CreateService();

        var results = service.Evaluate(
            reading,
            new[] { rule });

        Assert.Empty(results);
    }

    private static RuleEvaluationService CreateService()
    {
        var operators = new IRuleOperator[]
        {
            new GreaterThanOperator(),
            new GreaterThanOrEqualOperator(),
            new LessThanOperator(),
            new LessThanOrEqualOperator(),
            new EqualOperator(),
            new BetweenOperator()
        };

        return new RuleEvaluationService(
            new RuleApplicabilityChecker(),
            new RuleOperatorResolver(operators));
    }

    private static SensorReading CreateReading(
        decimal value,
        string deviceId = "PUMP-01",
        string metric = "temperature")
    {
        return new SensorReading
        {
            DeviceId = deviceId,
            Metric = metric,
            Timestamp = DateTimeOffset.Parse(
                "2025-06-01T08:00:00Z"),
            Value = value,
            Sequence = 1
        };
    }

    private static Rule CreateRule(
        RuleOperatorType operatorType,
        Dictionary<string, decimal> parameters,
        string id = "test-rule",
        string metric = "temperature",
        string? deviceId = "PUMP-01",
        bool enabled = true)
    {
        return new Rule
        {
            Id = id,
            Name = "Test Rule",
            Enabled = enabled,
            Metric = metric,
            DeviceId = deviceId,
            Operator = operatorType,
            Parameters = parameters
        };
    }
}