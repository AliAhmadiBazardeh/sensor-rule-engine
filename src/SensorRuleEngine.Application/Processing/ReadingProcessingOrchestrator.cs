using SensorRuleEngine.Application.Ingestion;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Application.Processing;

public sealed class ReadingProcessingOrchestrator
    : IReadingProcessingOrchestrator
{
    private readonly IReadingIngestionService _ingestionService;
    private readonly IReadingBatchProcessor _batchProcessor;

    public ReadingProcessingOrchestrator(
        IReadingIngestionService ingestionService,
        IReadingBatchProcessor batchProcessor)
    {
        _ingestionService = ingestionService;
        _batchProcessor = batchProcessor;
    }

    public async Task<ProcessingReport> ProcessAsync(
        Stream input,
        IEnumerable<Rule> rules,
        CancellationToken cancellationToken = default)
    {
        var ingestionResult =
            await _ingestionService.IngestAsync(
                input,
                cancellationToken);

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