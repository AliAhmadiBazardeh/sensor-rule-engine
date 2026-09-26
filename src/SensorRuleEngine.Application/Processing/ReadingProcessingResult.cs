using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Processing;

public sealed class ReadingProcessingResult
{
    public required IReadOnlyList<SensorReading> ProcessedReadings { get; init; }

    public required IReadOnlyList<ReadingClassification> Classifications { get; init; }

    public required IReadOnlyList<Alert> Alerts { get; init; }
}