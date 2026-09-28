namespace SensorRuleEngine.Application.Aggregation;

public interface IAggregationService
{
    Task<IReadOnlyList<AggregationBucket>> AggregateAsync(
        AggregationQuery query,
        CancellationToken cancellationToken = default);
}