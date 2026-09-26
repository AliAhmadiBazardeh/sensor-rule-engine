using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Readings;

public static class EventTimeReadingOrdering
{
    public static IReadOnlyList<SensorReading> Order(
        IEnumerable<SensorReading> readings)
    {
        return readings
            .OrderBy(reading => reading.Timestamp)
            .ThenBy(reading => reading.Sequence)
            .ToList();
    }
}