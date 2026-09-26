using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Readings;
using Xunit;

namespace SensorRuleEngine.UnitTests.Readings;

public sealed class SensorReadingValidatorTests
{
    private readonly SensorReadingValidator _validator = new();

    [Fact]
    public void Validate_ShouldAccept_ValidReading()
    {
        var reading = CreateReading();

        var result = _validator.Validate(reading);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ShouldReject_WhenDeviceIdIsEmpty()
    {
        var reading = CreateReading(deviceId: "");

        var result = _validator.Validate(reading);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Contains("deviceId",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldReject_WhenMetricIsEmpty()
    {
        var reading = CreateReading(metric: "");

        var result = _validator.Validate(reading);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Contains("metric",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldReject_WhenSequenceIsNegative()
    {
        var reading = CreateReading(sequence: -1);

        var result = _validator.Validate(reading);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Contains("seq",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldReject_WhenTimestampIsNotUtc()
    {
        var reading = CreateReading(
            timestamp: new DateTimeOffset(
                2025,
                6,
                1,
                8,
                0,
                0,
                TimeSpan.FromHours(3.5)));

        var result = _validator.Validate(reading);

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.Contains("UTC",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldAccept_ZeroSequence()
    {
        var reading = CreateReading(sequence: 0);

        var result = _validator.Validate(reading);

        Assert.True(result.IsValid);
    }

    private static SensorReading CreateReading(
        string deviceId = "device-1",
        string metric = "temperature",
        DateTimeOffset? timestamp = null,
        decimal value = 80,
        int sequence = 1)
    {
        return new SensorReading
        {
            DeviceId = deviceId,
            Metric = metric,
            Timestamp = timestamp
                ?? DateTimeOffset.Parse("2025-06-01T08:00:00Z"),
            Value = value,
            Sequence = sequence
        };
    }
}