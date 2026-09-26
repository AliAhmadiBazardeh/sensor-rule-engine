using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Readings;
using Xunit;

namespace SensorRuleEngine.UnitTests.Readings;

public sealed class EventTimeReadingOrderingTests
{
    [Fact]
    public void Order_ShouldSortReadingsByTimestampAscending()
    {
        var readings = new[]
        {
            CreateReading("2025-06-01T08:05:00Z", 1),
            CreateReading("2025-06-01T08:01:00Z", 2),
            CreateReading("2025-06-01T08:03:00Z", 3)
        };

        var ordered = EventTimeReadingOrdering.Order(readings);

        Assert.Equal(
            DateTimeOffset.Parse("2025-06-01T08:01:00Z"),
            ordered[0].Timestamp);

        Assert.Equal(
            DateTimeOffset.Parse("2025-06-01T08:03:00Z"),
            ordered[1].Timestamp);

        Assert.Equal(
            DateTimeOffset.Parse("2025-06-01T08:05:00Z"),
            ordered[2].Timestamp);
    }

    [Fact]
    public void Order_ShouldUseSequenceAsTieBreaker()
    {
        var readings = new[]
        {
            CreateReading("2025-06-01T08:00:00Z", 30),
            CreateReading("2025-06-01T08:00:00Z", 10),
            CreateReading("2025-06-01T08:00:00Z", 20)
        };

        var ordered = EventTimeReadingOrdering.Order(readings);

        Assert.Equal(10, ordered[0].Sequence);
        Assert.Equal(20, ordered[1].Sequence);
        Assert.Equal(30, ordered[2].Sequence);
    }

    [Fact]
    public void Order_ShouldPreserveAllReadings()
    {
        var readings = new[]
        {
            CreateReading("2025-06-01T08:05:00Z", 1),
            CreateReading("2025-06-01T08:01:00Z", 2),
            CreateReading("2025-06-01T08:03:00Z", 3)
        };

        var ordered = EventTimeReadingOrdering.Order(readings);

        Assert.Equal(
            readings.Length,
            ordered.Count);
    }

    [Fact]
    public void Order_ShouldNotModifyOriginalCollection()
    {
        var first = CreateReading(
            "2025-06-01T08:05:00Z",
            1);

        var second = CreateReading(
            "2025-06-01T08:01:00Z",
            2);

        var readings = new[]
        {
            first,
            second
        };

        EventTimeReadingOrdering.Order(readings);

        Assert.Same(first, readings[0]);
        Assert.Same(second, readings[1]);
    }

    [Fact]
    public void Order_ShouldReturnEmptyCollection_WhenInputIsEmpty()
    {
        var readings = Array.Empty<SensorReading>();

        var ordered = EventTimeReadingOrdering.Order(readings);

        Assert.Empty(ordered);
    }

    private static SensorReading CreateReading(
        string timestamp,
        int sequence)
    {
        return new SensorReading
        {
            DeviceId = "PUMP-01",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(timestamp),
            Value = 80,
            Sequence = sequence
        };
    }
}