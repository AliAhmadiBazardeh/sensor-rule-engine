using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Infrastructure.Persistence.Entities;

namespace SensorRuleEngine.Infrastructure.Persistence.Mappers;

internal static class AlertMapper
{
    public static AlertEntity ToEntity(
        Alert alert)
    {
        return new AlertEntity
        {
            RuleId = alert.RuleId,
            DeviceId = alert.DeviceId,
            Metric = alert.Metric,
            StartTimestamp = alert.StartTimestamp,
            EndTimestamp = alert.EndTimestamp,
            PeakValue = alert.PeakValue
        };
    }
}