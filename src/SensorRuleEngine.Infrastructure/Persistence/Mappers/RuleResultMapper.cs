using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Infrastructure.Persistence.Entities;

namespace SensorRuleEngine.Infrastructure.Persistence.Mappers;

internal static class RuleResultMapper
{
    public static RuleResultEntity ToEntity(
        RuleResult result)
    {
        return new RuleResultEntity
        {
            RuleId = result.RuleId,
            DeviceId = result.ReadingKey.DeviceId,
            Metric = result.ReadingKey.Metric,
            Timestamp = result.ReadingKey.Timestamp,
            Sequence = result.ReadingKey.Sequence,
            Status = result.Status.ToString(),
            Reason = result.Reason
        };
    }
}