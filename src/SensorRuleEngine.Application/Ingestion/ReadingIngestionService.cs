using System.Text;
using SensorRuleEngine.Domain.Readings;

namespace SensorRuleEngine.Application.Ingestion;

public sealed class ReadingIngestionService : IReadingIngestionService
{
    private readonly JsonlReadingParser _parser;
    private readonly SensorReadingValidator _validator;

    public ReadingIngestionService(
        JsonlReadingParser parser,
        SensorReadingValidator validator)
    {
        _parser = parser;
        _validator = validator;
    }

    public async Task<ReadingIngestionResult> IngestAsync(
        Stream input,
        CancellationToken cancellationToken = default)
    {
        var deduplicator =
            new ReadingDeduplicator();

        var totalLines = 0;
        var parsed = 0;
        var invalid = 0;
        var duplicates = 0;

        using var reader = new StreamReader(
            input,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            leaveOpen: true);

        while (await reader.ReadLineAsync(cancellationToken)
                   is { } line)
        {
            totalLines++;

            var parseResult = _parser.Parse(line);

            if (!parseResult.IsSuccess)
            {
                invalid++;
                continue;
            }

            parsed++;

            var reading = parseResult.Reading!;

            var validationResult =
                _validator.Validate(reading);

            if (!validationResult.IsValid)
            {
                invalid++;
                continue;
            }

            var deduplicationResult =
                deduplicator.Add(reading);

            if (deduplicationResult.IsDuplicate)
            {
                duplicates++;
            }
        }

        return new ReadingIngestionResult
        {
            Readings = deduplicator.GetAcceptedReadings(),
            TotalLines = totalLines,
            Parsed = parsed,
            Invalid = invalid,
            Duplicates = duplicates
        };
    }
}