namespace SensorRuleEngine.Application.Aggregation;

public sealed class AggregationQueryValidator
{
    public IReadOnlyList<string> Validate(
        AggregationQuery query)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(query.DeviceId))
        {
            errors.Add("deviceId is required.");
        }

        if (string.IsNullOrWhiteSpace(query.Metric))
        {
            errors.Add("metric is required.");
        }

        if (query.From >= query.To)
        {
            errors.Add("'from' must be earlier than 'to'.");
        }
        
        if (query.From.Offset != TimeSpan.Zero)
        {
            errors.Add("'from' must be UTC.");
        }

        if (query.To.Offset != TimeSpan.Zero)
        {
            errors.Add("'to' must be UTC.");
        }

        if (query.BucketSeconds <= 0)
        {
            errors.Add("bucketSeconds must be greater than zero.");
        }
        
        return errors;
    }
}