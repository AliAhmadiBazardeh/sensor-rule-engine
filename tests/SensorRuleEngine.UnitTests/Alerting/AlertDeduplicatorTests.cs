using SensorRuleEngine.Domain.Alerting;
using SensorRuleEngine.Domain.Entities;
using Xunit;

namespace SensorRuleEngine.UnitTests.Alerting;

public sealed class AlertDeduplicatorTests
{
    [Fact]
    public void ShouldAcceptFirstAlert()
    {
        var deduplicator = new AlertDeduplicator();

        var alert = CreateAlert();

        Assert.True(deduplicator.TryAdd(alert));
    }

    [Fact]
    public void ShouldRejectExactDuplicate()
    {
        var deduplicator = new AlertDeduplicator();

        var alert = CreateAlert();

        Assert.True(deduplicator.TryAdd(alert));
        Assert.False(deduplicator.TryAdd(alert));
    }

    [Fact]
    public void ShouldAcceptDifferentEpisodes()
    {
        var deduplicator = new AlertDeduplicator();

        var first = CreateAlert(
            "2026-01-01T00:00:00Z");

        var second = CreateAlert(
            "2026-01-01T00:10:00Z");

        Assert.True(deduplicator.TryAdd(first));
        Assert.True(deduplicator.TryAdd(second));
    }

    private static Alert CreateAlert(
        string start = "2026-01-01T00:00:00Z")
    {
        var startTimestamp =
            DateTimeOffset.Parse(start);

        return new Alert
        {
            RuleId = "rule-1",
            DeviceId = "device-1",
            Metric = "temperature",
            StartTimestamp = startTimestamp,
            EndTimestamp = startTimestamp.AddMinutes(5),
            PeakValue = 110
        };
    }
}