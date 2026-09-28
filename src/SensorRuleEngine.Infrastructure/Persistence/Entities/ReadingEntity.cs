namespace SensorRuleEngine.Infrastructure.Persistence.Entities;

public sealed class ReadingEntity
{
    public long Id { get; set; }

    public required string DeviceId { get; set; }

    public required string Metric { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public decimal Value { get; set; }

    public int Sequence { get; set; }
    public bool IsAcceptable { get; set; }
}