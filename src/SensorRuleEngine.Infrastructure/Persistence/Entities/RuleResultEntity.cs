namespace SensorRuleEngine.Infrastructure.Persistence.Entities;

public sealed class RuleResultEntity
{
    public long Id { get; set; }

    public required string RuleId { get; set; }

    public required string DeviceId { get; set; }

    public required string Metric { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public int Sequence { get; set; }

    public required string Status { get; set; }

    public string? Reason { get; set; }
}