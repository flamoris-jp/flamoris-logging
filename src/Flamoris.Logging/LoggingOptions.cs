namespace Flamoris.Logging;

public sealed class LoggingOptions
{
    public string? Level { get; set; } = "debug";
    public IDictionary<string, string?> Categories { get; set; } =
        new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
    public IList<LogOutputOptions> Outputs { get; set; } =
    [
        new() { Type = "console" },
        new() { Type = "file", Path = "logs/flamoris.log" },
    ];
    public bool UseLocalTime { get; set; } = true;
    public IList<string> RedactedPropertyNames { get; set; } = [];
}

public sealed class LogOutputOptions
{
    public string? Type { get; set; }
    public string? Path { get; set; }
    public string? Format { get; set; } = "text";
    public LogRotationOptions Rotation { get; set; } = new();
}

public sealed class LogRotationOptions
{
    public bool Enabled { get; set; } = true;
    public int MaxFileSizeMb { get; set; } = 20;
    public int MaxFiles { get; set; } = 10;
}
