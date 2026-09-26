using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Alerting;

public sealed class AlertCooldownPolicy
{
    private readonly TimeSpan _cooldown;

    public AlertCooldownPolicy(TimeSpan cooldown)
    {
        if (cooldown < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cooldown),
                "Cooldown cannot be negative.");
        }

        _cooldown = cooldown;
    }

    public bool ShouldEmit(
        Alert alert,
        IEnumerable<Alert> existingAlerts)
    {
        var previousAlert = existingAlerts
            .Where(existing =>
                existing.RuleId == alert.RuleId &&
                existing.DeviceId == alert.DeviceId &&
                existing.Metric == alert.Metric)
            .OrderByDescending(existing => existing.EndTimestamp)
            .FirstOrDefault();

        if (previousAlert is null)
        {
            return true;
        }

        var gap =
            alert.StartTimestamp - previousAlert.EndTimestamp;

        return gap > _cooldown;
    }
}