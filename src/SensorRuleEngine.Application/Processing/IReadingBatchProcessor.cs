using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Processing;

public interface IReadingBatchProcessor
{
    Task<ProcessingReport> ProcessAsync(
        IEnumerable<SensorReading> readings,
        IEnumerable<Rule> rules,
        int totalLines,
        int parsed,
        int invalid,
        int duplicates,
        CancellationToken cancellationToken = default);
}