namespace SensorRuleEngine.Domain.Rules;

public sealed class RuleValidationResult
{
    private RuleValidationResult(
        bool isValid,
        IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }

    public IReadOnlyList<string> Errors { get; }

    public static RuleValidationResult Valid()
    {
        return new RuleValidationResult(
            true,
            Array.Empty<string>());
    }

    public static RuleValidationResult Invalid(
        IEnumerable<string> errors)
    {
        var errorList = errors.ToList();

        return new RuleValidationResult(
            false,
            errorList);
    }
}