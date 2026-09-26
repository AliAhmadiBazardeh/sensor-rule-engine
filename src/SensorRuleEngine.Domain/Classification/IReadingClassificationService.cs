using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.ValueObjects;

namespace SensorRuleEngine.Domain.Classification;

public interface IReadingClassificationService
{
    ReadingClassification Classify(
        ReadingKey readingKey,
        IEnumerable<RuleResult> ruleResults);
}