using SensorRuleEngine.Domain.Enums;

namespace SensorRuleEngine.Domain.Entities;

public sealed class Rule
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public bool Enabled { get; init; }

    public required string Metric { get; init; }

    public string? DeviceId { get; init; }

    public required RuleOperatorType Operator { get; init; }

    public IReadOnlyDictionary<string, decimal> Parameters { get; init; }
        = new Dictionary<string, decimal>();
}
