using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Alerting;

public sealed class AlertDeduplicator
{
    private readonly HashSet<AlertKey> _seenAlerts = new();

    public bool TryAdd(Alert alert)
    {
        var key = new AlertKey(
            alert.RuleId,
            alert.DeviceId,
            alert.Metric,
            alert.StartTimestamp,
            alert.EndTimestamp);

        return _seenAlerts.Add(key);
    }

    private readonly record struct AlertKey(
        string RuleId,
        string DeviceId,
        string Metric,
        DateTimeOffset StartTimestamp,
        DateTimeOffset EndTimestamp);
}