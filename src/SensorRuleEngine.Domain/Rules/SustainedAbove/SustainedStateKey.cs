namespace SensorRuleEngine.Domain.Rules.SustainedAbove;

public readonly record struct SustainedStateKey(
    string RuleId,
    string DeviceId,
    string Metric);