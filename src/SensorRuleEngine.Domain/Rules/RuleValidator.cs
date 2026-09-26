using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;

namespace SensorRuleEngine.Domain.Rules;

public sealed class RuleValidator
{
    public RuleValidationResult Validate(Rule rule)
    {
        var errors = new List<string>();

        ValidateCommonProperties(rule, errors);
        ValidateParameters(rule, errors);

        return errors.Count == 0
            ? RuleValidationResult.Valid()
            : RuleValidationResult.Invalid(errors);
    }

    private static void ValidateCommonProperties(
        Rule rule,
        List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(rule.Id))
        {
            errors.Add("Rule id is required.");
        }

        if (string.IsNullOrWhiteSpace(rule.Name))
        {
            errors.Add("Rule name is required.");
        }

        if (string.IsNullOrWhiteSpace(rule.Metric))
        {
            errors.Add("Rule metric is required.");
        }
    }

    private static void ValidateParameters(
        Rule rule,
        List<string> errors)
    {
        switch (rule.Operator)
        {
            case RuleOperatorType.GreaterThan:
            case RuleOperatorType.GreaterThanOrEqual:
            case RuleOperatorType.LessThan:
            case RuleOperatorType.LessThanOrEqual:
            case RuleOperatorType.Equal:
                RequireParameter(
                    rule,
                    "threshold",
                    errors);
                break;

            case RuleOperatorType.Between:
                ValidateBetweenParameters(
                    rule,
                    errors);
                break;

            case RuleOperatorType.SustainedAbove:
                ValidateSustainedAboveParameters(
                    rule,
                    errors);
                break;

            default:
                errors.Add(
                    $"Unsupported operator '{rule.Operator}'.");
                break;
        }
    }

    private static void ValidateBetweenParameters(
        Rule rule,
        List<string> errors)
    {
        var hasMin = RequireParameter(
            rule,
            "min",
            errors);

        var hasMax = RequireParameter(
            rule,
            "max",
            errors);

        if (hasMin &&
            hasMax &&
            rule.Parameters["min"] > rule.Parameters["max"])
        {
            errors.Add(
                "Parameter 'min' must be less than or equal to 'max'.");
        }
    }

    private static void ValidateSustainedAboveParameters(
        Rule rule,
        List<string> errors)
    {
        RequireParameter(
            rule,
            "threshold",
            errors);

        var hasDuration = RequireParameter(
            rule,
            "durationSeconds",
            errors);

        if (hasDuration &&
            rule.Parameters["durationSeconds"] <= 0)
        {
            errors.Add(
                "Parameter 'durationSeconds' must be greater than zero.");
        }
    }

    private static bool RequireParameter(
        Rule rule,
        string parameterName,
        List<string> errors)
    {
        if (rule.Parameters.ContainsKey(parameterName))
        {
            return true;
        }

        errors.Add(
            $"Parameter '{parameterName}' is required.");

        return false;
    }
}