namespace Flamoris.Logging;

public enum LogLevel { Error = 0, Warn = 1, Info = 2, Debug = 3 }

internal static class LogLevelParser
{
    public static bool TryParse(string? value, out LogLevel level)
    {
        switch (value?.Trim().ToLowerInvariant())
        {
            case "error": level = LogLevel.Error; return true;
            case "warn": level = LogLevel.Warn; return true;
            case "info": level = LogLevel.Info; return true;
            case "debug": level = LogLevel.Debug; return true;
            default: level = LogLevel.Debug; return false;
        }
    }
}
