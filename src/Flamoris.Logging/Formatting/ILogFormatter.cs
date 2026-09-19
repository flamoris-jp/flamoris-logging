namespace Flamoris.Logging.Formatting;

internal interface ILogFormatter
{
    void Write(LogEvent logEvent, TextWriter writer);
}
