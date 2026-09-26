using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Rules;

public sealed class RuleApplicabilityChecker : IRuleApplicabilityChecker
{
    public bool IsApplicable(
        SensorReading reading,
        Rule rule)
    {
        if (!rule.Enabled)
        {
            return false;
        }

        if (!string.Equals(
                rule.Metric,
                reading.Metric,
                StringComparison.Ordinal))
        {
            return false;
        }

        if (rule.DeviceId is not null &&
            !string.Equals(
                rule.DeviceId,
                reading.DeviceId,
                StringComparison.Ordinal))
        {
            return false;
        }

        return true;
    }
}