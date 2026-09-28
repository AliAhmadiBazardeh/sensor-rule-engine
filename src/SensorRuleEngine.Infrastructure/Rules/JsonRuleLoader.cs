using System.Text.Json;
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
        catch (JsonException ex)
        {
            return new RuleLoaderResult
            {
                Rules = Array.Empty<Rule>(),
                TotalRules = 0,
                InvalidRules = 1,
                Errors = new[]
                {
                    $"Invalid JSON: {ex.Message}"
                }
            };
        }

        if (dtos is null)
        {
            return new RuleLoaderResult
            {
                Rules = Array.Empty<Rule>(),
                TotalRules = 0,
                InvalidRules = 1,
                Errors = new[]
                {
                    "rules.json does not contain a valid rule array."
                }
            };
        }

        var rules = new List<Rule>();
        var errors = new List<string>();

        for (var index = 0; index < dtos.Count; index++)
        {
            var dto = dtos[index];

            if (!TryMap(dto, out var rule, out var mappingError))
            {
                errors.Add(
                    $"Rule at index {index}: {mappingError}");

                continue;
            }

            var validationResult =
                _validator.Validate(rule);

            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    errors.Add(
                        $"Rule '{rule.Id}': {error}");
                }

                continue;
            }

            rules.Add(rule);
        }

        return new RuleLoaderResult
        {
            Rules = rules,
            TotalRules = dtos.Count,
            InvalidRules = errors.Count == 0
                ? 0
                : dtos.Count - rules.Count,
            Errors = errors
        };
    }

    private static bool TryMap(
        RuleDto dto,
        out Rule rule,
        out string error)
    {
        rule = null!;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(dto.Id))
        {
            error = "Rule id is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            error = "Rule name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Metric))
        {
            error = "Rule metric is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.Operator))
        {
            error = "Rule operator is required.";
            return false;
        }

        if (!Enum.TryParse<RuleOperatorType>(
                dto.Operator,
                ignoreCase: true,
                out var operatorType))
        {
            error =
                $"Unsupported operator '{dto.Operator}'.";

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