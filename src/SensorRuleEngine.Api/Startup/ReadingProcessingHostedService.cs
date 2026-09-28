using SensorRuleEngine.Application.Processing;

namespace SensorRuleEngine.Api.Startup;

public sealed class ReadingProcessingHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReadingProcessingHostedService> _logger;

    public ReadingProcessingHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<ReadingProcessingHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(
        CancellationToken cancellationToken)
    {
        var readingsPath = Path.Combine(
            AppContext.BaseDirectory,
            "data",
            "readings.jsonl");

        if (!File.Exists(readingsPath))
        {
            throw new FileNotFoundException(
                "The readings.jsonl file was not found.",
                readingsPath);
        }

        _logger.LogInformation(
            "Starting sensor readings processing from {ReadingsPath}.",
            readingsPath);

        await using var input =
            File.OpenRead(readingsPath);

        using var scope =
            _scopeFactory.CreateScope();

        var orchestrator =
            scope.ServiceProvider
                .GetRequiredService<IReadingProcessingOrchestrator>();

        var report =
            await orchestrator.ProcessAsync(
                input,
                cancellationToken);

        _logger.LogInformation(
            """
            Sensor readings processing completed.
            TotalLines: {TotalLines}
            Parsed: {Parsed}
            Invalid: {Invalid}
            Duplicates: {Duplicates}
            Stored: {Stored}
            RulesLoaded: {RulesLoaded}
            Evaluations: {Evaluations}
            Acceptable: {Acceptable}
            Unacceptable: {Unacceptable}
            Violations: {Violations}
            Alerts: {Alerts}
            """,
            report.TotalLines,
            report.Parsed,
            report.Invalid,
            report.Duplicates,
            report.Stored,
            report.RulesLoaded,
            report.Evaluations,
            report.Acceptable,
            report.Unacceptable,
            report.Violations,
            report.Alerts);
    }

    public Task StopAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}