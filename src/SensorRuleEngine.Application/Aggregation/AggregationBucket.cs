namespace SensorRuleEngine.Application.Aggregation;

public sealed class AggregationBucket
{
    public required DateTimeOffset BucketStart { get; init; }

    public required DateTimeOffset BucketEnd { get; init; }

    public int Count { get; init; }

    public decimal? Average { get; init; }

    public decimal? Min { get; init; }

    public decimal? Max { get; init; }
}