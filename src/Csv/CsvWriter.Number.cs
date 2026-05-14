using System.Buffers.Text;
using System.Runtime.CompilerServices;

namespace Csv;

partial struct CsvWriter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSByte(sbyte value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(6);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(4); // -128
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteByte(byte value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(5);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(3); // 255
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt16(short value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(8);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(6); // -32768
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt32(int value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(13);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(11); // -2147483648
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteInt64(long value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(22);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(20); // -9223372036854775808
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt16(ushort value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(7);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(5); // 65535
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt32(uint value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(12);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(10); // 4294967295
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteUInt64(ulong value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(22);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(20); // 18446744073709551615
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSingle(float value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(22);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(20);
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDouble(double value)
    {
        const int MaxLength = 24; // for writing double.MinValue (-1.7976931348623157E+308)

        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(MaxLength + 2);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(MaxLength);
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDecimal(decimal value)
    {
        if (options.QuoteMode == QuoteMode.All)
        {
            var span = GetSpan(32);
            span[0] = (byte)'"';
            if (!Utf8Formatter.TryFormat(value, span[1..], out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            span[bytesWritten + 1] = (byte)'"';
            Advance(bytesWritten + 2);
        }
        else
        {
            var span = GetSpan(30); // -79228162514264337593543950335
            if (!Utf8Formatter.TryFormat(value, span, out var bytesWritten))
            {
                CsvSerializationException.ThrowFailedEncoding(value);
            }
            Advance(bytesWritten);
        }
    }

}