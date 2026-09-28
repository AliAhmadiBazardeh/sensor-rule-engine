using Microsoft.OpenApi.Models;
using SensorRuleEngine.Api.Startup;
using SensorRuleEngine.Application.Aggregation;
using SensorRuleEngine.Application.Ingestion;
using SensorRuleEngine.Application.Processing;
using SensorRuleEngine.Application.Rules;
using SensorRuleEngine.Domain.Alerting;
using SensorRuleEngine.Domain.Classification;
using SensorRuleEngine.Domain.Readings;
using SensorRuleEngine.Domain.Rules;
using SensorRuleEngine.Domain.Rules.SustainedAbove;
using SensorRuleEngine.Infrastructure;
using SensorRuleEngine.Infrastructure.Rules;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddScoped<JsonlReadingParser>();
builder.Services.AddScoped<SensorReadingValidator>();
builder.Services.AddScoped<RuleValidator>();

builder.Services.AddScoped<
    IReadingIngestionService,
    ReadingIngestionService>();

builder.Services.AddScoped<
    IReadingBatchProcessor,
    ReadingBatchProcessor>();

builder.Services.AddScoped<
    IReadingProcessingOrchestrator,
    ReadingProcessingOrchestrator>();

builder.Services.AddHostedService<
    ReadingProcessingHostedService>();

builder.Services.AddScoped<
    IRuleApplicabilityChecker,
    RuleApplicabilityChecker>();

builder.Services.AddScoped<
    IReadingClassificationService,
    ReadingClassificationService>();

builder.Services.AddScoped<
    ISustainedAboveProcessor,
    SustainedAboveProcessor>();

builder.Services.AddScoped<AlertDeduplicator>();

builder.Services.AddScoped<AlertCooldownPolicy>(_ =>
    new AlertCooldownPolicy(
        TimeSpan.FromMinutes(5)));

builder.Services.AddScoped<IRuleOperatorResolver>(sp =>
{
    var operators = new IRuleOperator[]
    {
        new GreaterThanOperator(),
        new GreaterThanOrEqualOperator(),
        new LessThanOperator(),
        new LessThanOrEqualOperator(),
        new EqualOperator(),
        new BetweenOperator()
    };

    return new RuleOperatorResolver(operators);
});

builder.Services.AddScoped<IRuleEvaluationService>(sp =>
{
    var applicabilityChecker =
        sp.GetRequiredService<IRuleApplicabilityChecker>();

    var operatorResolver =
        sp.GetRequiredService<IRuleOperatorResolver>();

    return new RuleEvaluationService(
        applicabilityChecker,
        operatorResolver);
});

builder.Services.AddScoped<
    IReadingProcessingService,
    ReadingProcessingService>();

builder.Services.AddScoped<
    IRuleLoader,
    JsonRuleLoader>();

var ruleLoader = new JsonRuleLoader(
    new RuleValidator());

var rulesPath = Path.Combine(
    AppContext.BaseDirectory,
    "data",
    "rules.json");

if (!File.Exists(rulesPath))
{
    throw new FileNotFoundException(
        "The rules.json file was not found.",
        rulesPath);
}

await using var rulesStream =
    File.OpenRead(rulesPath);

var ruleLoadResult =
    await ruleLoader.LoadAsync(rulesStream);

if (ruleLoadResult.InvalidRules > 0)
{
    var errors = string.Join(
        Environment.NewLine,
        ruleLoadResult.Errors);

    throw new InvalidOperationException(
        $"rules.json contains " +
        $"{ruleLoadResult.InvalidRules} invalid rule(s)." +
        Environment.NewLine +
        errors);
}

builder.Services.AddSingleton<IRuleProvider>(
    new InMemoryRuleProvider(
        ruleLoadResult.Rules));

builder.Services.AddScoped<AggregationQueryValidator>();

builder.Services.AddScoped<
    IAggregationService,
    AggregationService>();


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Sensor Rule Engine",
            Version = "v1"
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Sensor Rule Engine");
    });
}

app.UseHttpsRedirection();

app.MapGet(
        "/api/v1/aggregation",
        async (
            string? deviceId,
            string? metric,
            DateTimeOffset? from,
            DateTimeOffset? to,
            int? bucketSeconds,
            AggregationQueryValidator validator,
            IAggregationService aggregationService,
            CancellationToken cancellationToken) =>
        {
            var query = new AggregationQuery
            {
                DeviceId = deviceId ?? string.Empty,
                Metric = metric ?? string.Empty,
                From = from ?? default,
                To = to ?? default,
                BucketSeconds = bucketSeconds ?? 0
            };

            var errors = validator.Validate(query);

            if (errors.Count > 0)
            {
                return Results.BadRequest(new
                {
                    errors
                });
            }

            var result =
                await aggregationService.AggregateAsync(
                    query,
                    cancellationToken);

            return Results.Ok(result);
        })
    .WithName("GetAggregation")
    .WithTags("Aggregation");

app.Run();