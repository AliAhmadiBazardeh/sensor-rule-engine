using SensorRuleEngine.Application.Processing;

namespace SensorRuleEngine.Application.Persistence;

public interface IProcessingPersistence
{
    Task<PersistenceResult> PersistAsync(
        ReadingProcessingResult result,
        CancellationToken cancellationToken = default);
}