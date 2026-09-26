using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Rules.SustainedAbove;

public sealed class SustainedAboveProcessor : ISustainedAboveProcessor
{
    private readonly Dictionary<SustainedStateKey, SustainedAboveState> _states = new();

    public Alert? Process(
        SensorReading reading,
        Rule rule)
    {
        var threshold = rule.Parameters["threshold"];
        var durationSeconds = rule.Parameters["durationSeconds"];

        var key = new SustainedStateKey(
            rule.Id,
            reading.DeviceId,
            reading.Metric);

        if (reading.Value > threshold)
        {
            return ProcessAboveThreshold(
                reading,
                rule,
                key,
                durationSeconds);
        }

        return ProcessAtOrBelowThreshold(
            reading,
            rule,
            key,
            threshold);
    }

    public IReadOnlyList<Alert> Complete()
    {
        var alerts = new List<Alert>();

        foreach (var entry in _states)
        {
            var state = entry.Value;

            var duration =
                state.LastTimestamp - state.StartTimestamp;

            if (duration.TotalSeconds >= 0)
            {
                alerts.Add(new Alert
                {
                    RuleId = entry.Key.RuleId,
                    DeviceId = entry.Key.DeviceId,
                    Metric = entry.Key.Metric,
                    StartTimestamp = state.StartTimestamp,
                    EndTimestamp = state.LastTimestamp,
                    PeakValue = state.PeakValue
                });
            }
        }

        _states.Clear();

        return alerts;
    }

    private Alert? ProcessAboveThreshold(
        SensorReading reading,
        Rule rule,
        SustainedStateKey key,
        decimal durationSeconds)
    {
        if (!_states.TryGetValue(key, out var state))
        {
            _states[key] = new SustainedAboveState
            {
                StartTimestamp = reading.Timestamp,
                LastTimestamp = reading.Timestamp,
                PeakValue = reading.Value,
                DurationSeconds =  durationSeconds
            };

            return null;
        }

        // Ignore late readings that arrive after the current event-time state.
        if (reading.Timestamp < state.LastTimestamp)
        {
            return null;
        }

        state.LastTimestamp = reading.Timestamp;

        if (reading.Value > state.PeakValue)
        {
            state.PeakValue = reading.Value;
        }

        return null;
    }

    private Alert? ProcessAtOrBelowThreshold(
        SensorReading reading,
        Rule rule,
        SustainedStateKey key,
        decimal threshold)
    {
        if (!_states.Remove(key, out var state))
        {
            return null;
        }

        var duration =
            reading.Timestamp - state.StartTimestamp;

        if ((decimal)duration.TotalSeconds < rule.Parameters["durationSeconds"])
        {
            return null;
        }

        return new Alert
        {
            RuleId = rule.Id,
            DeviceId = reading.DeviceId,
            Metric = reading.Metric,
            StartTimestamp = state.StartTimestamp,
            EndTimestamp = reading.Timestamp,
            PeakValue = state.PeakValue
        };
    }
}