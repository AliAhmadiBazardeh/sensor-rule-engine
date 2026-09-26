namespace SensorRuleEngine.Domain.Readings;

public sealed class ReadingDeduplicationResult
{
    private ReadingDeduplicationResult(
        bool isAccepted,
        bool isDuplicate)
    {
        IsAccepted = isAccepted;
        IsDuplicate = isDuplicate;
    }

    public bool IsAccepted { get; }

    public bool IsDuplicate { get; }

    public static ReadingDeduplicationResult Accepted()
    {
        return new ReadingDeduplicationResult(
            true,
            false);
    }

    public static ReadingDeduplicationResult Duplicate()
    {
        return new ReadingDeduplicationResult(
            false,
            true);
    }
}