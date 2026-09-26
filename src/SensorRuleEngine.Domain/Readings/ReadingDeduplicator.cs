using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Domain.Readings;

public sealed class ReadingDeduplicator
{
    private readonly HashSet<ReadingKey> _seenKeys = new();

    private readonly List<SensorReading> _acceptedReadings = new();

    public ReadingDeduplicationResult Add(
        SensorReading reading)
    {
        var key = new ReadingKey(
            reading.DeviceId,
            reading.Metric,
            reading.Timestamp,
            reading.Sequence);

        if (!_seenKeys.Add(key))
        {
            return ReadingDeduplicationResult.Duplicate();
        }

        _acceptedReadings.Add(reading);

        return ReadingDeduplicationResult.Accepted();
    }

    public IReadOnlyList<SensorReading> GetAcceptedReadings()
    {
        return _acceptedReadings;
    }
}