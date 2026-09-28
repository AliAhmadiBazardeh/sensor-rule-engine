using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Ingestion;

public sealed class ReadingIngestionResult
{
    public required IReadOnlyList<SensorReading> Readings { get; init; }

    public int TotalLines { get; init; }

    public int Parsed { get; init; }

    public int Invalid { get; init; }

    public int Duplicates { get; init; }
}