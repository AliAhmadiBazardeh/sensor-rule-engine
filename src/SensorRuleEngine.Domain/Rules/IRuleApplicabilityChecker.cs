using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Rules;

public interface IRuleApplicabilityChecker
{
    bool IsApplicable(
        SensorReading reading,
        Rule rule);
}