using SensorRuleEngine.Application.Processing;
using SensorRuleEngine.Domain.Alerting;
using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Domain.Rules.SustainedAbove;
using Xunit;

namespace SensorRuleEngine.UnitTests.Processing;

public sealed class ReadingProcessingServiceAlertingTests
{
    [Fact]
    public void Process_ShouldSuppressSecondAlert_WhenEpisodeGapIsWithinCooldown()
    {
        var service = CreateService();

        var readings = new[]
        {
            Reading("00:00", 101),
            Reading("00:02", 105),
            Reading("00:03", 90),

            // Gap from previous episode end = 3 minutes
            Reading("00:06", 101),
            Reading("00:08", 110),
            Reading("00:09", 90)
        };

        var result = service.Process(
            readings,
            new[] { CreateRule() });

        var alert = Assert.Single(result.Alerts);

        Assert.Equal(
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
            alert.StartTimestamp);
    }

    [Fact]
    public void Process_ShouldEmitSecondAlert_WhenEpisodeGapIsGreaterThanCooldown()
    {
        var service = CreateService();

        var readings = new[]
        {
            Reading("00:00", 101),
            Reading("00:02", 105),
            Reading("00:03", 90),

            // Gap from previous episode end = 6 minutes
            Reading("00:09", 101),
            Reading("00:11", 110),
            Reading("00:12", 90)
        };

        var result = service.Process(
            readings,
            new[] { CreateRule() });

        Assert.Equal(2, result.Alerts.Count);

        Assert.Equal(
            DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
            result.Alerts[0].StartTimestamp);

        Assert.Equal(
            DateTimeOffset.Parse("2026-01-01T00:09:00Z"),
            result.Alerts[1].StartTimestamp);
    }

    [Fact]
    public void Process_ShouldNotReturnDuplicateAlert_ForSameEpisode()
    {
        var service = CreateService();

        var readings = new[]
        {
            Reading("00:00", 101),
            Reading("00:02", 105),
            Reading("00:03", 90)
        };

        var rules = new[] { CreateRule() };

        var firstResult =
            service.Process(readings, rules);

        var secondResult =
            service.Process(readings, rules);

        Assert.Single(firstResult.Alerts);
        Assert.Empty(secondResult.Alerts);
    }

    private static ReadingProcessingService CreateService()
    {
        return new ReadingProcessingService(
            new RuleEvaluationService(
                new RuleApplicabilityChecker(),
                new RuleOperatorResolver(
                    new IRuleOperator[]
                    {
                        new GreaterThanOperator(),
                        new GreaterThanOrEqualOperator(),
                        new LessThanOperator(),
                        new LessThanOrEqualOperator(),
                        new EqualOperator(),
                        new BetweenOperator()
                    })),
            new ReadingClassificationService(),
            new SustainedAboveProcessor(),
            new RuleApplicabilityChecker(),
            new AlertCooldownPolicy(
                TimeSpan.FromMinutes(5)),
            new AlertDeduplicator());
    }

    private static Rule CreateRule()
    {
        return new Rule
        {
            Id = "sustained-1",
            Name = "Sustained temperature",
            Enabled = true,
            Metric = "temperature",
            Operator = RuleOperatorType.SustainedAbove,
            Parameters = new Dictionary<string, decimal>
            {
                ["threshold"] = 100,
                ["durationSeconds"] = 120
            }
        };
    }

    private static SensorReading Reading(
        string time,
        decimal value)
    {
        return new SensorReading
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(
                $"2026-01-01T{time}:00Z"),
            Value = value,
            Sequence = 1
        };
    }
}