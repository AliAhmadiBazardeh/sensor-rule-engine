using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Domain.Classification;

public sealed class ReadingClassificationService
    : IReadingClassificationService
{
    public ReadingClassification Classify(
        ReadingKey readingKey,
        IEnumerable<RuleResult> ruleResults)
    {
        var violations = ruleResults
            .Where(result =>
                result.Status == RuleResultStatus.Violated)
            .ToList();

        var status = violations.Count > 0
            ? ReadingClassificationStatus.Unacceptable
            : ReadingClassificationStatus.Acceptable;

        return new ReadingClassification
        {
            ReadingKey = readingKey,
            Status = status,
            Violations = violations
        };
    }
}