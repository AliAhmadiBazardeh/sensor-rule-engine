using SensorRuleEngine.Domain.ValueObjects;
using Xunit;

namespace SensorRuleEngine.UnitTests.ValueObjects;

public class ReadingKeyTests
{
    [Fact]
    public void Two_keys_with_same_values_should_be_equal()
    {
        var timestamp = DateTimeOffset.Parse("2025-06-01T08:33:00Z");

        var first = new ReadingKey(
            "PUMP-01",
            "temperature",
            timestamp,
            1199);

        var second = new ReadingKey(
            "PUMP-01",
            "temperature",
            timestamp,
            1199);

        Assert.Equal(first, second);
    }

    [Fact]
    public void Keys_with_different_sequence_should_not_be_equal()
    {
        var timestamp = DateTimeOffset.Parse("2025-06-01T08:33:00Z");

        var first = new ReadingKey(
            "PUMP-01",
            "temperature",
            timestamp,
            1199);

        var second = new ReadingKey(
            "PUMP-01",
            "temperature",
            timestamp,
            1200);

        Assert.NotEqual(first, second);
    }
}
