using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Infrastructure.Persistence.Mappers;

namespace SensorRuleEngine.Infrastructure.Persistence.Repositories;

public sealed class AlertRepository : IAlertRepository
{
    private readonly SensorRuleEngineDbContext _dbContext;

    public AlertRepository(
        SensorRuleEngineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Alert alert,
        CancellationToken cancellationToken = default)
    {
        var entity = AlertMapper.ToEntity(alert);

        await _dbContext.Alerts.AddAsync(
            entity,
            cancellationToken);
        
    }
    
    public async Task<IReadOnlySet<string>> GetExistingKeysAsync(
    IReadOnlyCollection<Alert> alerts,
    CancellationToken cancellationToken = default)
{
    if (alerts.Count == 0)
    {
        return new HashSet<string>();
    }

    var ruleIds = alerts
        .Select(alert => alert.RuleId)
        .Distinct()
        .ToList();

    var deviceIds = alerts
        .Select(alert => alert.DeviceId)
        .Distinct()
        .ToList();

    var metrics = alerts
        .Select(alert => alert.Metric)
        .Distinct()
        .ToList();

    var startTimestamps = alerts
        .Select(alert => alert.StartTimestamp)
        .Distinct()
        .ToList();

    var endTimestamps = alerts
        .Select(alert => alert.EndTimestamp)
        .Distinct()
        .ToList();

    var existingAlerts =
        await _dbContext.Alerts
            .AsNoTracking()
            .Where(alert =>
                ruleIds.Contains(alert.RuleId) &&
                deviceIds.Contains(alert.DeviceId) &&
                metrics.Contains(alert.Metric) &&
                startTimestamps.Contains(alert.StartTimestamp) &&
                endTimestamps.Contains(alert.EndTimestamp))
            .Select(alert => new
            {
                alert.RuleId,
                alert.DeviceId,
                alert.Metric,
                alert.StartTimestamp,
                alert.EndTimestamp
            })
            .ToListAsync(cancellationToken);

    return existingAlerts
        .Select(alert =>
            CreateKey(
                alert.RuleId,
                alert.DeviceId,
                alert.Metric,
                alert.StartTimestamp,
                alert.EndTimestamp))
        .ToHashSet();
}

private static string CreateKey(
    string ruleId,
    string deviceId,
    string metric,
    DateTimeOffset startTimestamp,
    DateTimeOffset endTimestamp)
{
    return string.Join(
        "|",
        ruleId,
        deviceId,
        metric,
        startTimestamp,
        endTimestamp);
}
}