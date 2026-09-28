using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Application.Processing;

public interface IReadingProcessingService
{
    ReadingProcessingResult Process(
        IEnumerable<SensorReading> readings,
        IEnumerable<Rule> rules);
}