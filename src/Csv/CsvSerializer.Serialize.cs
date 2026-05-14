using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace Csv;

public static partial class CsvSerializer
{
    public static byte[] Serialize<T>(T[] values, CsvOptions? options = default)
    {
        return Serialize<T>(values.AsSpan(), options);
    }

    public static byte[] Serialize<T>(ReadOnlySpan<T> values, CsvOptions? options = default)
    {
        var bufferWriter = SharedBufferWriter.GetWriter();
        try
        {
            Serialize(bufferWriter, values, options);
            return bufferWriter.WrittenSpan.ToArray();
        }
        finally
        {
            bufferWriter.Reset();
        }
    }

    public static byte[] Serialize<T>(IEnumerable<T> values, CsvOptions? options = default)
    {
        if (values is T[] array) return Serialize<T>(array.AsSpan(), options);
#if NET5_0_OR_GREATER
        if (values is List<T> list) return Serialize<T>(CollectionsMarshal.AsSpan(list), options);
#endif

        var bufferWriter = SharedBufferWriter.GetWriter();
        try
        {
            Serialize(bufferWriter, values, options);
            return bufferWriter.WrittenSpan.ToArray();
        }
        finally
        {
            bufferWriter.Reset();
        }
    }

    public static void Serialize<T>(IBufferWriter<byte> bufferWriter, ReadOnlySpan<T> values, CsvOptions? options = default)
    {
        options ??= DefaultOptions;

        var serializer = GetSerializerWithVerify<T>();
        var writer = new CsvWriter(bufferWriter, options);

        serializer.Serialize(ref writer, values);
    }

    public static void Serialize<T>(IBufferWriter<byte> bufferWriter, IEnumerable<T> values, CsvOptions? options = default)
    {
        if (values is T[] array)
        {
            Serialize<T>(bufferWriter, array.AsSpan(), options);
            return;
        }
#if NET5_0_OR_GREATER
        if (values is List<T> list)
        {
            Serialize<T>(bufferWriter, CollectionsMarshal.AsSpan(list), options);
            return;
        }
#endif

        options ??= DefaultOptions;

        var serializer = GetSerializerWithVerify<T>();
        var writer = new CsvWriter(bufferWriter, options);

        serializer.Serialize(ref writer, values);
    }

    public static void Serialize<T>(Stream stream, ReadOnlySpan<T> values, CsvOptions? options = default)
    {
        var writer = SharedBufferWriter.GetWriter();
        try
        {
            Serialize(writer, values, options);
            stream.Write(writer.WrittenSpan);
            stream.Flush();
        }
        finally
        {
            writer.Reset();
        }
    }

    public static void Serialize<T>(Stream stream, IEnumerable<T> values, CsvOptions? options = default)
    {
        if (values is T[] array)
        {
            Serialize<T>(stream, array.AsSpan(), options);
            return;
        }
#if NET5_0_OR_GREATER
        if (values is List<T> list)
        {
            Serialize<T>(stream, CollectionsMarshal.AsSpan(list), options);
            return;
        }
#endif

        var writer = SharedBufferWriter.GetWriter();
        try
        {
            Serialize(writer, values, options);
            stream.Write(writer.WrittenSpan);
            stream.Flush();
        }
        finally
        {
            writer.Reset();
        }
    }

    public static async ValueTask SerializeAsync<T>(Stream stream, ReadOnlyMemory<T> values, CsvOptions? options = default, CancellationToken cancellationToken = default)
    {
        var writer = SharedBufferWriter.GetWriter();
        try
        {
            Serialize(writer, values.Span, options);
            await stream.WriteAsync(writer.WrittenMemory, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            writer.Reset();
        }
    }

    public static ValueTask SerializeAsync<T>(Stream stream, IEnumerable<T> values, CsvOptions? options = default, CancellationToken cancellationToken = default)
    {
        if (values is T[] array) return SerializeAsync<T>(stream, array.AsMemory(), options, cancellationToken);
        return SerializeAsyncCore(stream, values, options, cancellationToken);
    }

    static async ValueTask SerializeAsyncCore<T>(Stream stream, IEnumerable<T> values, CsvOptions? options = default, CancellationToken cancellationToken = default)
    {
        var writer = SharedBufferWriter.GetWriter();
        try
        {
            Serialize(writer, values, options);
            await stream.WriteAsync(writer.WrittenMemory, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            writer.Reset();
        }
    }

    public static string SerializeToString<T>(T[] values, CsvOptions? options = default)
    {
        return SerializeToString<T>(values.AsSpan(), options);
    }

    public static string SerializeToString<T>(ReadOnlySpan<T> values, CsvOptions? options = default)
    {
        return Encoding.UTF8.GetString(Serialize(values, options));
    }

    public static string SerializeToString<T>(IEnumerable<T> values, CsvOptions? options = default)
    {
        return Encoding.UTF8.GetString(Serialize(values, options));
    }
}
