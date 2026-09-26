using SensorRuleEngine.Domain.Enums;

namespace SensorRuleEngine.Domain.Rules;

public sealed class RuleOperatorResolver : IRuleOperatorResolver
{
    private readonly IReadOnlyDictionary<RuleOperatorType, IRuleOperator> _operators;

    public RuleOperatorResolver(
        IEnumerable<IRuleOperator> operators)
    {
        _operators = operators.ToDictionary(
            GetOperatorType,
            operatorInstance => operatorInstance);
    }

    public IRuleOperator Resolve(RuleOperatorType operatorType)
    {
        if (_operators.TryGetValue(
                operatorType,
                out var ruleOperator))
        {
            return ruleOperator;
        }

        throw new InvalidOperationException(
            $"No operator registered for '{operatorType}'.");
    }

    private static RuleOperatorType GetOperatorType(
        IRuleOperator ruleOperator)
    {
        return ruleOperator switch
        {
            GreaterThanOperator =>
                RuleOperatorType.GreaterThan,

            GreaterThanOrEqualOperator =>
                RuleOperatorType.GreaterThanOrEqual,

            LessThanOperator =>
                RuleOperatorType.LessThan,

            LessThanOrEqualOperator =>
                RuleOperatorType.LessThanOrEqual,

            EqualOperator =>
                RuleOperatorType.Equal,

            BetweenOperator =>
                RuleOperatorType.Between,

            _ => throw new InvalidOperationException(
                $"Unsupported operator type: {ruleOperator.GetType().Name}.")
        };
    }
}