namespace SensorRuleEngine.Domain.ValueObjects;

public readonly record struct ReadingKey(
        string DeviceId,
        string Metric,
        DateTimeOffset Timestamp,
        int Sequence);