using SensorRuleEngine.Application.Ingestion;
using SensorRuleEngine.Application.Rules;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Application.Processing;

public sealed class ReadingProcessingOrchestrator
    : IReadingProcessingOrchestrator
{
    private readonly IReadingIngestionService _ingestionService;
    private readonly IRuleLoader _ruleLoader;
    private readonly IReadingBatchProcessor _batchProcessor;

    public ReadingProcessingOrchestrator(
        IReadingIngestionService ingestionService,
        IRuleLoader ruleLoader,
        IReadingBatchProcessor batchProcessor)
    {
        _ingestionService = ingestionService;
        _ruleLoader = ruleLoader;
        _batchProcessor = batchProcessor;
    }

    public async Task<ProcessingReport> ProcessAsync(
        Stream input,
        Stream rulesInput,
        CancellationToken cancellationToken = default)
    {
        var ingestionResult =
            await _ingestionService.IngestAsync(
                input,
                cancellationToken);
        
        var ruleResult =
            await _ruleLoader.LoadAsync(
                rulesInput,
                cancellationToken);

        return await _batchProcessor.ProcessAsync(
            ingestionResult.Readings,
            ruleResult.Rules,
            ingestionResult.TotalLines,
            ingestionResult.Parsed,
            ingestionResult.Invalid,
            ingestionResult.Duplicates,
            cancellationToken);
    }
}