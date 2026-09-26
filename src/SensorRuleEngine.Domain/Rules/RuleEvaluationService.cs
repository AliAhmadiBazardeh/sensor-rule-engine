using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;

namespace SensorRuleEngine.Domain.Rules;

public sealed class RuleEvaluationService : IRuleEvaluationService
{
    private readonly IRuleApplicabilityChecker _applicabilityChecker;
    private readonly IRuleOperatorResolver _operatorResolver;

    public RuleEvaluationService(
        IRuleApplicabilityChecker applicabilityChecker,
        IRuleOperatorResolver operatorResolver)
    {
        _applicabilityChecker = applicabilityChecker;
        _operatorResolver = operatorResolver;
    }

    public IReadOnlyList<RuleResult> Evaluate(
        SensorReading reading,
        IEnumerable<Rule> rules)
    {
        var results = new List<RuleResult>();

        foreach (var rule in rules)
        {
            if (!_applicabilityChecker.IsApplicable(
                    reading,
                    rule))
            {
                continue;
            }

            if (rule.Operator == RuleOperatorType.SustainedAbove)
            {
                continue;
            }

            var ruleOperator = _operatorResolver.Resolve(
                rule.Operator);

            var result = ruleOperator.Evaluate(
                reading,
                rule);

            results.Add(result);
        }

        return results;
    }
}