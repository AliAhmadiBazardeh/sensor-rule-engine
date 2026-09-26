using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Readings;

public sealed class SensorReadingValidator
{
    public ReadingValidationResult Validate(
        SensorReading reading)
    {
        var errors = new List<string>();

        ValidateDeviceId(reading, errors);
        ValidateMetric(reading, errors);
        ValidateTimestamp(reading, errors);
        ValidateSequence(reading, errors);

        return errors.Count == 0
            ? ReadingValidationResult.Valid()
            : ReadingValidationResult.Invalid(errors);
    }

    private static void ValidateDeviceId(
        SensorReading reading,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(reading.DeviceId))
        {
            errors.Add("deviceId is required.");
        }
    }

    private static void ValidateMetric(
        SensorReading reading,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(reading.Metric))
        {
            errors.Add("metric is required.");
        }
    }

    private static void ValidateTimestamp(
        SensorReading reading,
        List<string> errors)
    {
        if (reading.Timestamp.Offset != TimeSpan.Zero)
        {
            errors.Add("Timestamp must be UTC.");
        }
    }

    private static void ValidateSequence(
        SensorReading reading,
        List<string> errors)
    {
        if (reading.Sequence < 0)
        {
            errors.Add("seq must be greater than or equal to zero.");
        }
    }
}