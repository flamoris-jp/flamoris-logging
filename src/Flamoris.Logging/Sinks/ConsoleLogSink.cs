using Flamoris.Logging.Formatting;

namespace Flamoris.Logging.Sinks;

internal sealed class ConsoleLogSink(ILogFormatter formatter, TextWriter? output = null) : ILogSink
{
    private readonly object gate = new();
    private readonly TextWriter output = output ?? Console.Out;
    public void Write(LogEvent logEvent)
    {
        lock (gate)
        {
            formatter.Write(logEvent, output);
            output.WriteLine();
            output.Flush();
        }
    }
}
