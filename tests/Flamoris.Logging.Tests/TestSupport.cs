using Flamoris.Logging.Internal;
using Flamoris.Logging.Sinks;

namespace Flamoris.Logging.Tests;

internal sealed class MemorySink : ILogSink
{
    private readonly object gate = new();
    private readonly List<LogEvent> events = [];
    public IReadOnlyList<LogEvent> Events { get { lock (gate) return events.ToArray(); } }
    public void Write(LogEvent logEvent) { lock (gate) events.Add(logEvent); }
}

internal sealed class ThrowingSink : ILogSink
{
    public void Write(LogEvent logEvent) => throw new IOException("sink unavailable");
}

internal static class TestLogger
{
    public static FlamorisLogger Create(LoggingOptions options, IReadOnlyList<ILogSink> sinks,
        Action<string>? diagnostic = null, DateTimeOffset? now = null) =>
        new(ConfigurationNormalizer.Normalize(options), sinks,
            () => now ?? DateTimeOffset.UtcNow, diagnostic);
}

internal sealed class TempDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(
        System.IO.Path.GetTempPath(), "Flamoris.Logging.Tests", Guid.NewGuid().ToString("N"));
    public TempDirectory() => Directory.CreateDirectory(Path);
    public void Dispose() { try { Directory.Delete(Path, true); } catch { } }
}
