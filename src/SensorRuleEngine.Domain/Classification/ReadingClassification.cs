using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Domain.Classification;

public sealed class ReadingClassification
{
    public required ReadingKey ReadingKey { get; init; }

    public required ReadingClassificationStatus Status { get; init; }

    public IReadOnlyList<RuleResult> Violations { get; init; }
        = Array.Empty<RuleResult>();
}