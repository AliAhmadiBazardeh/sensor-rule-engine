using SensorRuleEngine.Domain.Alerting;
using SensorRuleEngine.Domain.Entities;
using Xunit;

namespace SensorRuleEngine.UnitTests.Alerting;

public sealed class AlertCooldownPolicyTests
{
    [Fact]
    public void ShouldAllowFirstAlert()
    {
        var policy = new AlertCooldownPolicy(
            TimeSpan.FromMinutes(5));

        var alert = CreateAlert(
            "2026-01-01T00:00:00Z",
            "2026-01-01T00:02:00Z");

        var result = policy.ShouldEmit(
            alert,
            Array.Empty<Alert>());

        Assert.True(result);
    }

    [Fact]
    public void ShouldSuppressAlert_WhenEpisodeGapIsLessThanCooldown()
    {
        var policy = new AlertCooldownPolicy(
            TimeSpan.FromMinutes(5));

        var previous = CreateAlert(
            "2026-01-01T00:00:00Z",
            "2026-01-01T00:10:00Z");

        var current = CreateAlert(
            "2026-01-01T00:12:00Z",
            "2026-01-01T00:15:00Z");

        var result = policy.ShouldEmit(
            current,
            new[] { previous });

        Assert.False(result);
    }

    [Fact]
    public void ShouldSuppressAlert_WhenEpisodeGapEqualsCooldown()
    {
        var policy = new AlertCooldownPolicy(
            TimeSpan.FromMinutes(5));

        var previous = CreateAlert(
            "2026-01-01T00:00:00Z",
            "2026-01-01T00:10:00Z");

        var current = CreateAlert(
            "2026-01-01T00:15:00Z",
            "2026-01-01T00:20:00Z");

        var result = policy.ShouldEmit(
            current,
            new[] { previous });

        Assert.False(result);
    }

    [Fact]
    public void ShouldAllowAlert_WhenEpisodeGapIsGreaterThanCooldown()
    {
        var policy = new AlertCooldownPolicy(
            TimeSpan.FromMinutes(5));

        var previous = CreateAlert(
            "2026-01-01T00:00:00Z",
            "2026-01-01T00:10:00Z");

        var current = CreateAlert(
            "2026-01-01T00:16:00Z",
            "2026-01-01T00:20:00Z");

        var result = policy.ShouldEmit(
            current,
            new[] { previous });

        Assert.True(result);
    }

    [Fact]
    public void ShouldOnlyConsiderPreviousAlertForSameRuleDeviceAndMetric()
    {
        var policy = new AlertCooldownPolicy(
            TimeSpan.FromMinutes(5));

        var previous = CreateAlert(
            "2026-01-01T00:00:00Z",
            "2026-01-01T00:10:00Z");

        previous = new Alert
        {
            RuleId = "different-rule",
            DeviceId = previous.DeviceId,
            Metric = previous.Metric,
            StartTimestamp = previous.StartTimestamp,
            EndTimestamp = previous.EndTimestamp,
            PeakValue = previous.PeakValue
        };

        var current = CreateAlert(
            "2026-01-01T00:12:00Z",
            "2026-01-01T00:15:00Z");

        var result = policy.ShouldEmit(
            current,
            new[] { previous });

        Assert.True(result);
    }

    private static Alert CreateAlert(
        string start,
        string end)
    {
        return new Alert
        {
            RuleId = "rule-1",
            DeviceId = "device-1",
            Metric = "temperature",
            StartTimestamp = DateTimeOffset.Parse(start),
            EndTimestamp = DateTimeOffset.Parse(end),
            PeakValue = 110
        };
    }
}