using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules.SustainedAbove;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules.SustainedAbove;

public sealed class SustainedAboveProcessorTests
{
    [Fact]
    public void Process_ShouldNotCreateAlert_WhenDurationIsInsufficient()
    {
        var processor = new SustainedAboveProcessor();

        var rule = CreateRule(durationSeconds: 180);

        processor.Process(
            CreateReading("08:00:00", 80),
            rule);

        var alert = processor.Process(
            CreateReading("08:02:00", 90),
            rule);

        Assert.Null(alert);
    }

    [Fact]
    public void Process_ShouldCreateAlert_WhenDurationReachesThreshold()
    {
        var processor = new SustainedAboveProcessor();

        var rule = CreateRule(durationSeconds: 180);

        processor.Process(
            CreateReading("08:00:00", 80),
            rule);

        processor.Process(
            CreateReading("08:01:00", 90),
            rule);

        var alert = processor.Process(
            CreateReading("08:03:00", 70),
            rule);

        Assert.NotNull(alert);
        Assert.Equal("rule-1", alert.RuleId);
        Assert.Equal("device-1", alert.DeviceId);
        Assert.Equal("temperature", alert.Metric);
        Assert.Equal(
            CreateTimestamp("08:00:00"),
            alert.StartTimestamp);
        Assert.Equal(
            CreateTimestamp("08:03:00"),
            alert.EndTimestamp);
    }

    [Fact]
    public void Process_ShouldCloseEpisode_WhenValueReachesThreshold()
    {
        var processor = new SustainedAboveProcessor();

        var rule = CreateRule(durationSeconds: 180);

        processor.Process(
            CreateReading("08:00:00", 80),
            rule);

        processor.Process(
            CreateReading("08:02:00", 90),
            rule);

        var alert = processor.Process(
            CreateReading("08:02:00", 75),
            rule);

        Assert.Null(alert);
    }

    [Fact]
    public void Process_ShouldTrackPeakValue()
    {
        var processor = new SustainedAboveProcessor();

        var rule = CreateRule(durationSeconds: 180);

        processor.Process(
            CreateReading("08:00:00", 80),
            rule);

        processor.Process(
            CreateReading("08:01:00", 120),
            rule);

        processor.Process(
            CreateReading("08:02:00", 100),
            rule);

        var alert = processor.Process(
            CreateReading("08:03:00", 70),
            rule);

        Assert.NotNull(alert);
        Assert.Equal(120, alert.PeakValue);
    }

    [Fact]
    public void Process_ShouldCreateSeparateAlerts_ForSeparateEpisodes()
    {
        var processor = new SustainedAboveProcessor();

        var rule = CreateRule(durationSeconds: 120);

        processor.Process(
            CreateReading("08:00:00", 80),
            rule);

        var firstAlert = processor.Process(
            CreateReading("08:02:00", 70),
            rule);

        processor.Process(
            CreateReading("08:05:00", 90),
            rule);

        var secondAlert = processor.Process(
            CreateReading("08:07:00", 70),
            rule);

        Assert.NotNull(firstAlert);
        Assert.NotNull(secondAlert);

        Assert.Equal(
            CreateTimestamp("08:00:00"),
            firstAlert.StartTimestamp);

        Assert.Equal(
            CreateTimestamp("08:05:00"),
            secondAlert.StartTimestamp);
    }

    [Fact]
    public void Complete_ShouldCreateAlert_ForOpenQualifiedEpisode()
    {
        var processor = new SustainedAboveProcessor();

        var rule = CreateRule(durationSeconds: 180);

        processor.Process(
            CreateReading("08:00:00", 80),
            rule);

        processor.Process(
            CreateReading("08:01:00", 100),
            rule);

        processor.Process(
            CreateReading("08:03:00", 110),
            rule);

        var alerts = processor.Complete();

        var alert = Assert.Single(alerts);

        Assert.Equal(
            CreateTimestamp("08:00:00"),
            alert.StartTimestamp);

        Assert.Equal(
            CreateTimestamp("08:03:00"),
            alert.EndTimestamp);

        Assert.Equal(110, alert.PeakValue);
    }

    private static Rule CreateRule(decimal durationSeconds)
    {
        return new Rule
        {
            Id = "rule-1",
            Name = "Temperature sustained above threshold",
            Enabled = true,
            Metric = "temperature",
            DeviceId = "device-1",
            Operator = RuleOperatorType.SustainedAbove,
            Parameters = new Dictionary<string, decimal>
            {
                ["threshold"] = 75,
                ["durationSeconds"] = durationSeconds
            }
        };
    }

    private static SensorReading CreateReading(
        string time,
        decimal value)
    {
        return new SensorReading
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = CreateTimestamp(time),
            Value = value,
            Sequence = 1
        };
    }

    private static DateTimeOffset CreateTimestamp(string time)
    {
        return DateTimeOffset.Parse(
            $"2025-06-01T{time}+00:00");
    }
}