using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Readings;
using Xunit;

namespace SensorRuleEngine.UnitTests.Readings;

public sealed class ReadingDeduplicatorTests
{
    [Fact]
    public void Add_ShouldAcceptReading_WhenReadingIsFirstOccurrence()
    {
        var deduplicator = new ReadingDeduplicator();
        var reading = CreateReading();

        var result = deduplicator.Add(reading);

        Assert.True(result.IsAccepted);
        Assert.False(result.IsDuplicate);
    }

    [Fact]
    public void Add_ShouldRejectReading_WhenReadingIsDuplicate()
    {
        var deduplicator = new ReadingDeduplicator();

        var first = CreateReading();
        var duplicate = CreateReading();

        var firstResult = deduplicator.Add(first);
        var duplicateResult = deduplicator.Add(duplicate);

        Assert.True(firstResult.IsAccepted);
        Assert.True(duplicateResult.IsDuplicate);
        Assert.False(duplicateResult.IsAccepted);
    }

    [Fact]
    public void Add_ShouldAcceptReading_WhenSequenceIsDifferent()
    {
        var deduplicator = new ReadingDeduplicator();

        var first = CreateReading(sequence: 1);
        var second = CreateReading(sequence: 2);

        var firstResult = deduplicator.Add(first);
        var secondResult = deduplicator.Add(second);

        Assert.True(firstResult.IsAccepted);
        Assert.True(secondResult.IsAccepted);
    }

    [Fact]
    public void Add_ShouldAcceptReading_WhenTimestampIsDifferent()
    {
        var deduplicator = new ReadingDeduplicator();

        var first = CreateReading(
            timestamp: "2025-06-01T08:00:00Z");

        var second = CreateReading(
            timestamp: "2025-06-01T08:01:00Z");

        var firstResult = deduplicator.Add(first);
        var secondResult = deduplicator.Add(second);

        Assert.True(firstResult.IsAccepted);
        Assert.True(secondResult.IsAccepted);
    }

    [Fact]
    public void Add_ShouldAcceptReading_WhenDeviceIsDifferent()
    {
        var deduplicator = new ReadingDeduplicator();

        var first = CreateReading(deviceId: "PUMP-01");
        var second = CreateReading(deviceId: "PUMP-02");

        var firstResult = deduplicator.Add(first);
        var secondResult = deduplicator.Add(second);

        Assert.True(firstResult.IsAccepted);
        Assert.True(secondResult.IsAccepted);
    }

    [Fact]
    public void Add_ShouldAcceptReading_WhenMetricIsDifferent()
    {
        var deduplicator = new ReadingDeduplicator();

        var first = CreateReading(metric: "temperature");
        var second = CreateReading(metric: "pressure");

        var firstResult = deduplicator.Add(first);
        var secondResult = deduplicator.Add(second);

        Assert.True(firstResult.IsAccepted);
        Assert.True(secondResult.IsAccepted);
    }

    [Fact]
    public void Add_ShouldKeepFirstOccurrence()
    {
        var deduplicator = new ReadingDeduplicator();

        var first = CreateReading(value: 80);
        var duplicate = CreateReading(value: 90);

        deduplicator.Add(first);
        deduplicator.Add(duplicate);

        var acceptedReadings = deduplicator.GetAcceptedReadings();

        var accepted = Assert.Single(acceptedReadings);

        Assert.Equal(80, accepted.Value);
    }

    private static SensorReading CreateReading(
        string deviceId = "PUMP-01",
        string metric = "temperature",
        string timestamp = "2025-06-01T08:00:00Z",
        decimal value = 82.5m,
        int sequence = 10)
    {
        return new SensorReading
        {
            DeviceId = deviceId,
            Metric = metric,
            Timestamp = DateTimeOffset.Parse(timestamp),
            Value = value,
            Sequence = sequence
        };
    }
}