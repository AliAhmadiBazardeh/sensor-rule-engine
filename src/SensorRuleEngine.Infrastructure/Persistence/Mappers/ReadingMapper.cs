using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Infrastructure.Persistence.Entities;

namespace SensorRuleEngine.Infrastructure.Persistence.Mappers;

internal static class ReadingMapper
{
    public static ReadingEntity ToEntity(
        SensorReading reading)
    {
        return new ReadingEntity
        {
            DeviceId = reading.DeviceId,
            Metric = reading.Metric,
            Timestamp = reading.Timestamp,
            Value = reading.Value,
            Sequence = reading.Sequence
        };
    }
}