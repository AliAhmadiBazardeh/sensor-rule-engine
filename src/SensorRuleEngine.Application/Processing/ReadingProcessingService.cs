using SensorRuleEngine.Domain.Alerting;
using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Readings;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Domain.Rules.SustainedAbove;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Application.Processing;

public sealed class ReadingProcessingService
{
    private readonly IRuleEvaluationService _ruleEvaluationService;
    private readonly IReadingClassificationService _classificationService;
    private readonly ISustainedAboveProcessor _sustainedAboveProcessor;
    private readonly AlertCooldownPolicy _alertCooldownPolicy;
    private readonly AlertDeduplicator _alertDeduplicator;

    public ReadingProcessingService(
        IRuleEvaluationService ruleEvaluationService,
        IReadingClassificationService classificationService,
        ISustainedAboveProcessor sustainedAboveProcessor,
        AlertCooldownPolicy alertCooldownPolicy,
        AlertDeduplicator alertDeduplicator)
    {
        _ruleEvaluationService = ruleEvaluationService;
        _classificationService = classificationService;
        _sustainedAboveProcessor = sustainedAboveProcessor;
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
        var alerts = new List<Alert>();

        foreach (var reading in orderedReadings)
        {
            var ruleResults =
                _ruleEvaluationService.Evaluate(
                    reading,
                    ruleList);

            var classification =
                _classificationService.Classify(
                    new ReadingKey(
                        reading.DeviceId,
                        reading.Metric,
                        reading.Timestamp,
                        reading.Sequence),
                    ruleResults);

            classifications.Add(classification);

            foreach (var rule in ruleList)
            {
                if (!rule.Enabled ||
                    rule.Operator != RuleOperatorType.SustainedAbove ||
                    !IsApplicable(reading, rule))
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

    private static bool IsApplicable(
        SensorReading reading,
        Rule rule)
    {
        if (!string.Equals(
                reading.Metric,
                rule.Metric,
                StringComparison.Ordinal))
        {
            return false;
        }

        return rule.DeviceId is null ||
               string.Equals(
                   reading.DeviceId,
                   rule.DeviceId,
                   StringComparison.Ordinal);
    }
}