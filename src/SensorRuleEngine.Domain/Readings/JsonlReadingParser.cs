using System.Text.Json;
using SensorRuleEngine.Domain.Entities;
using System.Text.Json.Serialization;

namespace SensorRuleEngine.Domain.Readings;

public sealed class JsonlReadingParser
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonlReadingParseResult Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return JsonlReadingParseResult.Failure(
                "Input line is empty.");
        }

        try
        {
            var dto = JsonSerializer.Deserialize<ReadingDto>(
                line,
                _jsonOptions);

            if (dto is null)
            {
                return JsonlReadingParseResult.Failure(
                    "Input JSON could not be deserialized.");
            }

            if (dto.DeviceId is null)
            {
                return JsonlReadingParseResult.Failure(
                    "Property 'deviceId' is required.");
            }

            if (dto.Metric is null)
            {
                return JsonlReadingParseResult.Failure(
                    "Property 'metric' is required.");
            }

            if (dto.Timestamp is null)
            {
                return JsonlReadingParseResult.Failure(
                    "Property 'ts' is required.");
            }

            if (dto.Value is null)
            {
                return JsonlReadingParseResult.Failure(
                    "Property 'value' is required.");
            }

            if (dto.Sequence is null)
            {
                return JsonlReadingParseResult.Failure(
                    "Property 'seq' is required.");
            }

            if (!DateTimeOffset.TryParse(
                    dto.Timestamp,
                    out var timestamp))
            {
                return JsonlReadingParseResult.Failure(
                    "Property 'ts' must be a valid timestamp.");
            }

            var reading = new SensorReading
            {
                DeviceId = dto.DeviceId,
                Metric = dto.Metric,
                Timestamp = timestamp,
                Value = dto.Value.Value,
                Sequence = dto.Sequence.Value
            };

            return JsonlReadingParseResult.Success(reading);
        }
        catch (JsonException)
        {
            return JsonlReadingParseResult.Failure(
                "Input line is not valid JSON.");
        }
        catch (OverflowException)
        {
            return JsonlReadingParseResult.Failure(
                "Numeric value is outside the supported range.");
        }
    }
    
    private sealed class ReadingDto
    {
        [JsonPropertyName("deviceId")]
        public string? DeviceId { get; init; }

        [JsonPropertyName("metric")]
        public string? Metric { get; init; }

        [JsonPropertyName("ts")]
        public string? Timestamp { get; init; }

        [JsonPropertyName("value")]
        public decimal? Value { get; init; }

        [JsonPropertyName("seq")]
        public int? Sequence { get; init; }
    }
}