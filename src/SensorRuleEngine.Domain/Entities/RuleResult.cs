using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Domain.Entities;

public sealed class RuleResult
{
    public required string RuleId { get; init; }

    public required ReadingKey ReadingKey { get; init; }

    public required RuleResultStatus Status { get; init; }

    public string? Reason { get; init; }
}
