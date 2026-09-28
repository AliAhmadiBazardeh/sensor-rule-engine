using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;

namespace SensorRuleEngine.Application.Processing;

public sealed class ReadingBatchProcessor : IReadingBatchProcessor
{
    private readonly IReadingProcessingService _readingProcessingService;
    private readonly IProcessingPersistence _processingPersistence;

    public ReadingBatchProcessor(
        IReadingProcessingService readingProcessingService,
        IProcessingPersistence processingPersistence)
    {
        _readingProcessingService = readingProcessingService;
        _processingPersistence = processingPersistence;
    }

    public async Task<ProcessingReport> ProcessAsync(
        IEnumerable<SensorReading> readings,
        IEnumerable<Rule> rules,
        int totalLines,
        int parsed,
        int invalid,
        int duplicates,
        CancellationToken cancellationToken = default)
    {
        var readingList = readings.ToList();
        var ruleList = rules.ToList();

        var result = _readingProcessingService.Process(
            readingList,
            ruleList);

        var persistenceResult =
            await _processingPersistence.PersistAsync(
                result,
                cancellationToken);

        var violations = result.RuleResults
            .Count(x => x.Status == RuleResultStatus.Violated);

        var unacceptable = result.Classifications
            .Count(x =>
                x.Status == ReadingClassificationStatus.Unacceptable);

        var acceptable = result.Classifications.Count - unacceptable;

        return new ProcessingReport
        {
            TotalLines = totalLines,
            Parsed = parsed,
            Invalid = invalid,
            Duplicates = duplicates,

            Stored = persistenceResult.ReadingsStored,
            
            RulesLoaded = ruleList.Count,

            Evaluations = result.RuleResults.Count,

            Acceptable = acceptable,

            Unacceptable = unacceptable,

            Violations = violations,

            Alerts = result.Alerts.Count
        };
    }
}