using SensorRuleEngine.Application.Ingestion;
using SensorRuleEngine.Application.Rules;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Application.Processing;

public sealed class ReadingProcessingOrchestrator
    : IReadingProcessingOrchestrator
{
    private readonly IReadingIngestionService _ingestionService;
    private readonly IRuleProvider _ruleProvider;
    private readonly IReadingBatchProcessor _batchProcessor;

    public ReadingProcessingOrchestrator(
        IReadingIngestionService ingestionService,
        IReadingBatchProcessor batchProcessor,
        IRuleProvider ruleProvider)
    {
        _ingestionService = ingestionService;
        _ruleProvider = ruleProvider;
        _batchProcessor = batchProcessor;
    }

    public async Task<ProcessingReport> ProcessAsync(
        Stream input,
        CancellationToken cancellationToken = default)
    {
        var ingestionResult =
            await _ingestionService.IngestAsync(
                input,
                cancellationToken);
        
        var rules = _ruleProvider.GetRules();

        return await _batchProcessor.ProcessAsync(
            ingestionResult.Readings,
            rules,
            ingestionResult.TotalLines,
            ingestionResult.Parsed,
            ingestionResult.Invalid,
            ingestionResult.Duplicates,
            cancellationToken);
    }
}