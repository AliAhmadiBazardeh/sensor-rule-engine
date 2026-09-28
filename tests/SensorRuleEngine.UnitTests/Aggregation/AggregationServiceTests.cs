using SensorRuleEngine.Application.Aggregation;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Entities;
using Xunit;

namespace SensorRuleEngine.UnitTests.Aggregation;

public sealed class AggregationServiceTests
{
    [Fact]
    public async Task AggregateAsync_ShouldCalculateBucketStatistics()
    {
        var from =
            DateTimeOffset.Parse("2026-01-01T10:00:00Z");

        var to =
            DateTimeOffset.Parse("2026-01-01T10:03:00Z");

        var readings = new[]
        {
            CreateReading(
                "2026-01-01T10:00:10Z",
                10,
                1),

            CreateReading(
                "2026-01-01T10:00:40Z",
                20,
                2),

            CreateReading(
                "2026-01-01T10:01:20Z",
                30,
                3)
        };

        var repository =
            new FakeAggregationRepository(readings);

        var service =
            CreateService(repository);

        var result =
            await service.AggregateAsync(
                new AggregationQuery
                {
                    DeviceId = "device-1",
                    Metric = "temperature",
                    From = from,
                    To = to,
                    BucketSeconds = 60
                });

        Assert.Equal(3, result.Count);

        Assert.Equal(2, result[0].Count);
        Assert.Equal(15, result[0].Average);
        Assert.Equal(10, result[0].Min);
        Assert.Equal(20, result[0].Max);

        Assert.Equal(1, result[1].Count);
        Assert.Equal(30, result[1].Average);
        Assert.Equal(30, result[1].Min);
        Assert.Equal(30, result[1].Max);

        Assert.Equal(0, result[2].Count);
        Assert.Null(result[2].Average);
        Assert.Null(result[2].Min);
        Assert.Null(result[2].Max);
    }

    [Fact]
    public async Task AggregateAsync_ShouldUseFromAsBucketAnchor()
    {
        var from =
            DateTimeOffset.Parse("2026-01-01T10:03:00Z");

        var to =
            DateTimeOffset.Parse("2026-01-01T10:06:00Z");

        var readings = new[]
        {
            CreateReading(
                "2026-01-01T10:03:10Z",
                10,
                1),

            CreateReading(
                "2026-01-01T10:04:10Z",
                20,
                2)
        };

        var repository =
            new FakeAggregationRepository(readings);

        var service =
            CreateService(repository);

        var result =
            await service.AggregateAsync(
                new AggregationQuery
                {
                    DeviceId = "device-1",
                    Metric = "temperature",
                    From = from,
                    To = to,
                    BucketSeconds = 60
                });

        Assert.Equal(
            from,
            result[0].BucketStart);

        Assert.Equal(
            from.AddMinutes(1),
            result[0].BucketEnd);

        Assert.Equal(
            from.AddMinutes(1),
            result[1].BucketStart);

        Assert.Equal(
            from.AddMinutes(2),
            result[1].BucketEnd);
    }

    [Fact]
    public async Task AggregateAsync_ShouldTreatToAsExclusive()
    {
        var from =
            DateTimeOffset.Parse("2026-01-01T10:00:00Z");

        var to =
            DateTimeOffset.Parse("2026-01-01T10:02:00Z");

        var readings = new[]
        {
            CreateReading(
                "2026-01-01T10:00:30Z",
                10,
                1),

            CreateReading(
                "2026-01-01T10:01:30Z",
                20,
                2),

            CreateReading(
                "2026-01-01T10:02:00Z",
                999,
                3)
        };

        var repository =
            new FakeAggregationRepository(readings);

        var service =
            CreateService(repository);

        var result =
            await service.AggregateAsync(
                new AggregationQuery
                {
                    DeviceId = "device-1",
                    Metric = "temperature",
                    From = from,
                    To = to,
                    BucketSeconds = 60
                });

        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Count);
        Assert.Equal(1, result[1].Count);

        Assert.DoesNotContain(
            result,
            bucket => bucket.Max == 999);
    }

    [Fact]
    public async Task AggregateAsync_ShouldCreateEmptyBuckets()
    {
        var from =
            DateTimeOffset.Parse("2026-01-01T10:00:00Z");

        var to =
            DateTimeOffset.Parse("2026-01-01T10:03:00Z");

        var repository =
            new FakeAggregationRepository(
                Array.Empty<SensorReading>());

        var service =
            CreateService(repository);

        var result =
            await service.AggregateAsync(
                new AggregationQuery
                {
                    DeviceId = "device-1",
                    Metric = "temperature",
                    From = from,
                    To = to,
                    BucketSeconds = 60
                });

        Assert.Equal(3, result.Count);

        foreach (var bucket in result)
        {
            Assert.Equal(0, bucket.Count);
            Assert.Null(bucket.Average);
            Assert.Null(bucket.Min);
            Assert.Null(bucket.Max);
        }
    }

    [Fact]
    public async Task AggregateAsync_ShouldRejectInvalidQuery()
    {
        var repository =
            new FakeAggregationRepository(
                Array.Empty<SensorReading>());

        var service =
            CreateService(repository);

        var query = new AggregationQuery
        {
            DeviceId = "",
            Metric = "",
            From = DateTimeOffset.Parse(
                "2026-01-01T11:00:00Z"),
            To = DateTimeOffset.Parse(
                "2026-01-01T10:00:00Z"),
            BucketSeconds = 0
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AggregateAsync(query));
    }

    private static AggregationService CreateService(
        IAggregationRepository repository)
    {
        return new AggregationService(
            repository,
            new AggregationQueryValidator());
    }

    private static SensorReading CreateReading(
        string timestamp,
        decimal value,
        int sequence)
    {
        return new SensorReading
        {
            DeviceId = "device-1",
            Metric = "temperature",
            Timestamp = DateTimeOffset.Parse(timestamp),
            Value = value,
            Sequence = sequence
        };
    }

    private sealed class FakeAggregationRepository
        : IAggregationRepository
    {
        private readonly IReadOnlyList<SensorReading> _readings;

        public FakeAggregationRepository(
            IReadOnlyList<SensorReading> readings)
        {
            _readings = readings;
        }

        public Task<IReadOnlyList<SensorReading>>
            GetAcceptableReadingsAsync(
                string deviceId,
                string metric,
                DateTimeOffset from,
                DateTimeOffset to,
                CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_readings);
        }
    }
}