using System.Buffers;
using System.Text;

namespace Csv;

// TODO: support ReadOnlySpan<byte>

public static partial class CsvSerializer
{
    public static T[] Deserialize<T>(byte[] bytes, CsvOptions? options = default)
    {
        return Deserialize<T>(new ReadOnlySequence<byte>(bytes), options);
    }

    public static int Deserialize<T>(byte[] bytes, Span<T> destination, CsvOptions? options = default)
    {
        return Deserialize(new ReadOnlySequence<byte>(bytes), destination, options);
    }

    public static T[] Deserialize<T>(ReadOnlyMemory<byte> bytes, CsvOptions? options = default)
    {
        return Deserialize<T>(new ReadOnlySequence<byte>(bytes), options);
    }

    public static int Deserialize<T>(ReadOnlyMemory<byte> bytes, Span<T> destination, CsvOptions? options = default)
    {
        return Deserialize(new ReadOnlySequence<byte>(bytes), destination, options);
    }

    public static T[] Deserialize<T>(in ReadOnlySequence<byte> bytes, CsvOptions? options = default)
    {
        options ??= DefaultOptions;

        var serializer = GetSerializerWithVerify<T>();
        var reader = new CsvReader(bytes, options);

        return serializer.Deserialize(ref reader);
    }

    public static int Deserialize<T>(in ReadOnlySequence<byte> bytes, Span<T> destination, CsvOptions? options = default)
    {
        options ??= DefaultOptions;

        var serializer = GetSerializerWithVerify<T>();
        var reader = new CsvReader(bytes, options);

        return serializer.Deserialize(ref reader, destination);
    }

    public static T[] Deserialize<T>(string str, CsvOptions? options = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, buffer);
        try
        {
            return Deserialize<T>(new ReadOnlySequence<byte>(buffer), options);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static int Deserialize<T>(string str, Span<T> destination, CsvOptions? options = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(str));
        Encoding.UTF8.GetBytes(str, buffer);
        try
        {
            return Deserialize(new ReadOnlySequence<byte>(buffer), destination, options);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public static T[] Deserialize<T>(Stream stream, CsvOptions? options = default)
    {
        options ??= DefaultOptions;

        if (TryDeserializeFromMemoryStream<T>(stream, options, out var result))
        {
            return result!;
        }

        var builder = SharedSequenceBuilder.GetBuilder();
        try
        {
            BuildSequenceFromStream(stream, builder);
            var sequence = builder.Build();
            return Deserialize<T>(sequence, options);
        }
        finally
        {
            builder.Reset();
        }
    }

    public static int Deserialize<T>(Stream stream, Span<T> destination, CsvOptions? options = default)
    {
        options ??= DefaultOptions;

        if (TryDeserializeFromMemoryStream(stream, destination, options, out var result))
        {
            return result!;
        }

        var builder = SharedSequenceBuilder.GetBuilder();
        try
        {
            BuildSequenceFromStream(stream, builder);
            var sequence = builder.Build();
            return Deserialize(sequence, destination, options);
        }
        finally
        {
            builder.Reset();
        }
    }
    
    public static async ValueTask<T[]> DeserializeAsync<T>(Stream stream, CsvOptions? options = default, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        options ??= DefaultOptions;

        if (TryDeserializeFromMemoryStream<T>(stream, options, out var result))
        {
            return result!;
        }

        var builder = SharedSequenceBuilder.GetBuilder();
        try
        {
            await BuildSequenceFromStreamAsync(stream, builder, cancellationToken);
            var sequence = builder.Build();
            return Deserialize<T>(sequence, options);
        }
        finally
        {
            builder.Reset();
        }
    }
    public static async ValueTask<int> DeserializeAsync<T>(Stream stream, Memory<T> destination, CsvOptions? options = default, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        options ??= DefaultOptions;

        if (TryDeserializeFromMemoryStream(stream, destination.Span, options, out var result))
        {
            return result!;
        }

        var builder = SharedSequenceBuilder.GetBuilder();
        try
        {
            await BuildSequenceFromStreamAsync(stream, builder, cancellationToken);
            var sequence = builder.Build();
            return Deserialize(sequence, destination.Span, options);
        }
        finally
        {
            builder.Reset();
        }
    }

    static bool TryDeserializeFromMemoryStream<T>(Stream stream, CsvOptions options, out T[]? result)
    {
        if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> streamBuffer))
        {
            var serializer = GetSerializerWithVerify<T>();
            var reader = new CsvReader(new ReadOnlySequence<byte>(streamBuffer.AsMemory(checked((int)ms.Position))), options);

            result = serializer.Deserialize(ref reader);

            ms.Seek(reader.Consumed, SeekOrigin.Current);
            return true;
        }

        result = default;
        return false;
    }

    static bool TryDeserializeFromMemoryStream<T>(Stream stream, Span<T> destination, CsvOptions options, out int readCount)
    {
        if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> streamBuffer))
        {
            var serializer = GetSerializerWithVerify<T>();
            var reader = new CsvReader(new ReadOnlySequence<byte>(streamBuffer.AsMemory(checked((int)ms.Position))), options);

            readCount = serializer.Deserialize(ref reader, destination);

            ms.Seek(reader.Consumed, SeekOrigin.Current);
            return true;
        }

        readCount = default;
        return false;
    }

    static void BuildSequenceFromStream(Stream stream, SharedSequenceBuilder builder)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        var offset = 0;
        do
        {
            if (offset == buffer.Length)
            {
                builder.Add(buffer, returnToPool: true);
                buffer = ArrayPool<byte>.Shared.Rent(MathEx.NewArrayCapacity(buffer.Length));
                offset = 0;
            }

            int read;
            try
            {
                read = stream.Read(buffer.AsSpan(offset, buffer.Length - offset));
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw;
            }

            offset += read;

            if (read == 0)
            {
                builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
                break;
            }

        } while (true);
    }

    static async ValueTask BuildSequenceFromStreamAsync(Stream stream, SharedSequenceBuilder builder, CancellationToken cancellationToken)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        var offset = 0;
        do
        {
            if (offset == buffer.Length)
            {
                builder.Add(buffer, returnToPool: true);
                buffer = ArrayPool<byte>.Shared.Rent(MathEx.NewArrayCapacity(buffer.Length));
                offset = 0;
            }

            int read;
            try
            {
                read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken);
            }
            catch
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw;
            }

            offset += read;

            if (read == 0)
            {
                builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
                break;
            }

        } while (true);
    }
}