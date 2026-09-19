using System.Collections;
using System.Globalization;
using System.Text;

namespace Flamoris.Logging.Formatting;

internal sealed class TextLogFormatter(bool useLocalTime) : ILogFormatter
{
    public string Format(LogEvent logEvent)
    {
        var timestamp = useLocalTime ? logEvent.TimestampUtc.ToLocalTime() : logEvent.TimestampUtc;
        var builder = new StringBuilder();
        builder.Append(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture));
        builder.Append(" [").Append(logEvent.Level.ToString().ToUpperInvariant().PadRight(5)).Append("] [");
        builder.Append(Escape(logEvent.Category)).Append("] ").Append(Escape(logEvent.Message));
        foreach (var property in logEvent.Properties.OrderBy(item => item.Key, StringComparer.Ordinal))
            builder.Append(' ').Append(Escape(property.Key)).Append('=').Append(FormatValue(property.Value));
        if (logEvent.Exception is not null)
            builder.AppendLine().Append(logEvent.Exception);
        return builder.ToString();
    }

    private static string FormatValue(object? value)
    {
        if (value is null) return "null";
        if (value is string text) return Quote(Escape(text));
        if (value is DateTimeOffset timestamp) return timestamp.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        if (value is DateTime dateTime) return dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        if (value is IEnumerable sequence and not string)
            return "[" + string.Join(',', sequence.Cast<object?>().Select(FormatValue)) + "]";
        return Escape(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
    }

    private static string Quote(string value) =>
        value.Any(character => char.IsWhiteSpace(character) || character is '=' or '"')
            ? $"\"{value.Replace("\"", "\\\"")}\"" : value;

    private static string Escape(string value) => value
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\r", "\\r", StringComparison.Ordinal)
        .Replace("\n", "\\n", StringComparison.Ordinal)
        .Replace("\t", "\\t", StringComparison.Ordinal);
}
