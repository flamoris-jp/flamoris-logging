namespace Flamoris.Logging.Internal;

internal static class ConfigurationNormalizer
{
    public static NormalizedConfiguration Normalize(LoggingOptions? source)
    {
        source ??= new LoggingOptions();
        var warnings = new List<string>();
        if (!LogLevelParser.TryParse(source.Level, out var global))
            warnings.Add($"Invalid global level '{source.Level ?? "<null>"}'; using debug.");

        var categories = new Dictionary<string, LogLevel>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in source.Categories ?? new Dictionary<string, string?>())
        {
            var category = entry.Key?.Trim().Trim('.');
            if (string.IsNullOrWhiteSpace(category))
            {
                warnings.Add("Ignored an empty category override.");
                continue;
            }
            if (!LogLevelParser.TryParse(entry.Value, out var level))
                warnings.Add($"Invalid level '{entry.Value ?? "<null>"}' for category '{category}'; using debug.");
            categories[category] = level;
        }

        var outputs = new List<NormalizedOutput>();
        foreach (var output in source.Outputs ?? [])
        {
            if (output is null) { warnings.Add("Ignored a null output configuration."); continue; }
            var type = output.Type?.Trim().ToLowerInvariant();
            if (type is not ("console" or "file"))
            {
                warnings.Add($"Ignored unknown output type '{output.Type ?? "<null>"}'.");
                continue;
            }
            if (!string.Equals(output.Format?.Trim() ?? "text", "text", StringComparison.OrdinalIgnoreCase))
                warnings.Add($"Unsupported format '{output.Format}' for {type}; using text.");
            var rotation = output.Rotation ?? new LogRotationOptions();
            var maxSize = rotation.MaxFileSizeMb;
            if (maxSize <= 0) { warnings.Add($"Invalid maxFileSizeMb '{maxSize}'; using 20."); maxSize = 20; }
            var maxFiles = rotation.MaxFiles;
            if (maxFiles <= 0) { warnings.Add($"Invalid maxFiles '{maxFiles}'; using 10."); maxFiles = 10; }
            var path = output.Path;
            if (type == "file" && string.IsNullOrWhiteSpace(path))
            {
                path = "logs/flamoris.log";
                warnings.Add("File output path was empty; using logs/flamoris.log.");
            }
            outputs.Add(new(type, path, rotation.Enabled, maxSize, maxFiles));
        }
        if (outputs.Count == 0)
        {
            warnings.Add("No valid outputs were configured; using console output.");
            outputs.Add(new("console", null, false, 20, 10));
        }

        var redacted = SensitiveDataRedactor.DefaultPropertyNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var name in source.RedactedPropertyNames ?? [])
            if (!string.IsNullOrWhiteSpace(name)) redacted.Add(name.Trim());
        return new(global, categories, outputs, source.UseLocalTime, redacted, warnings);
    }
}
