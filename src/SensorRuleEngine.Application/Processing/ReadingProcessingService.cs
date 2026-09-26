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

    public ReadingProcessingService(
        IRuleEvaluationService ruleEvaluationService,
        IReadingClassificationService classificationService,
        ISustainedAboveProcessor sustainedAboveProcessor)
    {
        _ruleEvaluationService = ruleEvaluationService;
        _classificationService = classificationService;
        _sustainedAboveProcessor = sustainedAboveProcessor;
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
                    alerts.Add(alert);
                }
            }
        }

        alerts.AddRange(
            _sustainedAboveProcessor.Complete());

        return new ReadingProcessingResult
        {
            ProcessedReadings = orderedReadings,
            Classifications = classifications,
            Alerts = alerts
        };
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