using System.Text;
using Flamoris.Logging.Formatting;

namespace Flamoris.Logging.Tests;

public sealed class FormattingTests
{
    [Fact]
    public void Text_format_is_deterministic_and_structured()
    {
        var formatter = new TextLogFormatter(false);
        using var writer = new StringWriter();
        formatter.Write(new LogEvent(
            new DateTimeOffset(2026, 9, 19, 11, 24, 31, 482, TimeSpan.Zero),
            LogLevel.Debug, "mcp.transport", "Connected",
            new Dictionary<string, object?> { ["session"] = "abc 123", ["client"] = "chatgpt" }), writer);
        var value = writer.ToString();
        Assert.Equal("2026-09-19 11:24:31.482 +00:00 [DEBUG] [mcp.transport] Connected client=chatgpt session=\"abc 123\"", value);
    }

    [Fact]
    public void Exception_type_message_and_stack_are_rendered()
    {
        Exception exception;
        try { ThrowForStack(); throw new InvalidOperationException(); }
        catch (Exception caught) { exception = caught; }
        using var writer = new StringWriter();
        new TextLogFormatter(false).Write(new LogEvent(
            DateTimeOffset.UnixEpoch, LogLevel.Error, "render", "Preview failed",
            new Dictionary<string, object?>(), exception), writer);
        var text = writer.ToString();
        Assert.Contains("System.InvalidOperationException: broken", text);
        Assert.Contains(nameof(ThrowForStack), text);
    }

    [Fact]
    public void Newlines_in_primary_fields_are_escaped()
    {
        using var writer = new StringWriter();
        new TextLogFormatter(false).Write(new LogEvent(
            DateTimeOffset.UnixEpoch, LogLevel.Info, "app\nforged", "line1\r\nline2",
            new Dictionary<string, object?> { ["value"] = "a\nb" }), writer);
        var text = writer.ToString();
        Assert.DoesNotContain("app\nforged", text);
        Assert.Contains("app\\nforged", text);
        Assert.Contains("line1\\r\\nline2", text);
    }

    [Fact]
    public void Bounded_utf8_writer_preserves_surrogate_pairs_and_rejects_partial_scalars()
    {
        var complete = new BoundedUtf8TextWriter(4);
        complete.Write("🐱");
        complete.Complete();
        Assert.False(complete.IsTruncated);
        Assert.Equal("🐱", Encoding.UTF8.GetString(complete.ToArray()));

        var boundary = new BoundedUtf8TextWriter(3);
        boundary.Write("🐱");
        boundary.Complete();
        Assert.True(boundary.IsTruncated);
        Assert.Empty(boundary.ToArray());
    }

    private static void ThrowForStack() => throw new InvalidOperationException("broken");
}
