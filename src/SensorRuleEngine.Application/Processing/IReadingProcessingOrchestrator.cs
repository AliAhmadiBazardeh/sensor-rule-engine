using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Application.Processing;

public interface IReadingProcessingOrchestrator
{
    Task<ProcessingReport> ProcessAsync(
        Stream input,
        IEnumerable<Rule> rules,
        CancellationToken cancellationToken = default);
}