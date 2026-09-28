using SensorRuleEngine.Application.Persistence;

namespace SensorRuleEngine.Application.Aggregation;

public sealed class AggregationService : IAggregationService
{
    private readonly IAggregationRepository _repository;
    private readonly AggregationQueryValidator _validator;

    public AggregationService(
        IAggregationRepository repository,
        AggregationQueryValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<IReadOnlyList<AggregationBucket>> AggregateAsync(
        AggregationQuery query,
        CancellationToken cancellationToken = default)
    {
        var errors = _validator.Validate(query);

        if (errors.Count > 0)
        {
            throw new ArgumentException(
                string.Join(" ", errors));
        }

        var readings =
            await _repository.GetAcceptableReadingsAsync(
                query.DeviceId,
                query.Metric,
                query.From,
                query.To,
                cancellationToken);

        var bucketDuration =
            TimeSpan.FromSeconds(query.BucketSeconds);

        var totalDuration =
            query.To - query.From;

        var bucketCount =
            (long)Math.Ceiling(
                totalDuration.TotalSeconds /
                query.BucketSeconds);

        var accumulators =
            new BucketAccumulator[checked((int)bucketCount)];

        for (var index = 0; index < accumulators.Length; index++)
        {
            var bucketStart =
                query.From.Add(
                    TimeSpan.FromTicks(
                        bucketDuration.Ticks * index));

            var bucketEnd =
                bucketStart + bucketDuration;

            if (bucketEnd > query.To)
            {
                bucketEnd = query.To;
            }

            accumulators[index] =
                new BucketAccumulator(
                    bucketStart,
                    bucketEnd);
        }

        foreach (var reading in readings)
        {
            var elapsed =
                reading.Timestamp - query.From;

            var bucketIndex =
                elapsed.Ticks / bucketDuration.Ticks;

            if (bucketIndex < 0 ||
                bucketIndex >= accumulators.Length)
            {
                continue;
            }

            accumulators[(int)bucketIndex]
                .Add(reading.Value);
        }

        return accumulators
            .Select(accumulator =>
                accumulator.ToBucket())
            .ToList();
    }

    private sealed class BucketAccumulator
    {
        private decimal _sum;

        public BucketAccumulator(
            DateTimeOffset bucketStart,
            DateTimeOffset bucketEnd)
        {
            BucketStart = bucketStart;
            BucketEnd = bucketEnd;
        }

        public DateTimeOffset BucketStart { get; }

        public DateTimeOffset BucketEnd { get; }

        public int Count { get; private set; }

        public decimal Min { get; private set; }

        public decimal Max { get; private set; }

        public void Add(decimal value)
        {
            if (Count == 0)
            {
                Min = value;
                Max = value;
            }
            else
            {
                Min = Math.Min(Min, value);
                Max = Math.Max(Max, value);
            }

            _sum += value;
            Count++;
        }

        public AggregationBucket ToBucket()
        {
            return new AggregationBucket
            {
                BucketStart = BucketStart,
                BucketEnd = BucketEnd,
                Count = Count,
                Average = Count == 0
                    ? null
                    : _sum / Count,
                Min = Count == 0
                    ? null
                    : Min,
                Max = Count == 0
                    ? null
                    : Max
            };
        }
    }
}