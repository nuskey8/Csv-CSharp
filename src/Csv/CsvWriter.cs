using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Csv.Internal;

namespace Csv;

[StructLayout(LayoutKind.Auto)]
public ref partial struct CsvWriter
{
    public CsvWriter(IBufferWriter<byte> bufferWriter, CsvOptions options)
    {
        this.writer = bufferWriter;
        this.options = options;

        separator = options.Separator.ToUtf8();
        newLine = options.NewLine.ToUtf8();
    }

    readonly IBufferWriter<byte> writer;
    readonly CsvOptions options;
    readonly byte separator;
    readonly ReadOnlySpan<byte> newLine;

    public IBufferWriter<byte> BufferWriter => writer;
    public CsvOptions Options => options;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint)
    {
        return writer.GetSpan(sizeHint);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        writer.Advance(count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteRaw(byte code)
    {
        GetSpan(1)[0] = code;
        Advance(1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteRaw(scoped ReadOnlySpan<byte> bytes)
    {
        var span = GetSpan(bytes.Length);
        bytes.CopyTo(span);
        Advance(bytes.Length);
    }

    public void WriteBoolean(bool value)
    {
        if (BitConverter.IsLittleEndian) WriteBooleanLittleEndian(value);
        else WriteBooleanBigEndian(value);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    void WriteBooleanBigEndian(bool value)
    {
        if (options.QuoteMode is QuoteMode.Minimal or QuoteMode.None)
        {
            if (value)
            {
                var span = GetSpan(4);
                ref var first = ref MemoryMarshal.GetReference(span);
                Unsafe.WriteUnaligned(ref first, CsvConstants.trueBytesBigEndian);
                Advance(4);
            }
            else
            {
                var span = GetSpan(5);
                ref var first = ref MemoryMarshal.GetReference(span);
                Unsafe.WriteUnaligned(ref first, CsvConstants.falseBytesBigEndian);
                Unsafe.Add(ref first, 4) = (byte)'e';
                Advance(5);
            }
        }
        else
        {
            if (value)
            {
                var span = GetSpan(6);
                ref var first = ref MemoryMarshal.GetReference(span);
                Unsafe.WriteUnaligned(ref first, ((byte)'"' << 24) | (CsvConstants.trueBytesBigEndian >> 8));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref first, 4), (short)((byte)'e' << 8 | ((byte)'"')));
                Advance(6);
            }
            else
            {
                var span = GetSpan(7);
                ref var first = ref MemoryMarshal.GetReference(span);
                Unsafe.WriteUnaligned(ref first, ((byte)'"' << 24) | (CsvConstants.falseBytesBigEndian >> 8));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref first, 4), (short)((byte)'s' << 8 | ((byte)'e')));
                Unsafe.Add(ref first, 6) = (byte)'"';
                Advance(7);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    void WriteBooleanLittleEndian(bool value)
    {
        if (options.QuoteMode is QuoteMode.Minimal or QuoteMode.None)
        {
            if (value)
            {
                var span = GetSpan(4);
                ref var first = ref MemoryMarshal.GetReference(span);
                Unsafe.As<byte, int>(ref first) = CsvConstants.trueBytesLittleEndian;
                Advance(4);
            }
            else
            {
                var span = GetSpan(5);
                ref var first = ref MemoryMarshal.GetReference(span);
                Unsafe.As<byte, int>(ref first) = CsvConstants.falseBytesLittleEndian;
                Unsafe.Add(ref first, 4) = (byte)'e';
                Advance(5);
            }
        }
        else
        {
            if (value)
            {
                var span = GetSpan(6);
                ref var first = ref MemoryMarshal.GetReference(span);
                Unsafe.WriteUnaligned(ref first, ((byte)'"') | (CsvConstants.trueBytesLittleEndian << 8));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref first, 4), (short)((byte)'e' | ((byte)'"' << 8)));
                Advance(6);
            }
            else
            {
                var span = GetSpan(7);
                ref var first = ref MemoryMarshal.GetReference(span);
                Unsafe.WriteUnaligned(ref first, ((byte)'"') | (CsvConstants.falseBytesLittleEndian << 8));
                Unsafe.WriteUnaligned(ref Unsafe.Add(ref first, 4), (short)((byte)'s' | ((byte)'e' << 8)));
                Unsafe.Add(ref first, 6) = (byte)'"';
                Advance(7);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteChar(char value)
    {
        WriteString([value]);
    }

    public void WriteString(scoped ReadOnlySpan<char> value)
    {
        // TODO: optimize

        var shouldQuote = options.QuoteMode is not (QuoteMode.Minimal or QuoteMode.None);
        var separator = (char)this.separator;
#if NET8_0_OR_GREATER
        shouldQuote |= value.ContainsAny(['"', '\n', '\r', separator]);
#else
        for (int i = 0; i < value.Length; i++)
        {
            var item = value[i];
            shouldQuote |= item is '"' or '\n' or '\r';
            shouldQuote |= item == separator;
        }
#endif

        var max = Encoding.UTF8.GetMaxByteCount(value.Length);
        if (shouldQuote) max += 2;

        var span = GetSpan(max);

        var offset = 0;
        var from = 0;

        if (shouldQuote)
        {
            span[offset++] = (byte)'"';
        }

        for (int i = 0; i < value.Length; i++)
        {
            byte escapeChar;
            switch (value[i])
            {
                case '"':
                    escapeChar = (byte)'"';
                    break;
                default:
                    continue;
            }

            max++;
            if (span.Length < max) span = GetSpan(max);

            offset += Encoding.UTF8.GetBytes(value[from..i], span[offset..]);
            from = i + 1;
            span[offset++] = (byte)'"';
            span[offset++] = escapeChar;
        }

        if (from != value.Length)
        {
            offset += Encoding.UTF8.GetBytes(value[from..], span[offset..]);
        }

        if (shouldQuote)
        {
            span[offset++] = (byte)'"';
        }

        Advance(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSeparator()
    {
        WriteRaw(separator);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteEndOfLine()
    {
        WriteRaw(newLine);
    }
}