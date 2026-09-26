using SensorRuleEngine.Domain.Readings;
using Xunit;

namespace SensorRuleEngine.UnitTests.Readings;

public sealed class JsonlReadingParserTests
{
    private readonly JsonlReadingParser _parser = new();

    [Fact]
    public void Parse_ShouldReturnReading_WhenJsonIsValid()
    {
        const string json = """
        {
            "deviceId": "PUMP-01",
            "metric": "temperature",
            "ts": "2025-06-01T08:00:00Z",
            "value": 82.5,
            "seq": 10
        }
        """;

        var result = _parser.Parse(json);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Reading);

        Assert.Equal("PUMP-01", result.Reading.DeviceId);
        Assert.Equal("temperature", result.Reading.Metric);
        Assert.Equal(82.5m, result.Reading.Value);
        Assert.Equal(10, result.Reading.Sequence);
    }

    [Fact]
    public void Parse_ShouldFail_WhenJsonIsMalformed()
    {
        const string json = """
        {"deviceId":"PUMP-01","metric":"temperature"
        """;

        var result = _parser.Parse(json);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Reading);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void Parse_ShouldFail_WhenTimestampIsInvalid()
    {
        const string json = """
        {
            "deviceId": "PUMP-01",
            "metric": "temperature",
            "ts": "not-a-date",
            "value": 82.5,
            "seq": 10
        }
        """;

        var result = _parser.Parse(json);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Reading);
        Assert.Contains(
            result.Errors,
            error => error.Contains(
                "timestamp",
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_ShouldFail_WhenValueIsNotNumeric()
    {
        const string json = """
        {
            "deviceId": "PUMP-01",
            "metric": "temperature",
            "ts": "2025-06-01T08:00:00Z",
            "value": "high",
            "seq": 10
        }
        """;

        var result = _parser.Parse(json);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Reading);
    }

    [Fact]
    public void Parse_ShouldFail_WhenSequenceIsNotInteger()
    {
        const string json = """
        {
            "deviceId": "PUMP-01",
            "metric": "temperature",
            "ts": "2025-06-01T08:00:00Z",
            "value": 82.5,
            "seq": 10.5
        }
        """;

        var result = _parser.Parse(json);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Reading);
    }

    [Fact]
    public void Parse_ShouldFail_WhenRequiredPropertyIsMissing()
    {
        const string json = """
        {
            "deviceId": "PUMP-01",
            "metric": "temperature",
            "value": 82.5,
            "seq": 10
        }
        """;

        var result = _parser.Parse(json);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Reading);
    }

    [Fact]
    public void Parse_ShouldAccept_ZeroValue()
    {
        const string json = """
        {
            "deviceId": "PUMP-01",
            "metric": "temperature",
            "ts": "2025-06-01T08:00:00Z",
            "value": 0,
            "seq": 0
        }
        """;

        var result = _parser.Parse(json);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Reading);
        Assert.Equal(0m, result.Reading.Value);
        Assert.Equal(0, result.Reading.Sequence);
    }
}