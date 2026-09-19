using System.Runtime.InteropServices;
using System.Text;

namespace Flamoris.Logging.Formatting;

/// <summary>Buffers at most a configured number of UTF-8 bytes.</summary>
internal sealed class BoundedUtf8TextWriter(long maximumBytes) : TextWriter
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false);
    private readonly List<byte> bytes = new((int)Math.Min(maximumBytes, 4_096));
    private readonly long maximumBytes = maximumBytes;
    private char? pendingHighSurrogate;

    public override Encoding Encoding => Utf8;
    public bool IsTruncated { get; private set; }
    public byte[] ToArray() => bytes.ToArray();

    public override void Write(char value)
    {
        if (IsTruncated) return;

        if (pendingHighSurrogate is char highSurrogate)
        {
            pendingHighSurrogate = null;
            if (char.IsLowSurrogate(value))
            {
                WriteScalar(highSurrogate, value);
                return;
            }

            WriteReplacementCharacter();
            if (IsTruncated) return;
        }

        if (char.IsHighSurrogate(value))
        {
            pendingHighSurrogate = value;
            return;
        }

        if (char.IsLowSurrogate(value))
        {
            WriteReplacementCharacter();
            return;
        }

        WriteScalar(value);
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

    public void Complete()
    {
        if (pendingHighSurrogate is not null && !IsTruncated)
        {
            pendingHighSurrogate = null;
            WriteReplacementCharacter();
        }
    }

    private void WriteScalar(char value)
    {
        Span<byte> encoded = stackalloc byte[4];
        var written = Utf8.GetBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1), encoded);
        Append(encoded[..written]);
    }

    private void WriteScalar(char highSurrogate, char lowSurrogate)
    {
        Span<char> scalar = stackalloc char[2] { highSurrogate, lowSurrogate };
        Span<byte> encoded = stackalloc byte[4];
        var written = Utf8.GetBytes(scalar, encoded);
        Append(encoded[..written]);
    }

    private void WriteReplacementCharacter()
    {
        Span<byte> replacement = stackalloc byte[] { 0xEF, 0xBF, 0xBD };
        Append(replacement);
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
