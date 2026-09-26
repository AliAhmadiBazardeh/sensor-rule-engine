namespace SensorRuleEngine.Domain.Readings;

public sealed class ReadingValidationResult
{
    private ReadingValidationResult(
        bool isValid,
        IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ReadingValidationResult Valid()
    {
        return new ReadingValidationResult(
            true,
            Array.Empty<string>());
    }

    public static ReadingValidationResult Invalid(
        IEnumerable<string> errors)
    {
        return new ReadingValidationResult(
            false,
            errors.ToList());
    }
}