using Microsoft.EntityFrameworkCore;
using SensorRuleEngine.Application.Persistence;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Infrastructure.Persistence.Mappers;

namespace SensorRuleEngine.Infrastructure.Persistence.Repositories;

public sealed class RuleResultRepository
    : IRuleResultRepository
{
    private readonly SensorRuleEngineDbContext _dbContext;

    public RuleResultRepository(
        SensorRuleEngineDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        RuleResult result,
        CancellationToken cancellationToken = default)
    {
        var entity = RuleResultMapper.ToEntity(result);

        await _dbContext.RuleResults.AddAsync(
            entity,
            cancellationToken);
    }
    
    public async Task<IReadOnlySet<string>> GetExistingKeysAsync(
    IReadOnlyCollection<RuleResult> results,
    CancellationToken cancellationToken = default)
    {
        if (results.Count == 0)
        {
            return new HashSet<string>();
        }

        var ruleIds = results
            .Select(result => result.RuleId)
            .Distinct()
            .ToList();

        var deviceIds = results
            .Select(result => result.ReadingKey.DeviceId)
            .Distinct()
            .ToList();

        var metrics = results
            .Select(result => result.ReadingKey.Metric)
            .Distinct()
            .ToList();

        var timestamps = results
            .Select(result => result.ReadingKey.Timestamp)
            .Distinct()
            .ToList();

        var sequences = results
            .Select(result => result.ReadingKey.Sequence)
            .Distinct()
            .ToList();

        var existingResults =
            await _dbContext.RuleResults
                .AsNoTracking()
                .Where(result =>
                    ruleIds.Contains(result.RuleId) &&
                    deviceIds.Contains(result.DeviceId) &&
                    metrics.Contains(result.Metric) &&
                    timestamps.Contains(result.Timestamp) &&
                    sequences.Contains(result.Sequence))
                .Select(result => new
                {
                    result.RuleId,
                    result.DeviceId,
                    result.Metric,
                    result.Timestamp,
                    result.Sequence
                })
                .ToListAsync(cancellationToken);

        return existingResults
            .Select(result =>
                CreateKey(
                    result.RuleId,
                    result.DeviceId,
                    result.Metric,
                    result.Timestamp,
                    result.Sequence))
            .ToHashSet();
    }

    private static string CreateKey(
        string ruleId,
        string deviceId,
        string metric,
        DateTimeOffset timestamp,
        int sequence)
    {
        return string.Join(
            "|",
            ruleId,
            deviceId,
            metric,
            timestamp,
            sequence);
    }
    
}