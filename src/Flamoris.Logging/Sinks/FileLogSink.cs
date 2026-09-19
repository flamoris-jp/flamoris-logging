using System.Text;
using Flamoris.Logging.Formatting;

namespace Flamoris.Logging.Sinks;

internal sealed class FileLogSink : ILogSink
{
    private static readonly UTF8Encoding Utf8 = new(false);
    private readonly object gate = new();
    private readonly ILogFormatter formatter;
    private readonly string path;
    private readonly bool rotationEnabled;
    private readonly long maxFileSizeBytes;
    private readonly int maxFiles;

    public FileLogSink(ILogFormatter formatter, string configuredPath, string basePath,
        bool rotationEnabled, int maxFileSizeMb, int maxFiles)
        : this(formatter, configuredPath, basePath, rotationEnabled,
            checked((long)maxFileSizeMb * 1024 * 1024), maxFiles) { }

    internal FileLogSink(ILogFormatter formatter, string configuredPath, string basePath,
        bool rotationEnabled, long maxFileSizeBytes, int maxFiles)
    {
        this.formatter = formatter;
        path = Path.GetFullPath(configuredPath, basePath);
        this.rotationEnabled = rotationEnabled;
        this.maxFileSizeBytes = maxFileSizeBytes;
        this.maxFiles = maxFiles;
    }

    internal string ResolvedPath => path;

    public void Write(LogEvent logEvent)
    {
        var payload = new BoundedUtf8TextWriter(maxFileSizeBytes);
        formatter.Write(logEvent, payload);
        payload.Write(Environment.NewLine);
        var bytes = payload.IsTruncated ? CreateOversizedEventNotice() : payload.ToArray();

        lock (gate)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            if (rotationEnabled && File.Exists(path))
            {
                var length = new FileInfo(path).Length;
                if (length > 0 && length + bytes.Length > maxFileSizeBytes) Rotate();
            }
            using var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            stream.Write(bytes);
            stream.Flush();
        }
    }

    private byte[] CreateOversizedEventNotice()
    {
        // A configured rotating file must never exceed its size bound because of
        // one pathological event. The normalized public configuration has a
        // minimum size of one MiB; this extra guard also keeps test/internal
        // construction bounded for smaller limits.
        var notice = Utf8.GetBytes(
            $"[TRUNCATED oversized log event limitBytes={maxFileSizeBytes}]"
            + Environment.NewLine);
        if (notice.Length <= maxFileSizeBytes) return notice;

        return maxFileSizeBytes > 0 ? [(byte)'!'] : [];
    }

    private void Rotate()
    {
        if (maxFiles <= 1) { File.Delete(path); return; }
        var archiveCount = maxFiles - 1;
        File.Delete($"{path}.{archiveCount}");
        for (var index = archiveCount - 1; index >= 1; index--)
        {
            var source = $"{path}.{index}";
            if (File.Exists(source)) File.Move(source, $"{path}.{index + 1}");
        }
        if (File.Exists(path)) File.Move(path, $"{path}.1");
    }
}
