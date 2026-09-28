using SensorRuleEngine.Application.Ingestion;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Application.Ingestion;

public interface IReadingIngestionService
{
    Task<ReadingIngestionResult> IngestAsync(
        Stream input,
        CancellationToken cancellationToken = default);
}