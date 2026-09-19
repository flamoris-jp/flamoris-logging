namespace Flamoris.Logging;

public sealed record LogEvent(
    DateTimeOffset TimestampUtc,
    LogLevel Level,
    string Category,
    string Message,
    IReadOnlyDictionary<string, object?> Properties,
    Exception? Exception = null)
{
    public DateTimeOffset TimestampUtc { get; init; } = TimestampUtc.ToUniversalTime();
}
