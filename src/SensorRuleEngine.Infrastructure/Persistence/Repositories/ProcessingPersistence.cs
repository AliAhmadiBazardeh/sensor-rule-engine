using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Application.Processing;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Infrastructure.Persistence.Repositories;

public sealed class ProcessingPersistence : IProcessingPersistence
{
    private readonly IReadingRepository _readingRepository;
    private readonly IRuleResultRepository _ruleResultRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessingPersistence(
        IReadingRepository readingRepository,
        IRuleResultRepository ruleResultRepository,
        IAlertRepository alertRepository,
        IUnitOfWork unitOfWork)
    {
        _readingRepository = readingRepository;
        _ruleResultRepository = ruleResultRepository;
        _alertRepository = alertRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task PersistAsync(
        ReadingProcessingResult result,
        CancellationToken cancellationToken = default)
    {
        var readingKeys = result.ProcessedReadings
            .Select(reading => new ReadingKey(
                reading.DeviceId,
                reading.Metric,
                reading.Timestamp,
                reading.Sequence))
            .ToList();

        var existingReadingKeys =
            await _readingRepository.GetExistingKeysAsync(
                readingKeys,
                cancellationToken);

        foreach (var reading in result.ProcessedReadings)
        {
            var key = new ReadingKey(
                reading.DeviceId,
                reading.Metric,
                reading.Timestamp,
                reading.Sequence);

            if (existingReadingKeys.Contains(key))
            {
                continue;
            }

            await _readingRepository.AddAsync(
                reading,
                cancellationToken);
        }

        var existingRuleResultKeys =
            await _ruleResultRepository.GetExistingKeysAsync(
                result.RuleResults,
                cancellationToken);

        foreach (var ruleResult in result.RuleResults)
        {
            var key = string.Join(
                "|",
                ruleResult.RuleId,
                ruleResult.ReadingKey.DeviceId,
                ruleResult.ReadingKey.Metric,
                ruleResult.ReadingKey.Timestamp,
                ruleResult.ReadingKey.Sequence);

            if (existingRuleResultKeys.Contains(key))
            {
                continue;
            }

            await _ruleResultRepository.AddAsync(
                ruleResult,
                cancellationToken);
        }

        var existingAlertKeys =
            await _alertRepository.GetExistingKeysAsync(
                result.Alerts,
                cancellationToken);

        foreach (var alert in result.Alerts)
        {
            var key = string.Join(
                "|",
                alert.RuleId,
                alert.DeviceId,
                alert.Metric,
                alert.StartTimestamp,
                alert.EndTimestamp);

            if (existingAlertKeys.Contains(key))
            {
                continue;
            }

            await _alertRepository.AddAsync(
                alert,
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}