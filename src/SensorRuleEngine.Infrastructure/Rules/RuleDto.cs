namespace SensorRuleEngine.Infrastructure.Rules;

internal sealed class RuleDto
{
    public string? Id { get; init; }

    public string? Name { get; init; }

    public bool Enabled { get; init; }

    public string? DeviceId { get; init; }

    public string? Metric { get; init; }

    public string? Operator { get; init; }

    public decimal? Threshold { get; init; }

    public decimal? Min { get; init; }

    public decimal? Max { get; init; }

    public decimal? DurationSeconds { get; init; }
}