using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SensorRuleEngine.Infrastructure.Persistence;
using SensorRuleEngine.Infrastructure.Persistence.Entities;
using SensorRuleEngine.IntegrationTests.Api;
using Xunit;

namespace SensorRuleEngine.IntegrationTests;

public sealed class AggregationApiTests
{
    [Fact]
    public async Task GetAggregation_ShouldReturnAggregatedAcceptableReadings()
    {
        await using var factory =
            new ApiWebApplicationFactory();

        using var client =
            factory.CreateClient();

        await SeedReadingsAsync(factory);

        var response =
            await client.GetAsync(
                "/api/v1/aggregation" +
                "?deviceId=device-1" +
                "&metric=temperature" +
                "&from=2026-01-01T10:00:00Z" +
                "&to=2026-01-01T10:10:00Z" +
                "&bucketSeconds=600");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var buckets =
            await response.Content
                .ReadFromJsonAsync<
                    List<AggregationBucketResponse>>();

        Assert.NotNull(buckets);

        var bucket =
            Assert.Single(buckets);

        Assert.Equal(
            DateTimeOffset.Parse(
                "2026-01-01T10:00:00Z"),
            bucket.BucketStart);

        Assert.Equal(
            DateTimeOffset.Parse(
                "2026-01-01T10:10:00Z"),
            bucket.BucketEnd);

        Assert.Equal(3, bucket.Count);

        Assert.Equal(80m, bucket.Average);

        Assert.Equal(75m, bucket.Min);

        Assert.Equal(85m, bucket.Max);
    }

    private static async Task SeedReadingsAsync(
        ApiWebApplicationFactory factory)
    {
        using var scope =
            factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SensorRuleEngineDbContext>();

        await dbContext.Database.MigrateAsync();

        dbContext.Readings.AddRange(
            new ReadingEntity
            {
                DeviceId = "device-1",
                Metric = "temperature",
                Timestamp = DateTimeOffset.Parse(
                    "2026-01-01T10:05:00Z"),
                Value = 75,
                Sequence = 1,
                IsAcceptable = true
            },
            new ReadingEntity
            {
                DeviceId = "device-1",
                Metric = "temperature",
                Timestamp = DateTimeOffset.Parse(
                    "2026-01-01T10:06:00Z"),
                Value = 85,
                Sequence = 2,
                IsAcceptable = true
            },
            new ReadingEntity
            {
                DeviceId = "device-1",
                Metric = "temperature",
                Timestamp = DateTimeOffset.Parse(
                    "2026-01-01T10:07:00Z"),
                Value = 80,
                Sequence = 3,
                IsAcceptable = true
            },
            new ReadingEntity
            {
                DeviceId = "device-1",
                Metric = "temperature",
                Timestamp = DateTimeOffset.Parse(
                    "2026-01-01T10:08:00Z"),
                Value = 100,
                Sequence = 4,
                IsAcceptable = false
            });

        await dbContext.SaveChangesAsync();
    }

    private sealed class AggregationBucketResponse
    {
        public DateTimeOffset BucketStart { get; init; }

        public DateTimeOffset BucketEnd { get; init; }

        public int Count { get; init; }

        public decimal? Average { get; init; }

        public decimal? Min { get; init; }

        public decimal? Max { get; init; }
    }

    [Fact]
    public async Task GetAggregation_ShouldExcludeReadingAtToBoundary()
    {
        await using var factory =
            new ApiWebApplicationFactory();

        using var client =
            factory.CreateClient();

        await SeedBoundaryReadingsAsync(factory);

        var response =
            await client.GetAsync(
                "/api/v1/aggregation" +
                "?deviceId=device-1" +
                "&metric=temperature" +
                "&from=2026-01-01T10:00:00Z" +
                "&to=2026-01-01T10:10:00Z" +
                "&bucketSeconds=600");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var buckets =
            await response.Content
                .ReadFromJsonAsync<
                    List<AggregationBucketResponse>>();

        Assert.NotNull(buckets);

        var bucket =
            Assert.Single(buckets);

        Assert.Equal(1, bucket.Count);
        Assert.Equal(75m, bucket.Average);
        Assert.Equal(75m, bucket.Min);
        Assert.Equal(75m, bucket.Max);
    }

    private static async Task SeedBoundaryReadingsAsync(
        ApiWebApplicationFactory factory)
    {
        using var scope =
            factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SensorRuleEngineDbContext>();

        await dbContext.Database.MigrateAsync();

        dbContext.Readings.AddRange(
            new ReadingEntity
            {
                DeviceId = "device-1",
                Metric = "temperature",
                Timestamp = DateTimeOffset.Parse(
                    "2026-01-01T10:09:59Z"),
                Value = 75,
                Sequence = 1,
                IsAcceptable = true
            },
            new ReadingEntity
            {
                DeviceId = "device-1",
                Metric = "temperature",
                Timestamp = DateTimeOffset.Parse(
                    "2026-01-01T10:10:00Z"),
                Value = 100,
                Sequence = 2,
                IsAcceptable = true
            });

        await dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAggregation_ShouldPreserveEmptyBuckets()
    {
        await using var factory =
            new ApiWebApplicationFactory();

        using var client =
            factory.CreateClient();

        await SeedSparseReadingsAsync(factory);

        var response =
            await client.GetAsync(
                "/api/v1/aggregation" +
                "?deviceId=device-1" +
                "&metric=temperature" +
                "&from=2026-01-01T10:00:00Z" +
                "&to=2026-01-01T10:30:00Z" +
                "&bucketSeconds=600");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var buckets =
            await response.Content
                .ReadFromJsonAsync<
                    List<AggregationBucketResponse>>();

        Assert.NotNull(buckets);

        Assert.Equal(3, buckets.Count);

        Assert.Equal(1, buckets[0].Count);
        Assert.Equal(75m, buckets[0].Average);

        Assert.Equal(0, buckets[1].Count);
        Assert.Null(buckets[1].Average);
        Assert.Null(buckets[1].Min);
        Assert.Null(buckets[1].Max);

        Assert.Equal(1, buckets[2].Count);
        Assert.Equal(85m, buckets[2].Average);
    }

    private static async Task SeedSparseReadingsAsync(
        ApiWebApplicationFactory factory)
    {
        using var scope =
            factory.Services.CreateScope();

        var dbContext =
            scope.ServiceProvider
                .GetRequiredService<
                    SensorRuleEngineDbContext>();

        await dbContext.Database.MigrateAsync();

        dbContext.Readings.AddRange(
            new ReadingEntity
            {
                DeviceId = "device-1",
                Metric = "temperature",
                Timestamp = DateTimeOffset.Parse(
                    "2026-01-01T10:05:00Z"),
                Value = 75,
                Sequence = 1,
                IsAcceptable = true
            },
            new ReadingEntity
            {
                DeviceId = "device-1",
                Metric = "temperature",
                Timestamp = DateTimeOffset.Parse(
                    "2026-01-01T10:25:00Z"),
                Value = 85,
                Sequence = 2,
                IsAcceptable = true
            });

        await dbContext.SaveChangesAsync();
    }
    
    [Fact]
    public async Task GetAggregation_ShouldReturnBadRequest_WhenBucketSecondsIsZero()
    {
        await using var factory =
            new ApiWebApplicationFactory();

        using var client =
            factory.CreateClient();

        var response =
            await client.GetAsync(
                "/api/v1/aggregation" +
                "?deviceId=device-1" +
                "&metric=temperature" +
                "&from=2026-01-01T10:00:00Z" +
                "&to=2026-01-01T10:10:00Z" +
                "&bucketSeconds=0");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    [Fact]
    public async Task GetAggregation_ShouldReturnBadRequest_WhenFromIsAfterTo()
    {
        await using var factory =
            new ApiWebApplicationFactory();

        using var client =
            factory.CreateClient();

        var response =
            await client.GetAsync(
                "/api/v1/aggregation" +
                "?deviceId=device-1" +
                "&metric=temperature" +
                "&from=2026-01-01T10:10:00Z" +
                "&to=2026-01-01T10:00:00Z" +
                "&bucketSeconds=600");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
}