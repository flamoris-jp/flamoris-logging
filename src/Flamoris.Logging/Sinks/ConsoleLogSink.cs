using Flamoris.Logging.Formatting;

namespace Flamoris.Logging.Sinks;

internal sealed class ConsoleLogSink(ILogFormatter formatter, TextWriter? output = null) : ILogSink
{
    private readonly object gate = new();
    private readonly TextWriter output = output ?? Console.Out;
    public void Write(LogEvent logEvent)
    {
        var text = formatter.Format(logEvent);
        lock (gate) { output.WriteLine(text); output.Flush(); }
    }
}
