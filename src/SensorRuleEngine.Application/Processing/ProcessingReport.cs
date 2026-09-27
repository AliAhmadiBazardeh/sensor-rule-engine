namespace SensorRuleEngine.Application.Processing;

public sealed class ProcessingReport
{
    public int TotalLines { get; init; }

    public int Parsed { get; init; }

    public int Stored { get; init; }

    public int Duplicates { get; init; }

    public int Invalid { get; init; }

    public int RulesLoaded { get; init; }

    public int Evaluations { get; init; }

    public int Acceptable { get; init; }

    public int Unacceptable { get; init; }

    public int Violations { get; init; }

    public int Alerts { get; init; }
}