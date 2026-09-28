using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Rules;

public sealed class RuleLoaderResult
{
    public required IReadOnlyList<Rule> Rules { get; init; }

    public int TotalRules { get; init; }

    public int InvalidRules { get; init; }
}