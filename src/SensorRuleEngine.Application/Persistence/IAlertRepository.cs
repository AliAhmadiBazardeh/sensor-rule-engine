using SensorRuleEngine.Domain.Entities;

namespace SensorRuleEngine.Application.Persistence;

public interface IAlertRepository
{
    Task AddAsync(
        Alert alert,
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlySet<string>> GetExistingKeysAsync(
        IReadOnlyCollection<Alert> alerts,
        CancellationToken cancellationToken = default);
}