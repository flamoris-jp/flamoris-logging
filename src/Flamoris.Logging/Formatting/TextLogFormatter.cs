using System.Collections;
using System.Globalization;

namespace Flamoris.Logging.Formatting;

internal sealed class TextLogFormatter(bool useLocalTime) : ILogFormatter
{
    public void Write(LogEvent logEvent, TextWriter writer)
    {
        var timestamp = useLocalTime ? logEvent.TimestampUtc.ToLocalTime() : logEvent.TimestampUtc;
        writer.Write(timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz", CultureInfo.InvariantCulture));
        writer.Write(" [");
        writer.Write(logEvent.Level.ToString().ToUpperInvariant().PadRight(5));
        writer.Write("] [");
        WriteEscaped(writer, logEvent.Category);
        writer.Write("] ");
        WriteEscaped(writer, logEvent.Message);

        foreach (var property in logEvent.Properties.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            if (IsTruncated(writer)) return;
            writer.Write(' ');
            WriteEscaped(writer, property.Key);
            writer.Write('=');
            WriteValue(writer, property.Value);
        }

        if (logEvent.Exception is not null && !IsTruncated(writer))
        {
            writer.WriteLine();
            if (writer is not BoundedUtf8TextWriter)
            {
                writer.Write(logEvent.Exception);
                return;
            }
            writer.Write(logEvent.Exception.GetType().FullName);
            writer.Write(": ");
            WriteEscaped(writer, logEvent.Exception.Message);
            if (!IsTruncated(writer)) writer.WriteLine(" [exception details omitted from bounded sinks]");
        }
    }

    private static void WriteValue(TextWriter writer, object? value)
    {
        if (value is null) { writer.Write("null"); return; }
        if (value is string text) { WriteString(writer, text); return; }
        if (value is DateTimeOffset timestamp)
        {
            writer.Write(timestamp.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            return;
        }
        if (value is DateTime dateTime)
        {
            writer.Write(dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
            return;
        }
        if (value is IEnumerable sequence and not string)
        {
            writer.Write('[');
            var first = true;
            foreach (var item in sequence)
            {
                if (IsTruncated(writer)) return;
                if (!first) writer.Write(',');
                WriteValue(writer, item);
                first = false;
            }
            if (!IsTruncated(writer)) writer.Write(']');
            return;
        }

        WriteEscaped(writer, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
    }

    private static void WriteString(TextWriter writer, string value)
    {
        var quote = value.Length > 256 || value.Take(256).Any(character =>
            char.IsWhiteSpace(character) || character is '=' or '"');
        if (quote) writer.Write('"');
        WriteEscaped(writer, value);
        if (quote && !IsTruncated(writer)) writer.Write('"');
    }

    private static void WriteEscaped(TextWriter writer, string value)
    {
        foreach (var character in value)
        {
            if (IsTruncated(writer)) return;
            switch (character)
            {
                case '\\': writer.Write("\\\\"); break;
                case '\r': writer.Write("\\r"); break;
                case '\n': writer.Write("\\n"); break;
                case '\t': writer.Write("\\t"); break;
                default: writer.Write(character); break;
            }
        }
    }

    private static bool IsTruncated(TextWriter writer) =>
        writer is BoundedUtf8TextWriter bounded && bounded.IsTruncated;
}
