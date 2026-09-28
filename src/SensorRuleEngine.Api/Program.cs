using SensorRuleEngine.Api.Startup;
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

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
