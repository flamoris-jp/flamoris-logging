namespace Flamoris.Logging.Internal;

internal static class SensitiveDataRedactor
{
    internal static readonly string[] DefaultPropertyNames =
    [
        "authorization", "credential", "credentials", "password", "privateKey",
        "secret", "token", "accessToken", "refreshToken", "apiKey",
    ];

    public static IReadOnlyDictionary<string, object?> Redact(
        IReadOnlyDictionary<string, object?>? properties, IReadOnlySet<string> names)
    {
        if (properties is null || properties.Count == 0)
            return new Dictionary<string, object?>();
        var copy = new Dictionary<string, object?>(properties.Count, StringComparer.Ordinal);
        foreach (var property in properties)
            copy[property.Key] = names.Contains(property.Key) ? "[REDACTED]" : property.Value;
        return copy;
    }
}
