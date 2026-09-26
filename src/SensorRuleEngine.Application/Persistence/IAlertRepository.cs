using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Persistence;

public interface IAlertRepository
{
    Task<bool> AddAsync(
        Alert alert,
        CancellationToken cancellationToken = default);
}