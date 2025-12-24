using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Csv.Internal;

namespace Csv;

[StructLayout(LayoutKind.Auto)]
public ref partial struct CsvReader
{
    SequenceReader<byte> reader;
    readonly CsvOptions options;
    readonly byte separator;

    public readonly CsvOptions Options => options;
    public readonly long Consumed => reader.Consumed;
    public readonly long Remaining => reader.Remaining;

    public CsvReader(in ReadOnlySequence<byte> sequence, CsvOptions options)
    {
        reader = new(sequence);
        this.options = options;
        separator = options.Separator.ToUtf8();
    }

    public bool ReadBoolean()
    {
        return BitConverter.IsLittleEndian
            ? ReadBooleanLittleEndian()
            : ReadBooleanBigEndian();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    bool ReadBooleanLittleEndian()
    {
        if (IsNextSeparatorOrNewline()) return default;

        TrySkipQuotation();

        if (reader.TryRead(out int intValue))
        {
            switch (intValue)
            {
                case CsvConstants.trueBytesLittleEndian:
                case CsvConstants.TrueBytesLittleEndian:
                case CsvConstants.TRUEBytesLittleEndian:
                    return true;
                case CsvConstants.falseBytesLittleEndian:
                case CsvConstants.FalseBytesLittleEndian:
                    if (reader.TryRead(out var c) && c is (byte)'e') return false;
                    break;
                case CsvConstants.FALSEBytesLittleEndian:
                    if (reader.TryRead(out var c2) && c2 is (byte)'E') return false;
                    break;
            }
        }

        CsvSerializationException.ThrowFailedEncoding();
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    bool ReadBooleanBigEndian()
    {
        if (IsNextSeparatorOrNewline()) return default;

        TrySkipQuotation();

        if (reader.TryRead(out int intValue))
        {
            switch (intValue)
            {
                case CsvConstants.TRUEBytesBigEndian:
                case CsvConstants.TrueBytesBigEndian:
                case CsvConstants.trueBytesBigEndian:
                    return true;
                case CsvConstants.FALSEBytesBigEndian:
                case CsvConstants.FalseBytesBigEndian:
                    if (reader.TryRead(out var c) && c is (byte)'E') return false;
                    break;
                case CsvConstants.falseBytesBigEndian:
                    if (reader.TryRead(out var c2) && c2 is (byte)'e') return false;
                    break;
            }
        }

        CsvSerializationException.ThrowFailedEncoding();
        return false;
    }

    public char ReadChar()
    {
        // TODO: optimize

        var str = ReadString();
        if (str == null || str.Length != 1) CsvSerializationException.ThrowFailedEncoding();
        return str[0];
    }

    public string? ReadString()
    {
        if (IsNextSeparatorOrNewline()) return null;

        var startFromQuotation = TrySkipQuotation(false);

        if (!startFromQuotation)
        {
#if NET7_0_OR_GREATER
            scoped Span<byte> delimitersSpan = [(byte)'\r', (byte)'\n', separator];
#else
            byte[] delimiters = [(byte)'\r', (byte)'\n', separator];
            Span<byte> delimitersSpan = delimiters.AsSpan(0, 3);
#endif

            var remaining = reader.Remaining;
            if (reader.CurrentSpan.Length - reader.CurrentSpanIndex == remaining)
            {
                if (!reader.TryReadToAny(out ReadOnlySpan<byte> span, delimitersSpan, false))
                {
                    span = reader.UnreadSpan;
#if NET5_0_OR_GREATER
                    reader.AdvanceToEnd();
#else
                    reader.Advance(remaining);
#endif
                }

                if (span.Length == 0) return null;
                return Encoding.UTF8.GetString(span);
            }
            else
            {
                if (!reader.TryReadToAny(out ReadOnlySequence<byte> sequence, delimitersSpan, false))
                {
                    sequence = reader.Sequence.Slice(reader.Consumed);
#if NET5_0_OR_GREATER
                    reader.AdvanceToEnd();
#else
                    reader.Advance(remaining);
#endif
                }

                var length = (int)sequence.Length;
                if (length == 0) return null;
#if NET5_0_OR_GREATER
                return Encoding.UTF8.GetString(sequence);
#else
                var buffer = ArrayPool<byte>.Shared.Rent(length);
                try
                {
                    var span = new Span<byte>(buffer, 0, length);
                    sequence.CopyTo(span);
                    return Encoding.UTF8.GetString(span);
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
#endif
            }
        }
        else
        {
            using var buffer = new TempList<byte>(64);

            while (reader.TryRead(out var current))
            {
                // escape '"'
                if (current == '"')
                {
                    if (!reader.TryPeek(out var c2)) goto RETURN;
                    if (c2 != '"') goto RETURN;

                    buffer.Add(c2);
                    reader.Advance(1);
                    continue;
                }

                buffer.Add(current);
            }

        RETURN:
            return Encoding.UTF8.GetString(buffer.AsSpan());
        }
    }

    public int SkipField()
    {
        if (IsNextSeparatorOrNewline())
        {
            return 0;
        }

        var consumed = reader.Consumed;
        var startFromQuotation = TrySkipQuotation(false);

        while (reader.TryRead(out var c1))
        {
            // escape '"'
            if (startFromQuotation)
            {
                if (c1 == '"')
                {
                    if (!reader.TryPeek(out var c2)) goto RETURN;
                    if (c2 != '"') goto RETURN;

                    reader.Advance(1);
                    continue;
                }
            }
            else if (c1 == (byte)'\n' || c1 == (byte)'\r' || c1 == separator)
            {
                reader.Rewind(1);
                goto RETURN;
            }
        }

    RETURN:
        return (int)(reader.Consumed - consumed);
    }

    public int ReadUtf8(ref TempList<byte> buffer)
    {
        if (IsNextSeparatorOrNewline())
        {
            return 0;
        }

        var consumed = reader.Consumed;

        var startFromQuotation = TrySkipQuotation(false);

        while (reader.TryRead(out var c1))
        {
            // escape '"'
            if (startFromQuotation)
            {
                if (c1 == '"')
                {
                    if (!reader.TryPeek(out var c2)) goto RETURN;
                    if (c2 != '"') goto RETURN;

                    buffer.Add(c2);
                    reader.Advance(1);
                    continue;
                }
            }
            else if (c1 == (byte)'\n' || c1 == (byte)'\r' || c1 == separator)
            {
                reader.Rewind(1);
                goto RETURN;
            }

            buffer.Add(c1);
        }

    RETURN:
        return (int)(reader.Consumed - consumed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadSeparator(bool skipWhiteSpace = true)
    {
        if (skipWhiteSpace) SkipWhitespace();

        if (!reader.TryPeek(out var current)) return false;
        if (current != separator) return false;

        reader.Advance(1);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadSeparator()
    {
        if (!TryReadSeparator()) CsvSerializationException.ThrowMessage("Parsing failed.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryReadEndOfLine(bool skipWhiteSpace = true)
    {
        if (skipWhiteSpace) SkipWhitespace();

        if (!reader.TryPeek(out var c1) || c1 is not ((byte)'\n' or (byte)'\r')) return false;
        reader.Advance(1);

        if (c1 == '\r' && reader.TryPeek(out var c2) && c2 == '\n')
        {
            reader.Advance(1);
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReadEndOfLine()
    {
        if (!TryReadEndOfLine()) CsvSerializationException.ThrowMessage("Parsing failed.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySkipComment(bool skipWhiteSpace = true)
    {
        if (skipWhiteSpace) SkipWhitespace();
        var isComment = reader.IsNext((byte)'#', true);
        if (isComment) SkipLine();

        return isComment;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipLine()
    {
        if (!reader.TryAdvanceToAny([(byte)'\n', (byte)'\r'], false))
        {
#if NETSTANDARD2_1
            reader.Advance(reader.Remaining);
#else
            reader.AdvanceToEnd();
#endif
        }

        reader.TryRead(out var c1);
        if (c1 == '\r' && reader.TryPeek(out var c2) && c2 == '\n')
        {
            reader.Advance(1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SkipWhitespace()
    {
        reader.AdvancePast((byte)' ');
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool TrySkipQuotation(bool skipWhiteSpace = true)
    {
        if (skipWhiteSpace) SkipWhitespace();
        return reader.IsNext((byte)'"', true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsNextSeparatorOrNewline(bool skipWhiteSpace = true)
    {
        if (skipWhiteSpace) SkipWhitespace();

        if (!reader.TryPeek(out var c)) return true;
        return c == separator || (c is (byte)'\r' or (byte)'\n');
    }
}