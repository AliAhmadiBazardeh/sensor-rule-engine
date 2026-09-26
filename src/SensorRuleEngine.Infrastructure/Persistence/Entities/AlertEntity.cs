namespace SensorRuleEngine.Infrastructure.Persistence.Entities;

public sealed class AlertEntity
{
    public long Id { get; set; }

    public required string RuleId { get; set; }

    public required string DeviceId { get; set; }

    public required string Metric { get; set; }

    public DateTimeOffset StartTimestamp { get; set; }

    public DateTimeOffset EndTimestamp { get; set; }

    public decimal? PeakValue { get; set; }
}