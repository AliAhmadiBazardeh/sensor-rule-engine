using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Application.Processing;
using SensorRuleEngine.Domain.Alerting;
using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Domain.Rules.SustainedAbove;

namespace SensorRuleEngine.IntegrationTests.Persistence;

internal static class TestServiceFactory
{
    public static ReadingBatchProcessor CreateBatchProcessor(
        IProcessingPersistence persistence)
    {
        var applicabilityChecker =
            new RuleApplicabilityChecker();

        var operators = new IRuleOperator[]
        {
            new GreaterThanOperator(),
            new GreaterThanOrEqualOperator(),
            new LessThanOperator(),
            new LessThanOrEqualOperator(),
            new EqualOperator(),
            new BetweenOperator()
        };

        var operatorResolver =
            new RuleOperatorResolver(operators);

        var evaluationService =
            new RuleEvaluationService(
                applicabilityChecker,
                operatorResolver);

        var classificationService =
            new ReadingClassificationService();

        var sustainedAboveProcessor =
            new SustainedAboveProcessor();

        var cooldownPolicy =
            new AlertCooldownPolicy(
                TimeSpan.FromMinutes(5));

        var alertDeduplicator =
            new AlertDeduplicator();

        var readingProcessingService =
            new ReadingProcessingService(
                evaluationService,
                classificationService,
                sustainedAboveProcessor,
                applicabilityChecker,
                cooldownPolicy,
                alertDeduplicator);

        return new ReadingBatchProcessor(
            readingProcessingService,
            persistence);
    }
}