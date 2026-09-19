using System.Runtime.InteropServices;
using System.Text;

namespace Flamoris.Logging.Formatting;

/// <summary>Buffers at most a configured number of UTF-8 bytes.</summary>
internal sealed class BoundedUtf8TextWriter(long maximumBytes) : TextWriter
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);
    private readonly List<byte> bytes = new((int)Math.Min(maximumBytes, 4_096));
    private readonly long maximumBytes = maximumBytes;

    public override Encoding Encoding => Utf8;
    public bool IsTruncated { get; private set; }
    public byte[] ToArray() => bytes.ToArray();

    public override void Write(char value)
    {
        if (IsTruncated) return;
        Span<byte> encoded = stackalloc byte[4];
        var written = Utf8.GetBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1), encoded);
        Append(encoded[..written]);
    }

    public override void Write(string? value)
    {
        if (string.IsNullOrEmpty(value) || IsTruncated) return;
        foreach (var character in value)
        {
            Write(character);
            if (IsTruncated) return;
        }
    }

    private void Append(ReadOnlySpan<byte> value)
    {
        if (bytes.Count + value.Length > maximumBytes)
        {
            IsTruncated = true;
            return;
        }
        foreach (var item in value) bytes.Add(item);
    }
}
