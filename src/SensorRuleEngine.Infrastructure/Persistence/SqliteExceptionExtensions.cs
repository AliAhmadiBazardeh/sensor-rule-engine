using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace SensorRuleEngine.Infrastructure.Persistence;

internal static class SqliteExceptionExtensions
{
    public static bool IsUniqueConstraintViolation(
        this DbUpdateException exception)
    {
        return exception.InnerException is SqliteException sqliteException
               && sqliteException.SqliteErrorCode == 19;
    }
}