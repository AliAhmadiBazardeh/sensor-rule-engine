using SensorRuleEngine.Domain.Alerting;
using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Readings;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Domain.Rules.SustainedAbove;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Application.Processing;

public sealed class ReadingProcessingService : IReadingProcessingService
{
    private readonly IRuleEvaluationService _ruleEvaluationService;
    private readonly IReadingClassificationService _classificationService;
    private readonly ISustainedAboveProcessor _sustainedAboveProcessor;
    private readonly AlertCooldownPolicy _alertCooldownPolicy;
    private readonly AlertDeduplicator _alertDeduplicator;

    private readonly IRuleApplicabilityChecker _applicabilityChecker;

    public ReadingProcessingService(
        IRuleEvaluationService ruleEvaluationService,
        IReadingClassificationService classificationService,
        ISustainedAboveProcessor sustainedAboveProcessor,
        IRuleApplicabilityChecker applicabilityChecker,
        AlertCooldownPolicy alertCooldownPolicy,
        AlertDeduplicator alertDeduplicator)
    {
        _ruleEvaluationService = ruleEvaluationService;
        _classificationService = classificationService;
        _sustainedAboveProcessor = sustainedAboveProcessor;
        _applicabilityChecker = applicabilityChecker;
        _alertCooldownPolicy = alertCooldownPolicy;
        _alertDeduplicator = alertDeduplicator;
    }

    public ReadingProcessingResult Process(
        IEnumerable<SensorReading> readings,
        IEnumerable<Rule> rules)
    {
        var orderedReadings =
            EventTimeReadingOrdering.Order(readings);

        var ruleList = rules.ToList();

        var classifications = new List<ReadingClassification>();
        var allRuleResults = new List<RuleResult>();
        var alerts = new List<Alert>();

        foreach (var reading in orderedReadings)
        {
            var readingRuleResults =
                _ruleEvaluationService.Evaluate(
                    reading,
                    ruleList);

            allRuleResults.AddRange(readingRuleResults);

            var classification =
                _classificationService.Classify(
                    new ReadingKey(
                        reading.DeviceId,
                        reading.Metric,
                        reading.Timestamp,
                        reading.Sequence),
                    readingRuleResults);

            classifications.Add(classification);

            foreach (var rule in ruleList)
            {
                if (
                    rule.Operator != RuleOperatorType.SustainedAbove ||
                    !_applicabilityChecker.IsApplicable(reading, rule))
                {
                    continue;
                }

                var alert =
                    _sustainedAboveProcessor.Process(
                        reading,
                        rule);

                if (alert is not null)
                {
                    TryAddAlert(alert, alerts);
                }
            }
        }

        foreach (var alert in _sustainedAboveProcessor.Complete())
        {
            TryAddAlert(alert, alerts);
        }

        return new ReadingProcessingResult
        {
            ProcessedReadings = orderedReadings,
            RuleResults = allRuleResults,
            Classifications = classifications,
            Alerts = alerts
        };
    }
    private void TryAddAlert(
        Alert alert,
        List<Alert> alerts)
    {
        if (!_alertDeduplicator.TryAdd(alert))
        {
            return;
        }

        if (!_alertCooldownPolicy.ShouldEmit(
                alert,
                alerts))
        {
            return;
        }

        alerts.Add(alert);
    }
}