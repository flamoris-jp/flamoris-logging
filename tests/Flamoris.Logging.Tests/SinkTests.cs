using System.Text;
using Flamoris.Logging.Formatting;
using Flamoris.Logging.Sinks;

namespace Flamoris.Logging.Tests;

public sealed class SinkTests
{
    private static readonly LogEvent Event = new(
        DateTimeOffset.UnixEpoch, LogLevel.Info, "app", "hello", new Dictionary<string, object?>());

    [Fact]
    public void Console_sink_writes_formatted_event()
    {
        using var writer = new StringWriter();
        new ConsoleLogSink(new TextLogFormatter(false), writer).Write(Event);
        Assert.Contains("[INFO ] [app] hello", writer.ToString());
    }

    [Fact]
    public void File_sink_creates_directory_and_appends()
    {
        using var temp = new TempDirectory();
        var sink = new FileLogSink(new TextLogFormatter(false), "nested/app.log", temp.Path, false, 1, 2);
        sink.Write(Event); sink.Write(Event with { Message = "again" });
        Assert.Equal(2, File.ReadAllLines(sink.ResolvedPath).Length);
        Assert.Contains("again", File.ReadAllText(sink.ResolvedPath));
    }

    [Fact]
    public void Relative_path_uses_explicit_base_path()
    {
        using var temp = new TempDirectory();
        var sink = new FileLogSink(new TextLogFormatter(false), "logs/app.log", temp.Path, false, 1, 2);
        Assert.Equal(System.IO.Path.Combine(temp.Path, "logs", "app.log"), sink.ResolvedPath);
    }

    [Fact]
    public void Rotation_obeys_boundary_and_retention()
    {
        using var temp = new TempDirectory();
        var formatter = new ConstantFormatter("1234");
        var lineBytes = Encoding.UTF8.GetByteCount("1234" + Environment.NewLine);
        var sink = new FileLogSink(formatter, "app.log", temp.Path, true, (long)lineBytes * 2, 3);
        sink.Write(Event); sink.Write(Event);
        Assert.False(File.Exists(sink.ResolvedPath + ".1"));
        sink.Write(Event);
        Assert.Equal(2, File.ReadAllLines(sink.ResolvedPath + ".1").Length);
        for (var index = 0; index < 8; index++) sink.Write(Event);
        Assert.True(File.Exists(sink.ResolvedPath + ".2"));
        Assert.False(File.Exists(sink.ResolvedPath + ".3"));
    }

    [Fact]
    public void Oversized_event_is_replaced_with_a_bounded_notice()
    {
        using var temp = new TempDirectory();
        const int maxBytes = 96;
        var sink = new FileLogSink(
            new ConstantFormatter(new string('x', 1_000)),
            "app.log",
            temp.Path,
            rotationEnabled: true,
            maxFileSizeBytes: maxBytes,
            maxFiles: 3);

        sink.Write(Event);

        var bytes = File.ReadAllBytes(sink.ResolvedPath);
        Assert.InRange(bytes.Length, 1, maxBytes);
        Assert.Contains("[TRUNCATED oversized log event", Encoding.UTF8.GetString(bytes));
    }

    private sealed class ConstantFormatter(string value) : ILogFormatter
    {
        public string Format(LogEvent logEvent) => value;
    }
}
