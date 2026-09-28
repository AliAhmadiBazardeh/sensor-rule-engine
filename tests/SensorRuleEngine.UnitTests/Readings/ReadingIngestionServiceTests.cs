using System.Text;
using SensorRuleEngine.Application.Ingestion;
using SensorRuleEngine.Domain.Readings;
using Xunit;

namespace SensorRuleEngine.UnitTests.Readings;

public sealed class ReadingIngestionServiceTests
{
    [Fact]
    public async Task IngestAsync_ShouldAcceptValidReading()
    {
        const string jsonl =
            """{"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:00:00Z","value":25.5,"seq":1}""";

        await using var stream = CreateStream(jsonl);

        var service = CreateService();

        var result = await service.IngestAsync(stream);

        Assert.Equal(1, result.TotalLines);
        Assert.Equal(1, result.Parsed);
        Assert.Equal(0, result.Invalid);
        Assert.Equal(0, result.Duplicates);

        var reading = Assert.Single(result.Readings);

        Assert.Equal("device-1", reading.DeviceId);
        Assert.Equal("temperature", reading.Metric);
        Assert.Equal(25.5m, reading.Value);
        Assert.Equal(1, reading.Sequence);
    }

    [Fact]
    public async Task IngestAsync_ShouldRejectMalformedJson()
    {
        const string jsonl =
            """{"deviceId":"device-1","metric":"temperature","value":25.5,"seq":1""";

        await using var stream = CreateStream(jsonl);

        var service = CreateService();

        var result = await service.IngestAsync(stream);

        Assert.Equal(1, result.TotalLines);
        Assert.Equal(0, result.Parsed);
        Assert.Equal(1, result.Invalid);
        Assert.Empty(result.Readings);
    }

    [Fact]
    public async Task IngestAsync_ShouldRejectSemanticallyInvalidReading()
    {
        const string jsonl =
            """{"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:00:00+03:30","value":25.5,"seq":1}""";

        await using var stream = CreateStream(jsonl);

        var service = CreateService();

        var result = await service.IngestAsync(stream);

        Assert.Equal(1, result.TotalLines);
        Assert.Equal(1, result.Parsed);
        Assert.Equal(1, result.Invalid);
        Assert.Empty(result.Readings);
    }

    [Fact]
    public async Task IngestAsync_ShouldCountDuplicateReading()
    {
        const string jsonl =
            """
            {"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:00:00Z","value":25.5,"seq":1}
            {"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:00:00Z","value":25.5,"seq":1}
            """;

        await using var stream = CreateStream(jsonl);

        var service = CreateService();

        var result = await service.IngestAsync(stream);

        Assert.Equal(2, result.TotalLines);
        Assert.Equal(2, result.Parsed);
        Assert.Equal(0, result.Invalid);
        Assert.Equal(1, result.Duplicates);

        Assert.Single(result.Readings);
    }

    [Fact]
    public async Task IngestAsync_ShouldKeepDifferentSequenceAsDifferentReading()
    {
        const string jsonl =
            """
            {"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:00:00Z","value":25.5,"seq":1}
            {"deviceId":"device-1","metric":"temperature","ts":"2026-01-01T10:00:00Z","value":26.5,"seq":2}
            """;

        await using var stream = CreateStream(jsonl);

        var service = CreateService();

        var result = await service.IngestAsync(stream);

        Assert.Equal(2, result.TotalLines);
        Assert.Equal(2, result.Parsed);
        Assert.Equal(0, result.Invalid);
        Assert.Equal(0, result.Duplicates);

        Assert.Equal(2, result.Readings.Count);
    }

    private static ReadingIngestionService CreateService()
    {
        return new ReadingIngestionService(
            new JsonlReadingParser(),
            new SensorReadingValidator());
    }

    private static MemoryStream CreateStream(string content)
    {
        return new MemoryStream(
            Encoding.UTF8.GetBytes(content));
    }
}