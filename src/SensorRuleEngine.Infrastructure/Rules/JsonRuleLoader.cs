using System.Text.Json;
using System.Text.Json.Serialization;
using SensorRuleEngine.Application.Rules;
using SensorRuleEngine.Domain.Entities;
using SensorRuleEngine.Domain.Enums;
using SensorRuleEngine.Domain.Rules;

namespace SensorRuleEngine.Infrastructure.Rules;

public sealed class JsonRuleLoader : IRuleLoader
{
    private readonly RuleValidator _validator;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JsonRuleLoader(
        RuleValidator validator)
    {
        _validator = validator;
    }

    public async Task<RuleLoaderResult> LoadAsync(
        Stream input,
        CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(
            input,
            leaveOpen: true);

        var json = await reader.ReadToEndAsync(
            cancellationToken);

        List<RuleDto>? dtos;

        try
        {
            dtos = JsonSerializer.Deserialize<List<RuleDto>>(
                json,
                _jsonOptions);
        }
        catch (JsonException)
        {
            return new RuleLoaderResult
            {
                Rules = Array.Empty<Rule>(),
                TotalRules = 0,
                InvalidRules = 1
            };
        }

        if (dtos is null)
        {
            return new RuleLoaderResult
            {
                Rules = Array.Empty<Rule>(),
                TotalRules = 0,
                InvalidRules = 1
            };
        }

        var rules = new List<Rule>();
        var invalidRules = 0;

        foreach (var dto in dtos)
        {
            if (!TryMap(dto, out var rule))
            {
                invalidRules++;
                continue;
            }

            var validationResult =
                _validator.Validate(rule);

            if (!validationResult.IsValid)
            {
                invalidRules++;
                continue;
            }

            rules.Add(rule);
        }

        return new RuleLoaderResult
        {
            Rules = rules,
            TotalRules = dtos.Count,
            InvalidRules = invalidRules
        };
    }

    private static bool TryMap(
        RuleDto dto,
        out Rule rule)
    {
        rule = null!;

        if (string.IsNullOrWhiteSpace(dto.Operator))
        {
            return false;
        }

        if (!Enum.TryParse<RuleOperatorType>(
                dto.Operator,
                ignoreCase: true,
                out var operatorType))
        {
            return false;
        }

        var parameters =
            new Dictionary<string, decimal>();

        if (dto.Threshold.HasValue)
        {
            parameters["threshold"] =
                dto.Threshold.Value;
        }

        if (dto.Min.HasValue)
        {
            parameters["min"] =
                dto.Min.Value;
        }

        if (dto.Max.HasValue)
        {
            parameters["max"] =
                dto.Max.Value;
        }

        if (dto.DurationSeconds.HasValue)
        {
            parameters["durationSeconds"] =
                dto.DurationSeconds.Value;
        }

        if (string.IsNullOrWhiteSpace(dto.Id) ||
            string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Metric))
        {
            return false;
        }

        rule = new Rule
        {
            Id = dto.Id,
            Name = dto.Name,
            Enabled = dto.Enabled,
            DeviceId = dto.DeviceId,
            Metric = dto.Metric,
            Operator = operatorType,
            Parameters = parameters
        };

        return true;
    }
}