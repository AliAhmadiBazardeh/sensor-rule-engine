namespace SensorRuleEngine.Domain.Rules.SustainedAbove;

public sealed class SustainedAboveState
{
    public required DateTimeOffset StartTimestamp { get; init; }

    public required DateTimeOffset LastTimestamp { get; set; }

    public required decimal PeakValue { get; set; }
    
    public required decimal DurationSeconds { get; init; }

}