using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.ValueObjects;
using Xunit;

namespace SensorRuleEngine.UnitTests.Classification;

public sealed class ReadingClassificationServiceTests
{
    [Fact]
    public void Classify_ShouldReturnAcceptable_WhenAllRulesAreSatisfied()
    {
        var readingKey = CreateReadingKey();

        var results = new[]
        {
            CreateResult(
                readingKey,
                "rule-1",
                RuleResultStatus.Satisfied),

            CreateResult(
                readingKey,
                "rule-2",
                RuleResultStatus.Satisfied)
        };

        var service = new ReadingClassificationService();

        var classification = service.Classify(
            readingKey,
            results);

        Assert.Equal(
            ReadingClassificationStatus.Acceptable,
            classification.Status);

        Assert.Empty(
            classification.Violations);
    }

    [Fact]
    public void Classify_ShouldReturnUnacceptable_WhenAtLeastOneRuleIsViolated()
    {
        var readingKey = CreateReadingKey();

        var results = new[]
        {
            CreateResult(
                readingKey,
                "rule-1",
                RuleResultStatus.Satisfied),

            CreateResult(
                readingKey,
                "rule-2",
                RuleResultStatus.Violated,
                "Value 90 is greater than threshold 80.")
        };

        var service = new ReadingClassificationService();

        var classification = service.Classify(
            readingKey,
            results);

        Assert.Equal(
            ReadingClassificationStatus.Unacceptable,
            classification.Status);

        var violation = Assert.Single(
            classification.Violations);

        Assert.Equal(
            "rule-2",
            violation.RuleId);

        Assert.Equal(
            "Value 90 is greater than threshold 80.",
            violation.Reason);
    }

    [Fact]
    public void Classify_ShouldIncludeAllViolations()
    {
        var readingKey = CreateReadingKey();

        var results = new[]
        {
            CreateResult(
                readingKey,
                "rule-1",
                RuleResultStatus.Violated),

            CreateResult(
                readingKey,
                "rule-2",
                RuleResultStatus.Violated)
        };

        var service = new ReadingClassificationService();

        var classification = service.Classify(
            readingKey,
            results);

        Assert.Equal(
            ReadingClassificationStatus.Unacceptable,
            classification.Status);

        Assert.Equal(
            2,
            classification.Violations.Count);
    }

    [Fact]
    public void Classify_ShouldReturnAcceptable_WhenNoRulesAreApplicable()
    {
        var readingKey = CreateReadingKey();

        var service = new ReadingClassificationService();

        var classification = service.Classify(
            readingKey,
            Array.Empty<RuleResult>());

        Assert.Equal(
            ReadingClassificationStatus.Acceptable,
            classification.Status);

        Assert.Empty(
            classification.Violations);
    }

    [Fact]
    public void Classify_ShouldPreserveReadingKey()
    {
        var readingKey = CreateReadingKey();

        var service = new ReadingClassificationService();

        var classification = service.Classify(
            readingKey,
            Array.Empty<RuleResult>());

        Assert.Equal(
            readingKey,
            classification.ReadingKey);
    }

    private static ReadingKey CreateReadingKey()
    {
        return new ReadingKey(
            "PUMP-01",
            "temperature",
            DateTimeOffset.Parse(
                "2025-06-01T08:00:00Z"),
            1);
    }

    private static RuleResult CreateResult(
        ReadingKey readingKey,
        string ruleId,
        RuleResultStatus status,
        string? reason = null)
    {
        return new RuleResult
        {
            RuleId = ruleId,
            ReadingKey = readingKey,
            Status = status,
            Reason = reason
        };
    }
}