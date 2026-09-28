using System.Text;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Infrastructure.Rules;
using Xunit;

namespace SensorRuleEngine.UnitTests.Rules;

public sealed class JsonRuleLoaderTests
{
    [Fact]
    public async Task LoadAsync_ShouldLoadAllValidRules()
    {
        const string json =
            """
            [
              {
                "id": "overheat-pump01",
                "name": "Overheating Sustained",
                "enabled": true,
                "deviceId": "PUMP-01",
                "metric": "temperature",
                "operator": "SustainedAbove",
                "threshold": 80,
                "durationSeconds": 30
              },
              {
                "id": "high-pressure-all",
                "name": "High Pressure",
                "enabled": true,
                "metric": "pressure",
                "operator": "GreaterThan",
                "threshold": 100
              },
              {
                "id": "vibration-between",
                "name": "Vibration Normal Range",
                "enabled": true,
                "deviceId": "PUMP-02",
                "metric": "vibration",
                "operator": "Between",
                "min": 10,
                "max": 20
              }
            ]
            """;

        await using var stream =
            new MemoryStream(Encoding.UTF8.GetBytes(json));

        var loader = new JsonRuleLoader(
            new RuleValidator());

        var result = await loader.LoadAsync(stream);

        Assert.Equal(3, result.TotalRules);
        Assert.Equal(3, result.Rules.Count);
        Assert.Equal(0, result.InvalidRules);
    }

    [Fact]
    public async Task LoadAsync_ShouldMapSustainedAboveParameters()
    {
        const string json =
            """
            [
              {
                "id": "overheat-pump01",
                "name": "Overheating Sustained",
                "enabled": true,
                "deviceId": "PUMP-01",
                "metric": "temperature",
                "operator": "SustainedAbove",
                "threshold": 80,
                "durationSeconds": 30
              }
            ]
            """;

        await using var stream =
            new MemoryStream(Encoding.UTF8.GetBytes(json));

        var loader = new JsonRuleLoader(
            new RuleValidator());

        var result = await loader.LoadAsync(stream);

        var rule = Assert.Single(result.Rules);

        Assert.Equal(
            RuleOperatorType.SustainedAbove,
            rule.Operator);

        Assert.Equal(
            80m,
            rule.Parameters["threshold"]);

        Assert.Equal(
            30m,
            rule.Parameters["durationSeconds"]);

        Assert.Equal(
            "PUMP-01",
            rule.DeviceId);
    }

    [Fact]
    public async Task LoadAsync_ShouldRejectSustainedAboveWithoutDuration()
    {
        const string json =
            """
            [
              {
                "id": "invalid-rule",
                "name": "Invalid Sustained Rule",
                "enabled": true,
                "metric": "temperature",
                "operator": "SustainedAbove",
                "threshold": 80
              }
            ]
            """;

        await using var stream =
            new MemoryStream(Encoding.UTF8.GetBytes(json));

        var loader = new JsonRuleLoader(
            new RuleValidator());

        var result = await loader.LoadAsync(stream);

        Assert.Equal(1, result.TotalRules);
        Assert.Empty(result.Rules);
        Assert.Equal(1, result.InvalidRules);
    }

    [Fact]
    public async Task LoadAsync_ShouldRejectUnknownOperator()
    {
        const string json =
            """
            [
              {
                "id": "invalid-rule",
                "name": "Invalid Operator",
                "enabled": true,
                "metric": "temperature",
                "operator": "UnknownOperator",
                "threshold": 80
              }
            ]
            """;

        await using var stream =
            new MemoryStream(Encoding.UTF8.GetBytes(json));

        var loader = new JsonRuleLoader(
            new RuleValidator());

        var result = await loader.LoadAsync(stream);

        Assert.Equal(1, result.TotalRules);
        Assert.Empty(result.Rules);
        Assert.Equal(1, result.InvalidRules);
    }
}