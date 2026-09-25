namespace SensorRuleEngine.Domain.Entities;

public sealed class SensorReading
{
    public required string DeviceId { get; init; }

    public required string Metric { get; init; }

    public required DateTimeOffset Timestamp { get; init; }

    public required decimal Value { get; init; }

    public required int Sequence { get; init; }
}