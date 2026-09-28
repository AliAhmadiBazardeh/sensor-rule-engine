namespace SensorRuleEngine.Application.Aggregation;

public sealed class AggregationQuery
{
    public required string DeviceId { get; init; }

    public required string Metric { get; init; }

    public required DateTimeOffset From { get; init; }

    public required DateTimeOffset To { get; init; }

    public required int BucketSeconds { get; init; }
}