using SensorRuleEngine.Domain.Enums;

namespace SensorRuleEngine.Domain.Rules;

public interface IRuleOperatorResolver
{
    IRuleOperator Resolve(RuleOperatorType operatorType);
}