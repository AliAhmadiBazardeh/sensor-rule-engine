namespace SensorRuleEngine.Domain.Entities;

public sealed class Alert
{
    public required string RuleId { get; init; }

    public required string DeviceId { get; init; }

    public required string Metric { get; init; }

    public required DateTimeOffset StartTimestamp { get; init; }

    public required DateTimeOffset EndTimestamp { get; init; }

    public decimal? PeakValue { get; init; }
}
