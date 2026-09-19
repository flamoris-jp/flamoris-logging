namespace Flamoris.Logging.Internal;

internal sealed record NormalizedConfiguration(
    LogLevel Level,
    IReadOnlyDictionary<string, LogLevel> Categories,
    IReadOnlyList<NormalizedOutput> Outputs,
    bool UseLocalTime,
    IReadOnlySet<string> RedactedPropertyNames,
    IReadOnlyList<string> Warnings);

internal sealed record NormalizedOutput(
    string Type, string? Path, bool RotationEnabled, int MaxFileSizeMb, int MaxFiles);
