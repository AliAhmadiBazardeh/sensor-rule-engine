using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Readings;

public sealed class JsonlReadingParseResult
{
    private JsonlReadingParseResult(
        bool isSuccess,
        SensorReading? reading,
        IReadOnlyList<string> errors)
    {
        IsSuccess = isSuccess;
        Reading = reading;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public SensorReading? Reading { get; }

    public IReadOnlyList<string> Errors { get; }

    public static JsonlReadingParseResult Success(
        SensorReading reading)
    {
        return new JsonlReadingParseResult(
            true,
            reading,
            Array.Empty<string>());
    }

    public static JsonlReadingParseResult Failure(
        params string[] errors)
    {
        return new JsonlReadingParseResult(
            false,
            null,
            errors);
    }
}