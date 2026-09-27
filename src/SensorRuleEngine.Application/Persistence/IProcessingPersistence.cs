using SensorRuleEngine.Application.Processing;

namespace SensorRuleEngine.Application.Persistence;

public interface IProcessingPersistence
{
    Task PersistAsync(
        ReadingProcessingResult result,
        CancellationToken cancellationToken = default);
}