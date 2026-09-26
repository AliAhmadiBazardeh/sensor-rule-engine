using SensorRuleEngine.Application.Processing;
using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Domain.Rules.SustainedAbove;
using Xunit;

namespace SensorRuleEngine.UnitTests.Processing;

public sealed class ReadingProcessingServiceTests
{
    [Fact]
    public void Process_ShouldOrderReadingsByEventTime()
    {
        var service = CreateService();

        var readings = new[]
        {
            CreateReading("2026-01-01T00:02:00Z", 10),
            CreateReading("2026-01-01T00:01:00Z", 10)
        };

        var rules = new[]
        {
            new Rule
            {
                Id = "rule-1",
                Name = "Temperature",
                Enabled = true,
                Metric = "temperature",
                Operator = RuleOperatorType.GreaterThan,
                Parameters = new Dictionary<string, decimal>
                {
                    ["threshold"] = 100
                }
            }
        };

        var result = service.Process(readings, rules);

        Assert.Equal(2, result.ProcessedReadings.Count);
        Assert.Equal(
            readings[1].Timestamp,
            result.ProcessedReadings[0].Timestamp);
        Assert.Equal(
            readings[0].Timestamp,
            result.ProcessedReadings[1].Timestamp);
    }

    [Fact]
    public void Process_ShouldEvaluateStatelessRulesAndClassifyReading()
    {
        var service = CreateService();

        var readings = new[]
        {
            CreateReading("2026-01-01T00:00:00Z", 120)
        };

        var rules = new[]
        {
            new Rule
            {
                Id = "rule-1",
                Name = "Temperature limit",
                Enabled = true,
                Metric = "temperature",
                Operator = RuleOperatorType.GreaterThan,
                Parameters = new Dictionary<string, decimal>
                {
                    ["threshold"] = 100
                }
            }
        };

        var result = service.Process(readings, rules);

        var classification = Assert.Single(
            result.Classifications);

        Assert.Equal(
            ReadingClassificationStatus.Unacceptable,
            classification.Status);

        Assert.Single(classification.Violations);
        Assert.Equal("rule-1", classification.Violations[0].RuleId);
    }

    [Fact]
    public void Process_ShouldSendSustainedAboveToStatefulProcessor()
    {
        var service = CreateService();

        var readings = new[]
        {
            CreateReading("2026-01-01T00:00:00Z", 101),
            CreateReading("2026-01-01T00:01:00Z", 105),
            CreateReading("2026-01-01T00:02:00Z", 90)
        };

        var rules = new[]
        {
            new Rule
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
            }
        };

        var result = service.Process(readings, rules);

        var alert = Assert.Single(result.Alerts);

        Assert.Equal("sustained-1", alert.RuleId);
        Assert.Equal(
            readings[0].Timestamp,
            alert.StartTimestamp);
        Assert.Equal(
            readings[2].Timestamp,
            alert.EndTimestamp);
        Assert.Equal(105, alert.PeakValue);
    }

    [Fact]
    public void Process_ShouldNotClassifySustainedAboveAsPerReadingViolation()
    {
        var service = CreateService();

        var readings = new[]
        {
            CreateReading("2026-01-01T00:00:00Z", 101),
            CreateReading("2026-01-01T00:01:00Z", 105)
        };

        var rules = new[]
        {
            new Rule
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
            }
        };

        var result = service.Process(readings, rules);

        Assert.Equal(2, result.Classifications.Count);

        Assert.All(
            result.Classifications,
            classification =>
                Assert.Equal(
                    ReadingClassificationStatus.Acceptable,
                    classification.Status));
    }

    [Fact]
    public void Process_ShouldCompleteOpenSustainedEpisode()
    {
        var service = CreateService();

        var readings = new[]
        {
            CreateReading("2026-01-01T00:00:00Z", 101),
            CreateReading("2026-01-01T00:01:00Z", 105),
            CreateReading("2026-01-01T00:02:00Z", 110)
        };

        var rules = new[]
        {
            new Rule
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
            }
        };

        var result = service.Process(readings, rules);

        var alert = Assert.Single(result.Alerts);

        Assert.Equal(
            readings[0].Timestamp,
            alert.StartTimestamp);

        Assert.Equal(
            readings[2].Timestamp,
            alert.EndTimestamp);
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
            new SustainedAboveProcessor());
    }

    private static SensorReading CreateReading(
        string timestamp,
        decimal value)
    {
        return new SensorReading
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(timestamp),
            Value = value,
            Sequence = 1
        };
    }
}