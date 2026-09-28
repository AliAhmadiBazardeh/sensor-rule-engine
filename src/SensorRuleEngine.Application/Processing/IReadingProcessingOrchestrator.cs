using SensorRuleEngine.Application.Rules;

namespace SensorRuleEngine.Application.Processing;

public interface IReadingProcessingOrchestrator
{
    Task<ProcessingReport> ProcessAsync(
        Stream readingsInput,
        Stream rulesInput,
        CancellationToken cancellationToken = default);
}