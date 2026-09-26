using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Domain.Rules.SustainedAbove;

public interface ISustainedAboveProcessor
{
    Alert? Process(
        SensorReading reading,
        Rule rule);

    IReadOnlyList<Alert> Complete();
}