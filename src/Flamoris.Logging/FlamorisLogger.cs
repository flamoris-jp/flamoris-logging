using Flamoris.Logging.Formatting;
using Flamoris.Logging.Internal;
using Flamoris.Logging.Sinks;

namespace Flamoris.Logging;

public sealed class FlamorisLogger
{
    private readonly NormalizedConfiguration configuration;
    private readonly IReadOnlyList<ILogSink> sinks;
    private readonly Func<DateTimeOffset> utcNow;
    private readonly Action<string>? diagnostic;

    internal FlamorisLogger(NormalizedConfiguration configuration, IReadOnlyList<ILogSink> sinks,
        Func<DateTimeOffset> utcNow, Action<string>? diagnostic = null)
    {
        this.configuration = configuration;
        this.sinks = sinks;
        this.utcNow = utcNow;
        this.diagnostic = diagnostic;
    }

    public static FlamorisLogger Create(LoggingOptions? options = null, string? basePath = null,
        Action<string>? diagnostic = null)
    {
        var configuration = ConfigurationNormalizer.Normalize(options);
        var formatter = new TextLogFormatter(configuration.UseLocalTime);
        var resolvedBasePath = Path.GetFullPath(basePath ?? AppContext.BaseDirectory);
        var sinks = new List<ILogSink>();
        foreach (var output in configuration.Outputs)
        {
            try
            {
                sinks.Add(output.Type == "console"
                    ? new ConsoleLogSink(formatter)
                    : new FileLogSink(formatter, output.Path!, resolvedBasePath,
                        output.RotationEnabled, output.MaxFileSizeMb, output.MaxFiles));
            }
            catch (Exception exception)
            {
                TryReport(diagnostic, $"Failed to configure {output.Type} logging output: {exception.Message}");
            }
        }
        if (sinks.Count == 0) sinks.Add(new ConsoleLogSink(formatter));
        var logger = new FlamorisLogger(configuration, sinks, () => DateTimeOffset.UtcNow, diagnostic);
        foreach (var warning in configuration.Warnings)
        {
            logger.EmitCore(new LogEvent(DateTimeOffset.UtcNow, LogLevel.Warn,
                "flamoris.logging.config", warning, new Dictionary<string, object?>()));
            TryReport(diagnostic, warning);
        }
        return logger;
    }

    public bool IsEnabled(LogLevel level, string? category = null) =>
        level <= ResolveThreshold(NormalizeCategory(category));

    public void Log(LogLevel level, string? category, string? message,
        IReadOnlyDictionary<string, object?>? properties = null, Exception? exception = null)
    {
        var normalizedCategory = NormalizeCategory(category);
        if (!IsEnabled(level, normalizedCategory)) return;
        var safeProperties = SensitiveDataRedactor.Redact(properties, configuration.RedactedPropertyNames);
        EmitCore(new LogEvent(utcNow(), level, normalizedCategory, message ?? string.Empty, safeProperties, exception));
    }

    public void Debug(string category, string message, IReadOnlyDictionary<string, object?>? properties = null) =>
        Log(LogLevel.Debug, category, message, properties);
    public void Info(string category, string message, IReadOnlyDictionary<string, object?>? properties = null) =>
        Log(LogLevel.Info, category, message, properties);
    public void Warn(string category, string message, IReadOnlyDictionary<string, object?>? properties = null) =>
        Log(LogLevel.Warn, category, message, properties);
    public void Error(string category, string message, Exception? exception = null,
        IReadOnlyDictionary<string, object?>? properties = null) =>
        Log(LogLevel.Error, category, message, properties, exception);

    private LogLevel ResolveThreshold(string category)
    {
        var candidate = category;
        while (true)
        {
            if (configuration.Categories.TryGetValue(candidate, out var level)) return level;
            var separator = candidate.LastIndexOf('.');
            if (separator < 0) return configuration.Level;
            candidate = candidate[..separator];
        }
    }

    private void EmitCore(LogEvent logEvent)
    {
        foreach (var sink in sinks)
        {
            try { sink.Write(logEvent); }
            catch (Exception exception)
            {
                TryReport(diagnostic, $"Logging output failed ({sink.GetType().Name}): {exception.Message}");
            }
        }
    }

    private static string NormalizeCategory(string? category) =>
        string.IsNullOrWhiteSpace(category) ? "app" : category.Trim().Trim('.');

    private static void TryReport(Action<string>? callback, string message)
    {
        if (callback is null) return;
        try { callback(message); } catch { }
    }
}
